using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SRJE.Web.Infrastructure.Data;
using SRJE.Web.Models;
using SRJE.Web.Models.Entities;
using SRJE.Web.Models.ViewModels;
using SRJE.Web.Parsers;

namespace SRJE.Web.Services;

public interface ITemgeService
{
    Task<TemgeArchivoDto> PreviewTemgeAsync(Stream stream, string nombreArchivo);
    Task<ResultadoImportacionDto> ConfirmarTemgeAsync(ConfirmarImportacionRequest request, string usuario, string ip);
    Task<TemgeGeneradoDto> GenerarTemgeAsync(string usuario, string? periodoProceso = null);
}

public class TemgeService : ITemgeService
{
    private readonly SrjeDbContext _db;
    private readonly ILogger<TemgeService> _logger;
    private readonly SrjeSettings _settings;

    public TemgeService(SrjeDbContext db, ILogger<TemgeService> logger, IOptions<SrjeSettings> settings)
    {
        _db = db;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task<TemgeArchivoDto> PreviewTemgeAsync(Stream stream, string nombreArchivo)
    {
        var resultado = TemgeParser.Parsear(stream);

        if (!resultado.IntegridadOk)
        {
            _logger.LogWarning("Archivo TEMGE con integridad comprometida: monto cierre={MontoCierre}, registros cierre={RegCierre}",
                resultado.MontoTotalCierre, resultado.TotalRegistrosCierre);

            foreach (var l in resultado.Lineas)
            {
                if (l.EstadoLinea == "OK")
                {
                    l.EstadoLinea = "ADVERTENCIA";
                    l.Mensaje = "Integridad del archivo comprometida (totales no coinciden)";
                }
            }
        }

        // Solo cargar los RUTs necesarios en lugar de toda la tabla
        var rutsArchivo = resultado.Lineas
            .Where(l => l.EstadoLinea == "OK")
            .Select(l => l.RutBeneficiario)
            .Distinct()
            .ToList();
        var rutsExistentes = (await _db.Beneficiarios.AsNoTracking()
            .Where(b => rutsArchivo.Contains(b.RutBeneficiario))
            .Select(b => b.RutBeneficiario)
            .ToListAsync())
            .ToHashSet();

        foreach (var linea in resultado.Lineas)
        {
            if (!rutsExistentes.Contains(linea.RutBeneficiario) && linea.EstadoLinea == "OK")
            {
                linea.EstadoLinea = "ADVERTENCIA";
                linea.Mensaje = "Beneficiario no existe en BD";
            }
        }

        return resultado;
    }

    public async Task<ResultadoImportacionDto> ConfirmarTemgeAsync(
        ConfirmarImportacionRequest request, string usuario, string ip)
    {
        var inicio = DateTime.Now;
        var logCarga = new LogCarga
        {
            TipoCarga = "TEMGE_ENTRADA",
            NombreArchivo = "TEMGE_importacion",
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

            var historial = new HistorialPagosTemge
            {
                FechaProceso = inicio,
                CodEmpresa = _settings.CodEmpresa,
                Estado = "G",
                UsuarioGenera = usuario
            };
            _db.HistorialPagosTemge.Add(historial);
            await _db.SaveChangesAsync();

            // Pre-cargar beneficiarios para evitar N+1
            var rutsLineas = request.Lineas
                .Where(l => l.Incluir)
                .Select(l => l.RutBeneficiario)
                .Distinct()
                .ToList();
            var beneficiariosDict = await _db.Beneficiarios
                .Where(b => rutsLineas.Contains(b.RutBeneficiario))
                .ToDictionaryAsync(b => b.RutBeneficiario);

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
                    _db.DetallePagosTemge.Add(new DetallePagoTemge
                    {
                        IdHistorial = historial.Id,
                        RutBeneficiario = linea.RutBeneficiario,
                        MontoPagado = linea.Monto ?? 0,
                        CodBanco = linea.CodBanco ?? _settings.CodBancoEstado,
                        TipoCuenta = linea.TipoCuenta ?? 2
                    });

                    string accion;
                    if (beneficiariosDict.TryGetValue(linea.RutBeneficiario, out var beneficiario))
                    {
                        beneficiario.CodBanco = linea.CodBanco;
                        beneficiario.TipoCuenta = linea.TipoCuenta;
                        if (linea.CodBanco == _settings.CodBancoEstado)
                        {
                            beneficiario.CtaEstado = linea.NumeroCuenta;
                            beneficiario.CtaOtBanco = null;
                        }
                        else
                        {
                            beneficiario.CtaOtBanco = linea.NumeroCuenta;
                            beneficiario.CtaEstado = null;
                        }
                        beneficiario.FechaModificacion = DateTime.Now;
                        actualizados++;
                        accion = "ACTUALIZAR";
                    }
                    else
                    {
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
                    _logger.LogError(ex, "Error procesando linea TEMGE {NumLinea} RUT {Rut}",
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

            historial.MontoTotal = montoTotal;
            historial.CantidadRegistros = insertados + actualizados;

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
                "Importacion TEMGE completada: {Actualizados} actualizados, {SinBeneficiario} sin beneficiario en BD, {Errores} errores",
                actualizados, insertados, errores);

            return new ResultadoImportacionDto
            {
                IdCarga = logCarga.Id,
                Insertados = insertados,
                Actualizados = actualizados,
                Excluidos = excluidos,
                Errores = errores,
                MontoTotal = montoTotal,
                Mensaje = $"Importacion TEMGE completada: {actualizados} actualizados, {insertados} omitidos (sin beneficiario en BD)"
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error critico en importacion TEMGE, transaccion revertida");
            throw;
        }
    }

    public async Task<TemgeGeneradoDto> GenerarTemgeAsync(string usuario, string? periodoProceso = null)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            // Cargar retenciones activas del periodo (sin agrupar: 1 linea por retencion)
            var queryRetenciones = _db.RetenidosJudiciales.AsNoTracking()
                .Where(r => r.Estado == "A");

            if (!string.IsNullOrEmpty(periodoProceso))
                queryRetenciones = queryRetenciones.Where(r => r.PeriodoProceso == periodoProceso);

            var retenciones = await queryRetenciones.ToListAsync();

            var rutsBenef = retenciones.Select(r => r.RutBeneficiario).Distinct().ToList();

            var beneficiarios = await _db.Beneficiarios.AsNoTracking()
                .Where(b => b.Estado == "A" && rutsBenef.Contains(b.RutBeneficiario))
                .ToDictionaryAsync(b => b.RutBeneficiario);

            // Construir 1 registro por retencion, con fallback: retencion -> beneficiario -> default
            var datos = new List<(RegistroTemge Registro, long IdRetencion)>();
            var excluidos = new List<TemgeExcluidoDto>();

            foreach (var ret in retenciones)
            {
                if (!beneficiarios.TryGetValue(ret.RutBeneficiario, out var benef))
                {
                    excluidos.Add(new TemgeExcluidoDto
                    {
                        RutBeneficiario = ret.RutBeneficiario,
                        DvBeneficiario = ret.DvBeneficiario,
                        Monto = ret.Monto,
                        Motivo = "Beneficiario no existe o esta inactivo"
                    });
                    continue;
                }

                var codBanco = ret.CodBanco ?? benef.CodBanco ?? _settings.CodBancoEstado;
                var tipoCuenta = ret.TipoCuenta ?? benef.TipoCuenta ?? 2;
                var ctaEstado = ret.CtaEstado ?? benef.CtaEstado;
                var ctaOtBanco = ret.CtaOtBanco ?? benef.CtaOtBanco;

                // Debe tener al menos un numero de cuenta
                if (ctaEstado == null && ctaOtBanco == null)
                {
                    excluidos.Add(new TemgeExcluidoDto
                    {
                        RutBeneficiario = ret.RutBeneficiario,
                        DvBeneficiario = ret.DvBeneficiario,
                        NombreBeneficiario = benef.NombreBeneficiario,
                        Monto = ret.Monto,
                        Motivo = "Sin cuenta bancaria registrada"
                    });
                    continue;
                }

                datos.Add((new RegistroTemge
                {
                    RutBeneficiario = benef.RutBeneficiario,
                    DvBeneficiario = benef.DvBeneficiario,
                    NombreBeneficiario = benef.NombreBeneficiario,
                    CodBanco = codBanco,
                    TipoCuenta = tipoCuenta,
                    NumeroCuenta = ctaOtBanco,
                    CtaEstado = ctaEstado,
                    Monto = ret.Monto
                }, ret.Id));
            }

            var registros = datos.Select(d => d.Registro).ToList();
            var temgeBuilder = new TemgeBuilder(_settings);
            var fechaProceso = DateTime.Now;
            var archivo = temgeBuilder.Generar(registros, fechaProceso);

            var historial = new HistorialPagosTemge
            {
                FechaProceso = fechaProceso,
                CodEmpresa = _settings.CodEmpresa,
                MontoTotal = registros.Sum(d => d.Monto),
                CantidadRegistros = registros.Count,
                NombreArchivo = $"TEMGE_{fechaProceso:yyyyMMdd_HHmmss}.txt",
                UsuarioGenera = usuario
            };
            _db.HistorialPagosTemge.Add(historial);
            await _db.SaveChangesAsync();

            foreach (var (reg, idRet) in datos)
            {
                _db.DetallePagosTemge.Add(new DetallePagoTemge
                {
                    IdHistorial = historial.Id,
                    IdRetenidoJudicial = idRet,
                    RutBeneficiario = reg.RutBeneficiario,
                    MontoPagado = reg.Monto,
                    CodBanco = reg.CodBanco,
                    TipoCuenta = reg.TipoCuenta
                });
            }
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation(
                "Archivo TEMGE generado: {CantRegistros} registros, monto total {MontoTotal}, archivo {NombreArchivo}",
                registros.Count, registros.Sum(d => d.Monto), historial.NombreArchivo);

            if (excluidos.Count > 0)
            {
                foreach (var ex in excluidos)
                    _logger.LogWarning(
                        "Retencion excluida del TEMGE: RUT {Rut}-{Dv} {Nombre}, monto {Monto}, motivo: {Motivo}",
                        ex.RutBeneficiario, ex.DvBeneficiario, ex.NombreBeneficiario ?? "(sin nombre)", ex.Monto, ex.Motivo);

                _logger.LogWarning(
                    "TEMGE {NombreArchivo}: {CantExcluidos} retenciones excluidas por monto total {MontoExcluido}",
                    historial.NombreArchivo, excluidos.Count, excluidos.Sum(e => e.Monto));
            }

            return new TemgeGeneradoDto
            {
                Archivo = archivo,
                NombreArchivo = historial.NombreArchivo,
                CantidadRegistros = registros.Count,
                MontoTotal = registros.Sum(d => d.Monto),
                Excluidos = excluidos
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error critico generando archivo TEMGE, transaccion revertida");
            throw;
        }
    }
}
