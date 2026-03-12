using Microsoft.EntityFrameworkCore;
using SRJE.Web.Infrastructure.Data;
using SRJE.Web.Models.Entities;
using SRJE.Web.Models.ViewModels;
using SRJE.Web.Parsers;

namespace SRJE.Web.Services;

public interface ITemgeService
{
    Task<TemgeArchivoDto> PreviewTemgeAsync(Stream stream, string nombreArchivo);
    Task<ResultadoImportacionDto> ConfirmarTemgeAsync(ConfirmarImportacionRequest request, string usuario, string ip);
    Task<byte[]> GenerarTemgeAsync(string usuario);
}

public class TemgeService : ITemgeService
{
    private readonly SrjeDbContext _db;
    private readonly ILogger<TemgeService> _logger;

    public TemgeService(SrjeDbContext db, ILogger<TemgeService> logger)
    {
        _db = db;
        _logger = logger;
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
        var rutsExistentes = (await _db.Beneficiarios
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
                HoraProceso = inicio.ToString("HHmmss"),
                CodEmpresa = "06110104519640100572",
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
                        CodBanco = linea.CodBanco ?? 12,
                        TipoCuenta = linea.TipoCuenta ?? 2
                    });

                    string accion;
                    if (beneficiariosDict.TryGetValue(linea.RutBeneficiario, out var beneficiario))
                    {
                        beneficiario.CodBanco = linea.CodBanco;
                        beneficiario.TipoCuenta = linea.TipoCuenta;
                        if (linea.CodBanco == 12)
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
                "Importacion TEMGE completada: {Actualizados} actualizados, {Insertados} sin beneficiario, {Errores} errores",
                actualizados, insertados, errores);

            return new ResultadoImportacionDto
            {
                IdCarga = logCarga.Id,
                Insertados = insertados,
                Actualizados = actualizados,
                Excluidos = excluidos,
                Errores = errores,
                MontoTotal = montoTotal,
                Mensaje = $"Importacion TEMGE completada: {actualizados} actualizados, {insertados} sin beneficiario en BD"
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error critico en importacion TEMGE, transaccion revertida");
            throw;
        }
    }

    public async Task<byte[]> GenerarTemgeAsync(string usuario)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var beneficiarios = await _db.Beneficiarios
                .Where(b => b.Estado == "A"
                    && (b.CtaEstado != null || b.CtaOtBanco != null))
                .ToDictionaryAsync(b => b.RutBeneficiario);

            var retenciones = await _db.RetenidosJudiciales
                .Where(r => r.Estado == "A")
                .ToListAsync();

            var datos = retenciones
                .Where(r => beneficiarios.ContainsKey(r.RutBeneficiario))
                .GroupBy(r => r.RutBeneficiario)
                .Select(g =>
                {
                    var benef = beneficiarios[g.Key];
                    return new RegistroTemge
                    {
                        RutBeneficiario = benef.RutBeneficiario,
                        DvBeneficiario = benef.DvBeneficiario,
                        NombreBeneficiario = benef.NombreBeneficiario,
                        CodBanco = benef.CodBanco ?? 12,
                        TipoCuenta = benef.TipoCuenta ?? 2,
                        NumeroCuenta = benef.CtaOtBanco,
                        CtaEstado = benef.CtaEstado,
                        Monto = g.Sum(r => r.Monto)
                    };
                }).ToList();

            var builder = new TemgeBuilder();
            var fechaProceso = DateTime.Now;
            var archivo = builder.Generar(datos, fechaProceso);

            var historial = new HistorialPagosTemge
            {
                FechaProceso = fechaProceso,
                HoraProceso = fechaProceso.ToString("HHmmss"),
                CodEmpresa = "06110104519640100572",
                MontoTotal = datos.Sum(d => d.Monto),
                CantidadRegistros = datos.Count,
                NombreArchivo = $"TEMGE_{fechaProceso:yyyyMMdd_HHmmss}.txt",
                UsuarioGenera = usuario
            };
            _db.HistorialPagosTemge.Add(historial);
            await _db.SaveChangesAsync();

            foreach (var reg in datos)
            {
                _db.DetallePagosTemge.Add(new DetallePagoTemge
                {
                    IdHistorial = historial.Id,
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
                datos.Count, datos.Sum(d => d.Monto), historial.NombreArchivo);

            return archivo;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error critico generando archivo TEMGE, transaccion revertida");
            throw;
        }
    }
}
