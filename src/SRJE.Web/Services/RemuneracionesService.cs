using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SRJE.Web.Infrastructure.Data;
using SRJE.Web.Models;
using SRJE.Web.Models.Entities;
using SRJE.Web.Models.ViewModels;
using SRJE.Web.Parsers;

namespace SRJE.Web.Services;

public interface IRemuneracionesService
{
    Task<ArchivoPreviewDto> PreviewRemuneracionesAsync(Stream stream, string nombreArchivo);
    Task<ResultadoImportacionDto> ConfirmarRemuneracionesAsync(ConfirmarImportacionRequest request, string usuario, string ip);
}

public class RemuneracionesService : IRemuneracionesService
{
    private readonly SrjeDbContext _db;
    private readonly ILogger<RemuneracionesService> _logger;
    private readonly SrjeSettings _settings;

    public RemuneracionesService(SrjeDbContext db, ILogger<RemuneracionesService> logger, IOptions<SrjeSettings> settings)
    {
        _db = db;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task<ArchivoPreviewDto> PreviewRemuneracionesAsync(Stream stream, string nombreArchivo)
    {
        var lineas = RemuneracionesParser.Parsear(stream);

        // Cargar beneficiarios con datos bancarios
        var rutsArchivo = lineas
            .Where(l => l.EstadoLinea != "ERROR")
            .Select(l => l.RutBeneficiario)
            .Distinct()
            .ToList();
        var beneficiariosDict = await _db.Beneficiarios.AsNoTracking()
            .Where(b => rutsArchivo.Contains(b.RutBeneficiario))
            .ToDictionaryAsync(b => b.RutBeneficiario);

        // Detectar multicuenta: mismo (rutBenef, rutFunc) con más de 1 línea
        var multicuentaKeys = lineas
            .Where(l => l.EstadoLinea != "ERROR")
            .GroupBy(l => (l.RutBeneficiario, RutFunc: l.RutFuncionario ?? 0))
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToHashSet();

        foreach (var linea in lineas)
        {
            if (linea.EstadoLinea == "ERROR") continue;

            if (!beneficiariosDict.ContainsKey(linea.RutBeneficiario))
            {
                linea.EstadoLinea = "NUEVO";
                linea.Mensaje = "Beneficiario no existe en BD — se creara al confirmar";
            }
            else
            {
                // Pre-llenar datos bancarios del beneficiario
                var benef = beneficiariosDict[linea.RutBeneficiario];
                linea.CodBanco = benef.CodBanco;
                linea.TipoCuenta = benef.TipoCuenta;
                linea.NumeroCuenta = benef.CodBanco == _settings.CodBancoEstado
                    ? benef.CtaEstado : benef.CtaOtBanco;
            }

            // Marcar líneas multicuenta
            var key = (linea.RutBeneficiario, RutFunc: linea.RutFuncionario ?? 0);
            if (multicuentaKeys.Contains(key))
            {
                linea.EsMulticuenta = true;
                if (linea.EstadoLinea == "OK")
                {
                    linea.EstadoLinea = "ADVERTENCIA";
                    linea.Mensaje = "Multicuenta: asignar cuenta bancaria manualmente";
                }
            }
        }

        // Detectar beneficiarios con multiples funcionarios titulares
        var multiFuncKeys = lineas
            .Where(l => l.EstadoLinea != "ERROR" && l.RutFuncionario.HasValue)
            .GroupBy(l => l.RutBeneficiario)
            .Where(g => g.Select(l => l.RutFuncionario!.Value).Distinct().Count() > 1)
            .Select(g => g.Key)
            .ToHashSet();

        foreach (var linea in lineas)
        {
            if (multiFuncKeys.Contains(linea.RutBeneficiario) && linea.EstadoLinea != "ERROR")
            {
                if (linea.EstadoLinea == "OK")
                    linea.EstadoLinea = "ADVERTENCIA";
                linea.Mensaje = string.IsNullOrEmpty(linea.Mensaje)
                    ? "Beneficiario con multiples funcionarios titulares"
                    : linea.Mensaje + " | Beneficiario con multiples funcionarios titulares";
            }
        }

        return BuildPreview(lineas);
    }

    public async Task<ResultadoImportacionDto> ConfirmarRemuneracionesAsync(
        ConfirmarImportacionRequest request, string usuario, string ip)
    {
        var inicio = DateTime.Now;
        var logCarga = new LogCarga
        {
            TipoCarga = "REMUNERACIONES",
            NombreArchivo = "Remuneraciones_importacion",
            PeriodoProceso = request.PeriodoProceso,
            Usuario = usuario,
            IpUsuario = ip,
            TotalLineas = request.Lineas.Count
        };

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            _db.LogCargas.Add(logCarga);
            await _db.SaveChangesAsync();

            int insertados = 0, actualizados = 0, excluidos = 0, errores = 0;
            decimal montoTotal = 0;

            var funcionariosEnBatch = new Dictionary<long, Funcionario>();
            var beneficiariosEnBatch = new HashSet<long>();

            // Pre-cargar datos necesarios para evitar N+1
            var lineasIncluidas = request.Lineas.Where(l => l.Incluir).ToList();

            // Validar que líneas multicuenta tengan cuenta bancaria asignada
            var multicuentaSinCuenta = lineasIncluidas
                .Where(l => l.EsMulticuenta && (l.CodBanco == null || string.IsNullOrEmpty(l.NumeroCuenta)))
                .ToList();
            if (multicuentaSinCuenta.Count > 0)
                throw new ArgumentException(
                    $"Hay {multicuentaSinCuenta.Count} lineas multicuenta sin cuenta bancaria asignada. " +
                    "Asigne banco y cuenta a todas las lineas multicuenta antes de confirmar.");

            var rutsBenefLineas = lineasIncluidas
                .Select(l => l.RutBeneficiario)
                .Distinct()
                .ToList();
            var beneficiariosDict = await _db.Beneficiarios.AsNoTracking()
                .Where(b => rutsBenefLineas.Contains(b.RutBeneficiario))
                .ToDictionaryAsync(b => b.RutBeneficiario);
            var rutsExistentes = beneficiariosDict.Keys.ToHashSet();

            var rutsFuncionarios = lineasIncluidas
                .Where(l => l.RutFuncionario.HasValue)
                .Select(l => l.RutFuncionario!.Value)
                .Distinct()
                .ToList();
            var funcionariosExistentes = await _db.Funcionarios
                .Where(f => rutsFuncionarios.Contains(f.RutFuncionario))
                .ToDictionaryAsync(f => f.RutFuncionario);

            // Pre-cargar retenciones existentes para el periodo (cola por key para soportar duplicados)
            var retencionesExistentes = await _db.RetenidosJudiciales
                .Where(r => r.PeriodoProceso == request.PeriodoProceso)
                .ToListAsync();
            var retencionesColas = new Dictionary<(long, long), Queue<RetenidoJudicial>>();
            foreach (var r in retencionesExistentes)
            {
                var k = (r.RutBeneficiario, r.RutTitular);
                if (!retencionesColas.ContainsKey(k))
                    retencionesColas[k] = new Queue<RetenidoJudicial>();
                retencionesColas[k].Enqueue(r);
            }

            foreach (var linea in request.Lineas)
            {
                if (!linea.Incluir)
                {
                    excluidos++;
                    _db.LogCargaDetalles.Add(new LogCargaDetalle
                    {
                        IdCarga = logCarga.Id,
                        NumeroLinea = linea.NumeroLinea,
                        RutReferencia = $"{linea.RutBeneficiario}-{linea.DvBeneficiario}",
                        Accion = "EXCLUIR",
                        Estado = "OK"
                    });
                    continue;
                }

                try
                {
                    // Upsert funcionario (pre-cargado)
                    if (linea.RutFuncionario.HasValue &&
                        !funcionariosEnBatch.ContainsKey(linea.RutFuncionario.Value))
                    {
                        if (!funcionariosExistentes.ContainsKey(linea.RutFuncionario.Value))
                        {
                            var funcionario = new Funcionario
                            {
                                RutFuncionario = linea.RutFuncionario.Value,
                                DvFuncionario = linea.DvFuncionario ?? "",
                                Activo = "S"
                            };
                            _db.Funcionarios.Add(funcionario);
                            funcionariosEnBatch[linea.RutFuncionario.Value] = funcionario;
                        }
                        else
                        {
                            funcionariosEnBatch[linea.RutFuncionario.Value] = funcionariosExistentes[linea.RutFuncionario.Value];
                        }
                    }

                    // Crear beneficiario si no existe
                    if (!rutsExistentes.Contains(linea.RutBeneficiario)
                        && !beneficiariosEnBatch.Contains(linea.RutBeneficiario))
                    {
                        _db.Beneficiarios.Add(new Beneficiario
                        {
                            RutBeneficiario = linea.RutBeneficiario,
                            DvBeneficiario = linea.DvBeneficiario,
                            NombreBeneficiario = linea.NombreBeneficiario,
                            UsuarioCreacion = usuario
                        });
                        beneficiariosEnBatch.Add(linea.RutBeneficiario);
                    }

                    // Upsert retencion: cola por key para soportar multiples retenciones mismo par
                    string accion;
                    var key = (linea.RutBeneficiario, linea.RutFuncionario ?? 0);

                    // Resolver datos bancarios: preferir preview (operador), fallback a beneficiario
                    beneficiariosDict.TryGetValue(linea.RutBeneficiario, out var benefBanco);
                    var codBanco = linea.CodBanco ?? benefBanco?.CodBanco;
                    var tipoCuenta = linea.TipoCuenta ?? benefBanco?.TipoCuenta;
                    string? ctaEstado;
                    string? ctaOtBanco;

                    if (!string.IsNullOrEmpty(linea.NumeroCuenta) && codBanco.HasValue)
                    {
                        // Cuenta asignada en preview (manual o pre-llenada)
                        ctaEstado = codBanco == _settings.CodBancoEstado ? linea.NumeroCuenta : null;
                        ctaOtBanco = codBanco != _settings.CodBancoEstado ? linea.NumeroCuenta : null;
                    }
                    else
                    {
                        // Fallback: datos del beneficiario
                        ctaEstado = benefBanco?.CtaEstado;
                        ctaOtBanco = benefBanco?.CtaOtBanco;
                    }

                    if (retencionesColas.TryGetValue(key, out var cola) && cola.Count > 0)
                    {
                        var retencion = cola.Dequeue();
                        retencion.Monto = linea.Monto ?? 0;
                        retencion.CodRetencion = linea.CodRetencion;
                        retencion.TipoPago = linea.TipoPago;
                        retencion.CodBanco = codBanco;
                        retencion.TipoCuenta = tipoCuenta;
                        retencion.CtaEstado = ctaEstado;
                        retencion.CtaOtBanco = ctaOtBanco;
                        actualizados++;
                        accion = "ACTUALIZAR";
                    }
                    else
                    {
                        var nuevaRetencion = new RetenidoJudicial
                        {
                            IdRetencion = linea.NumeroLinea,
                            RutTitular = linea.RutFuncionario ?? 0,
                            DvTitular = linea.DvFuncionario ?? "",
                            RutBeneficiario = linea.RutBeneficiario,
                            DvBeneficiario = linea.DvBeneficiario,
                            Monto = linea.Monto ?? 0,
                            CodRetencion = linea.CodRetencion,
                            TipoPago = linea.TipoPago,
                            PeriodoProceso = request.PeriodoProceso,
                            CodBanco = codBanco,
                            TipoCuenta = tipoCuenta,
                            CtaEstado = ctaEstado,
                            CtaOtBanco = ctaOtBanco
                        };
                        _db.RetenidosJudiciales.Add(nuevaRetencion);
                        insertados++;
                        accion = "INSERTAR";
                    }

                    _db.LogCargaDetalles.Add(new LogCargaDetalle
                    {
                        IdCarga = logCarga.Id,
                        NumeroLinea = linea.NumeroLinea,
                        RutReferencia = $"{linea.RutBeneficiario}-{linea.DvBeneficiario}",
                        Accion = accion,
                        Estado = "OK",
                        Mensajes = linea.Mensaje
                    });

                    montoTotal += linea.Monto ?? 0;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error procesando linea {NumLinea} RUT {Rut} en remuneraciones",
                        linea.NumeroLinea, linea.RutBeneficiario);
                    errores++;
                    _db.LogCargaDetalles.Add(new LogCargaDetalle
                    {
                        IdCarga = logCarga.Id,
                        NumeroLinea = linea.NumeroLinea,
                        RutReferencia = $"{linea.RutBeneficiario}-{linea.DvBeneficiario}",
                        Accion = "ERROR",
                        Estado = "E",
                        Mensajes = ex.Message
                    });
                }
            }

            // Un solo SaveChanges para todo el batch
            logCarga.Estado = "C";
            logCarga.FechaFin = DateTime.Now;
            logCarga.DuracionMs = (long)(logCarga.FechaFin.Value - inicio).TotalMilliseconds;
            logCarga.RegistrosInsertados = insertados;
            logCarga.RegistrosActualizados = actualizados;
            logCarga.RegistrosExcluidos = excluidos;
            logCarga.RegistrosError = errores;
            logCarga.MontoTotal = montoTotal;

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation(
                "Importacion remuneraciones completada: {Insertados} nuevos, {Actualizados} actualizados, {Errores} errores, periodo {Periodo}",
                insertados, actualizados, errores, request.PeriodoProceso);

            return new ResultadoImportacionDto
            {
                IdCarga = logCarga.Id,
                Insertados = insertados,
                Actualizados = actualizados,
                Excluidos = excluidos,
                Errores = errores,
                MontoTotal = montoTotal,
                Mensaje = $"Importacion completada: {insertados} nuevos, {actualizados} actualizados"
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error critico en importacion de remuneraciones, transaccion revertida");
            throw;
        }
    }

    private static ArchivoPreviewDto BuildPreview(List<PreviewLineaDto> lineas)
    {
        return new ArchivoPreviewDto
        {
            Lineas = lineas,
            TotalLineas = lineas.Count,
            LineasOk = lineas.Count(l => l.EstadoLinea == "OK"),
            LineasAdvertencia = lineas.Count(l => l.EstadoLinea == "ADVERTENCIA"),
            LineasError = lineas.Count(l => l.EstadoLinea == "ERROR"),
            LineasNuevas = lineas.Count(l => l.EstadoLinea == "NUEVO"),
            LineasMulticuenta = lineas.Count(l => l.EsMulticuenta),
            MontoTotal = lineas.Where(l => l.Monto.HasValue).Sum(l => l.Monto!.Value)
        };
    }
}
