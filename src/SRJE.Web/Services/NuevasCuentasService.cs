using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SRJE.Web.Infrastructure.Data;
using SRJE.Web.Models;
using SRJE.Web.Models.Entities;
using SRJE.Web.Models.ViewModels;
using SRJE.Web.Parsers;

namespace SRJE.Web.Services;

public interface INuevasCuentasService
{
    Task<ArchivoPreviewDto> PreviewNuevasCuentasAsync(Stream stream, string nombreArchivo);
    Task<ResultadoImportacionDto> ConfirmarNuevasCuentasAsync(ConfirmarImportacionRequest request, string usuario, string ip);
}

public class NuevasCuentasService : INuevasCuentasService
{
    private readonly SrjeDbContext _db;
    private readonly ILogger<NuevasCuentasService> _logger;
    private readonly SrjeSettings _settings;

    public NuevasCuentasService(SrjeDbContext db, ILogger<NuevasCuentasService> logger, IOptions<SrjeSettings> settings)
    {
        _db = db;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task<ArchivoPreviewDto> PreviewNuevasCuentasAsync(Stream stream, string nombreArchivo)
    {
        var lineas = NuevasCuentasParser.Parsear(stream, _settings.CodBancoEstado);

        // Solo cargar los RUTs necesarios en lugar de toda la tabla
        var rutsArchivo = lineas
            .Where(l => l.EstadoLinea != "ERROR")
            .Select(l => l.RutBeneficiario)
            .Distinct()
            .ToList();

        var beneficiariosExistentes = await _db.Beneficiarios.AsNoTracking()
            .Where(b => rutsArchivo.Contains(b.RutBeneficiario))
            .ToDictionaryAsync(b => b.RutBeneficiario);

        foreach (var linea in lineas)
        {
            if (linea.EstadoLinea == "ERROR") continue;

            if (beneficiariosExistentes.TryGetValue(linea.RutBeneficiario, out var benef))
            {
                if (!string.IsNullOrEmpty(benef.CtaEstado) || !string.IsNullOrEmpty(benef.CtaOtBanco))
                {
                    linea.EstadoLinea = "ADVERTENCIA";
                    linea.Mensaje = "Cuenta existente sera reemplazada";
                }
            }
            else
            {
                linea.EstadoLinea = "NUEVO";
                linea.Mensaje = "Beneficiario no existe — se creara al confirmar";
            }
        }

        return BuildPreview(lineas);
    }

    public async Task<ResultadoImportacionDto> ConfirmarNuevasCuentasAsync(
        ConfirmarImportacionRequest request, string usuario, string ip)
    {
        var inicio = DateTime.Now;
        var logCarga = new LogCarga
        {
            TipoCarga = "NUEVAS_CUENTAS",
            NombreArchivo = "NuevasCuentas_importacion",
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
                    string accion;
                    if (beneficiariosDict.TryGetValue(linea.RutBeneficiario, out var beneficiario))
                    {
                        beneficiario.CodBanco = linea.CodBanco;
                        beneficiario.TipoCuenta = linea.TipoCuenta;
                        beneficiario.CtaEstado = linea.CodBanco == _settings.CodBancoEstado ? linea.NumeroCuenta : null;
                        beneficiario.CtaOtBanco = linea.CodBanco != _settings.CodBancoEstado ? linea.NumeroCuenta : null;
                        beneficiario.FechaModificacion = DateTime.Now;
                        actualizados++;
                        accion = "ACTUALIZAR";
                    }
                    else
                    {
                        var nuevo = new Beneficiario
                        {
                            RutBeneficiario = linea.RutBeneficiario,
                            DvBeneficiario = linea.DvBeneficiario,
                            NombreBeneficiario = linea.NombreBeneficiario,
                            CodBanco = linea.CodBanco,
                            TipoCuenta = linea.TipoCuenta,
                            CtaEstado = linea.CodBanco == _settings.CodBancoEstado ? linea.NumeroCuenta : null,
                            CtaOtBanco = linea.CodBanco != _settings.CodBancoEstado ? linea.NumeroCuenta : null,
                            UsuarioCreacion = usuario
                        };
                        _db.Beneficiarios.Add(nuevo);
                        beneficiariosDict[linea.RutBeneficiario] = nuevo;
                        insertados++;
                        accion = "INSERTAR";
                    }

                    // Upsert en CUENTAS_BENEFICIARIO
                    if (linea.CodBanco.HasValue && !string.IsNullOrEmpty(linea.NumeroCuenta))
                    {
                        var numeroCuenta = linea.NumeroCuenta;
                        var codBanco = linea.CodBanco.Value;
                        var tipoCta = linea.TipoCuenta ?? 1;

                        var existeCuenta = await _db.CuentasBeneficiario.CountAsync(c =>
                            c.RutBeneficiario == linea.RutBeneficiario &&
                            c.CodBanco == codBanco &&
                            c.TipoCuenta == tipoCta &&
                            c.NumeroCuenta == numeroCuenta);

                        if (existeCuenta == 0)
                        {
                            _db.CuentasBeneficiario.Add(new CuentaBeneficiario
                            {
                                RutBeneficiario = linea.RutBeneficiario,
                                CodBanco = codBanco,
                                TipoCuenta = tipoCta,
                                NumeroCuenta = numeroCuenta,
                                Alias = "Desde nuevas cuentas",
                                UsuarioCreacion = usuario
                            });
                        }
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
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error procesando linea nuevas cuentas RUT {Rut}", linea.RutBeneficiario);
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

            logCarga.Estado = "C";
            logCarga.FechaFin = DateTime.Now;
            logCarga.DuracionMs = (long)(logCarga.FechaFin.Value - inicio).TotalMilliseconds;
            logCarga.RegistrosInsertados = insertados;
            logCarga.RegistrosActualizados = actualizados;
            logCarga.RegistrosExcluidos = excluidos;
            logCarga.RegistrosError = errores;

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation(
                "Importacion nuevas cuentas completada: {Insertados} nuevos, {Actualizados} actualizados, {Errores} errores",
                insertados, actualizados, errores);

            return new ResultadoImportacionDto
            {
                IdCarga = logCarga.Id,
                Insertados = insertados,
                Actualizados = actualizados,
                Excluidos = excluidos,
                Errores = errores
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error critico en importacion de nuevas cuentas, transaccion revertida");
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
