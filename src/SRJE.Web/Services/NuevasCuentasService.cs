using Microsoft.EntityFrameworkCore;
using SRJE.Web.Infrastructure.Data;
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

    public NuevasCuentasService(SrjeDbContext db, ILogger<NuevasCuentasService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<ArchivoPreviewDto> PreviewNuevasCuentasAsync(Stream stream, string nombreArchivo)
    {
        var lineas = NuevasCuentasParser.Parsear(stream);

        // Solo cargar los RUTs necesarios en lugar de toda la tabla
        var rutsArchivo = lineas
            .Where(l => l.EstadoLinea != "ERROR")
            .Select(l => l.RutBeneficiario)
            .Distinct()
            .ToList();

        var beneficiariosExistentes = await _db.Beneficiarios
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
                if (!linea.Incluir) { excluidos++; continue; }

                try
                {
                    if (beneficiariosDict.TryGetValue(linea.RutBeneficiario, out var beneficiario))
                    {
                        beneficiario.CodBanco = linea.CodBanco;
                        beneficiario.TipoCuenta = linea.TipoCuenta;
                        beneficiario.CtaEstado = linea.CodBanco == 12 ? linea.NumeroCuenta : null;
                        beneficiario.CtaOtBanco = linea.CodBanco != 12 ? linea.NumeroCuenta : null;
                        beneficiario.FechaModificacion = DateTime.Now;
                        actualizados++;
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
                            CtaEstado = linea.CodBanco == 12 ? linea.NumeroCuenta : null,
                            CtaOtBanco = linea.CodBanco != 12 ? linea.NumeroCuenta : null,
                            UsuarioCreacion = usuario
                        };
                        _db.Beneficiarios.Add(nuevo);
                        // Agregar al dict para evitar duplicados en el mismo batch
                        beneficiariosDict[linea.RutBeneficiario] = nuevo;
                        insertados++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error procesando linea nuevas cuentas RUT {Rut}", linea.RutBeneficiario);
                    errores++;
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
