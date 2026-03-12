using Microsoft.EntityFrameworkCore;
using SRJE.Web.Infrastructure.Data;
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

    public RemuneracionesService(SrjeDbContext db, ILogger<RemuneracionesService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<ArchivoPreviewDto> PreviewRemuneracionesAsync(Stream stream, string nombreArchivo)
    {
        var lineas = RemuneracionesParser.Parsear(stream);

        var rutsExistentes = (await _db.Beneficiarios
            .Select(b => b.RutBeneficiario)
            .ToListAsync())
            .ToHashSet();

        foreach (var linea in lineas)
        {
            if (linea.EstadoLinea == "ERROR") continue;

            if (!rutsExistentes.Contains(linea.RutBeneficiario))
            {
                linea.EstadoLinea = "NUEVO";
                linea.Mensaje = "Beneficiario no existe en BD — se creara al confirmar";
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

            var rutsExistentes = (await _db.Beneficiarios
                .Select(b => b.RutBeneficiario)
                .ToListAsync())
                .ToHashSet();

            var rutsFuncionarios = lineasIncluidas
                .Where(l => l.RutFuncionario.HasValue)
                .Select(l => l.RutFuncionario!.Value)
                .Distinct()
                .ToList();
            var funcionariosExistentes = await _db.Funcionarios
                .Where(f => rutsFuncionarios.Contains(f.RutFuncionario))
                .ToDictionaryAsync(f => f.RutFuncionario);

            // Pre-cargar retenciones existentes para el periodo
            var rutsRetencionKey = lineasIncluidas
                .Select(l => new { l.RutBeneficiario, RutFuncionario = l.RutFuncionario ?? 0 })
                .ToList();
            var retencionesExistentes = await _db.RetenidosJudiciales
                .Where(r => r.PeriodoProceso == request.PeriodoProceso)
                .ToListAsync();
            var retencionesDict = retencionesExistentes
                .ToDictionary(r => (r.RutBeneficiario, r.RutTitular));

            foreach (var linea in request.Lineas)
            {
                if (!linea.Incluir)
                {
                    excluidos++;
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

                    // Upsert retencion (pre-cargado)
                    var key = (linea.RutBeneficiario, linea.RutFuncionario ?? 0);
                    if (retencionesDict.TryGetValue(key, out var retencion))
                    {
                        retencion.Monto = linea.Monto ?? 0;
                        retencion.CodRetencion = linea.CodRetencion;
                        retencion.TipoPago = linea.TipoPago;
                        actualizados++;
                    }
                    else
                    {
                        _db.RetenidosJudiciales.Add(new RetenidoJudicial
                        {
                            IdRetencion = 0,
                            RutTitular = linea.RutFuncionario ?? 0,
                            DvTitular = linea.DvFuncionario ?? "",
                            RutBeneficiario = linea.RutBeneficiario,
                            DvBeneficiario = linea.DvBeneficiario,
                            Monto = linea.Monto ?? 0,
                            CodRetencion = linea.CodRetencion,
                            TipoPago = linea.TipoPago,
                            PeriodoProceso = request.PeriodoProceso
                        });
                        insertados++;
                    }

                    montoTotal += linea.Monto ?? 0;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error procesando linea {NumLinea} RUT {Rut} en remuneraciones",
                        linea.NumeroLinea, linea.RutBeneficiario);
                    errores++;
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
            MontoTotal = lineas.Where(l => l.Monto.HasValue).Sum(l => l.Monto!.Value)
        };
    }
}
