using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using SRJE.Web.Helpers;
using SRJE.Web.Infrastructure.Data;
using SRJE.Web.Models.Entities;
using SRJE.Web.Models.ViewModels;
using SRJE.Web.Parsers;

namespace SRJE.Web.Services;

public class ArchivoService : IArchivoService
{
    private readonly SrjeDbContext _db;

    public ArchivoService(SrjeDbContext db)
    {
        _db = db;
    }

    public async Task<ArchivoPreviewDto> PreviewRemuneracionesAsync(Stream stream, string nombreArchivo)
    {
        var lineas = RemuneracionesParser.Parsear(stream);

        // Enriquecer con datos de BD
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

    public async Task<TemgeArchivoDto> PreviewTemgeAsync(Stream stream, string nombreArchivo)
    {
        var resultado = TemgeParser.Parsear(stream);

        // Verificar integridad
        if (!resultado.IntegridadOk)
        {
            foreach (var l in resultado.Lineas)
            {
                if (l.EstadoLinea == "OK")
                {
                    l.EstadoLinea = "ADVERTENCIA";
                    l.Mensaje = "Integridad del archivo comprometida (totales no coinciden)";
                }
            }
        }

        // Marcar beneficiarios no encontrados
        var rutsExistentes = (await _db.Beneficiarios
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

    public async Task<ArchivoPreviewDto> PreviewNuevasCuentasAsync(Stream stream, string nombreArchivo)
    {
        var lineas = NuevasCuentasParser.Parsear(stream);

        var beneficiariosExistentes = await _db.Beneficiarios
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
        _db.LogCargas.Add(logCarga);
        await _db.SaveChangesAsync();

        int insertados = 0, actualizados = 0, excluidos = 0, errores = 0;
        decimal montoTotal = 0;

        // Track funcionarios already processed in this batch to avoid duplicate inserts
        var funcionariosEnBatch = new Dictionary<long, Funcionario>();

        foreach (var linea in request.Lineas)
        {
            if (!linea.Incluir)
            {
                excluidos++;
                continue;
            }

            try
            {
                // Upsert funcionario
                if (linea.RutFuncionario.HasValue &&
                    !funcionariosEnBatch.ContainsKey(linea.RutFuncionario.Value))
                {
                    var funcionario = await _db.Funcionarios
                        .FirstOrDefaultAsync(f => f.RutFuncionario == linea.RutFuncionario);

                    if (funcionario == null)
                    {
                        funcionario = new Funcionario
                        {
                            RutFuncionario = linea.RutFuncionario.Value,
                            DvFuncionario = linea.DvFuncionario ?? "",
                            Activo = "S"
                        };
                        _db.Funcionarios.Add(funcionario);
                    }

                    funcionariosEnBatch[linea.RutFuncionario.Value] = funcionario;
                }

                // Upsert retencion
                var retencion = await _db.RetenidosJudiciales
                    .FirstOrDefaultAsync(r =>
                        r.RutBeneficiario == linea.RutBeneficiario &&
                        r.RutTitular == (linea.RutFuncionario ?? 0) &&
                        r.PeriodoProceso == request.PeriodoProceso);

                if (retencion == null)
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
                else
                {
                    retencion.Monto = linea.Monto ?? 0;
                    retencion.CodRetencion = linea.CodRetencion;
                    retencion.TipoPago = linea.TipoPago;
                    actualizados++;
                }

                montoTotal += linea.Monto ?? 0;
            }
            catch
            {
                errores++;
            }
        }

        await _db.SaveChangesAsync();

        logCarga.Estado = "C";
        logCarga.FechaFin = DateTime.Now;
        logCarga.DuracionMs = (long)(logCarga.FechaFin.Value - inicio).TotalMilliseconds;
        logCarga.RegistrosInsertados = insertados;
        logCarga.RegistrosActualizados = actualizados;
        logCarga.RegistrosExcluidos = excluidos;
        logCarga.RegistrosError = errores;
        logCarga.MontoTotal = montoTotal;
        await _db.SaveChangesAsync();

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
        _db.LogCargas.Add(logCarga);
        await _db.SaveChangesAsync();

        int insertados = 0, actualizados = 0, excluidos = 0, errores = 0;

        foreach (var linea in request.Lineas)
        {
            if (!linea.Incluir) { excluidos++; continue; }

            try
            {
                var beneficiario = await _db.Beneficiarios
                    .FirstOrDefaultAsync(b => b.RutBeneficiario == linea.RutBeneficiario);

                if (beneficiario == null)
                {
                    _db.Beneficiarios.Add(new Beneficiario
                    {
                        RutBeneficiario = linea.RutBeneficiario,
                        DvBeneficiario = linea.DvBeneficiario,
                        NombreBeneficiario = linea.NombreBeneficiario,
                        CodBanco = linea.CodBanco,
                        TipoCuenta = linea.TipoCuenta,
                        CtaEstado = linea.CodBanco == 12 ? linea.NumeroCuenta?.PadLeft(11, '0') : null,
                        CtaOtBanco = linea.CodBanco != 12 ? linea.NumeroCuenta : null,
                        UsuarioCreacion = usuario
                    });
                    insertados++;
                }
                else
                {
                    beneficiario.CodBanco = linea.CodBanco;
                    beneficiario.TipoCuenta = linea.TipoCuenta;
                    beneficiario.CtaEstado = linea.CodBanco == 12 ? linea.NumeroCuenta?.PadLeft(11, '0') : null;
                    beneficiario.CtaOtBanco = linea.CodBanco != 12 ? linea.NumeroCuenta : null;
                    beneficiario.FechaModificacion = DateTime.Now;
                    actualizados++;
                }
            }
            catch
            {
                errores++;
            }
        }

        await _db.SaveChangesAsync();

        logCarga.Estado = "C";
        logCarga.FechaFin = DateTime.Now;
        logCarga.DuracionMs = (long)(logCarga.FechaFin.Value - inicio).TotalMilliseconds;
        logCarga.RegistrosInsertados = insertados;
        logCarga.RegistrosActualizados = actualizados;
        logCarga.RegistrosExcluidos = excluidos;
        logCarga.RegistrosError = errores;
        await _db.SaveChangesAsync();

        return new ResultadoImportacionDto
        {
            IdCarga = logCarga.Id,
            Insertados = insertados,
            Actualizados = actualizados,
            Excluidos = excluidos,
            Errores = errores
        };
    }

    public async Task<byte[]> GenerarTemgeAsync(string usuario)
    {
        // Obtener beneficiarios activos con retencion activa y cuenta valida
        var datos = await (
            from b in _db.Beneficiarios
            join r in _db.RetenidosJudiciales on b.RutBeneficiario equals r.RutBeneficiario
            where b.Estado == "A" && r.Estado == "A"
                && (b.CtaEstado != null || b.CtaOtBanco != null)
            select new RegistroTemge
            {
                RutBeneficiario = b.RutBeneficiario,
                DvBeneficiario = b.DvBeneficiario,
                NombreBeneficiario = b.NombreBeneficiario,
                CodBanco = b.CodBanco ?? 12,
                TipoCuenta = b.TipoCuenta ?? 2,
                NumeroCuenta = b.CtaOtBanco,
                CtaEstado = b.CtaEstado,
                Monto = r.Monto
            }).ToListAsync();

        var builder = new TemgeBuilder();
        var fechaProceso = DateTime.Now;
        var archivo = builder.Generar(datos, fechaProceso);

        // Registrar historial
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

        // Registrar detalle
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

        return archivo;
    }

    private static string ComputeHash(Stream stream)
    {
        stream.Position = 0;
        var hash = SHA256.HashData(stream);
        stream.Position = 0;
        return Convert.ToHexString(hash);
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
