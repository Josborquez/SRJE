using System.Text;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using SRJE.Web.Helpers;
using SRJE.Web.Infrastructure.Data;
using SRJE.Web.Models.Entities;
using SRJE.Web.Models.Requests;
using SRJE.Web.Models.ViewModels;

namespace SRJE.Web.Services;

public class FuncionarioService : IFuncionarioService
{
    private readonly SrjeDbContext _db;

    public FuncionarioService(SrjeDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<FuncionarioDto>> ListarAsync(BuscarFuncionarioQuery query)
    {
        var q = _db.Funcionarios.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(query.Q))
        {
            var busqueda = query.Q.Trim().ToUpper();
            if (long.TryParse(busqueda.Replace(".", "").Replace("-", ""), out var rut))
                q = q.Where(f => f.RutFuncionario == rut);
            else
                q = q.Where(f =>
                    (f.ApellidoPaterno != null && f.ApellidoPaterno.ToUpper().Contains(busqueda)) ||
                    (f.ApellidoMaterno != null && f.ApellidoMaterno.ToUpper().Contains(busqueda)) ||
                    (f.Nombres != null && f.Nombres.ToUpper().Contains(busqueda)));
        }

        if (!string.IsNullOrEmpty(query.Activo))
            q = q.Where(f => f.Activo == query.Activo);

        var total = await q.CountAsync();

        // Contadores globales (sin filtro de busqueda ni estado)
        var totalInscritos = await _db.Funcionarios.AsNoTracking().CountAsync();
        var totalActivos = await _db.Funcionarios.AsNoTracking().CountAsync(f => f.Activo == "S");

        var items = await q
            .OrderBy(f => f.ApellidoPaterno).ThenBy(f => f.ApellidoMaterno).ThenBy(f => f.Nombres)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(f => MapToDto(f))
            .ToListAsync();

        // Enriquecer con cantidad de beneficiarios desde RETENIDO_JUDICIAL
        var rutsFuncionario = items.Select(i => i.RutFuncionario).ToList();
        if (rutsFuncionario.Count > 0)
        {
            var beneficiariosPorFunc = await _db.RetenidosJudiciales.AsNoTracking()
                .Where(r => r.Estado == "A" && rutsFuncionario.Contains(r.RutTitular))
                .Select(r => new { r.RutTitular, r.RutBeneficiario })
                .Distinct()
                .GroupBy(r => r.RutTitular)
                .Select(g => new { Rut = g.Key, Cantidad = g.Count() })
                .ToListAsync();

            var benefDict = beneficiariosPorFunc.ToDictionary(x => x.Rut);
            foreach (var item in items)
            {
                if (benefDict.TryGetValue(item.RutFuncionario, out var benef))
                    item.CantidadBeneficiarios = benef.Cantidad;
            }
        }

        // Enriquecer con monto total de retenciones activas del ultimo periodo
        if (rutsFuncionario.Count > 0)
        {
            var ultimoPeriodo = await _db.RetenidosJudiciales.AsNoTracking()
                .Where(r => r.Estado == "A")
                .OrderByDescending(r => r.PeriodoProceso)
                .Select(r => r.PeriodoProceso)
                .FirstOrDefaultAsync();

            if (ultimoPeriodo != null)
            {
                var retencionesPorFunc = await _db.RetenidosJudiciales.AsNoTracking()
                    .Where(r => r.Estado == "A"
                        && r.PeriodoProceso == ultimoPeriodo
                        && rutsFuncionario.Contains(r.RutTitular))
                    .GroupBy(r => r.RutTitular)
                    .Select(g => new { Rut = g.Key, Monto = g.Sum(r => r.Monto) })
                    .ToListAsync();

                var retDict = retencionesPorFunc.ToDictionary(r => r.Rut);
                foreach (var item in items)
                {
                    if (retDict.TryGetValue(item.RutFuncionario, out var ret))
                        item.MontoTotalRetenciones = ret.Monto;
                }
            }
        }

        return new PagedResult<FuncionarioDto>
        {
            Items = items,
            TotalCount = total,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalInscritos = totalInscritos,
            TotalActivos = totalActivos,
            TotalInactivos = totalInscritos - totalActivos
        };
    }

    public async Task<FuncionarioDetalleDto?> ObtenerPorRutAsync(long rut)
    {
        var funcionario = await _db.Funcionarios.AsNoTracking()
            .FirstOrDefaultAsync(f => f.RutFuncionario == rut);

        if (funcionario == null)
            return null;

        // Obtener beneficiarios asociados a este funcionario via RETENIDO_JUDICIAL
        var rutsBenefFromRet = await _db.RetenidosJudiciales.AsNoTracking()
            .Where(r => r.RutTitular == rut && r.Estado == "A")
            .Select(r => r.RutBeneficiario)
            .Distinct()
            .ToListAsync();

        var beneficiarios = await _db.Beneficiarios.AsNoTracking()
            .Where(b => rutsBenefFromRet.Contains(b.RutBeneficiario))
            .ToListAsync();

        // Obtener retenciones activas donde este funcionario es titular, agrupadas por beneficiario
        var retenciones = await _db.RetenidosJudiciales.AsNoTracking()
            .Where(r => r.RutTitular == rut && r.Estado == "A")
            .ToListAsync();

        // Enriquecer con nombres de banco
        var codsBanco = beneficiarios.Where(b => b.CodBanco.HasValue)
            .Select(b => b.CodBanco!.Value).Distinct().ToList();
        var bancos = new Dictionary<long, string>();
        if (codsBanco.Count > 0)
        {
            bancos = await _db.Bancos.AsNoTracking()
                .Where(b => codsBanco.Contains(b.CodBanco))
                .ToDictionaryAsync(b => b.CodBanco, b => b.NombreBanco);
        }

        // Enriquecer con tipos de cuenta
        var codsTipoCuenta = beneficiarios.Where(b => b.TipoCuenta.HasValue)
            .Select(b => b.TipoCuenta!.Value).Distinct().ToList();
        var tiposCuenta = new Dictionary<long, string>();
        if (codsTipoCuenta.Count > 0)
        {
            tiposCuenta = await _db.TiposCuenta.AsNoTracking()
                .Where(t => codsTipoCuenta.Contains(t.CodTipoCuenta))
                .ToDictionaryAsync(t => t.CodTipoCuenta, t => t.Descripcion);
        }

        // Agrupar retenciones por beneficiario
        var retencionesPorBenef = retenciones
            .GroupBy(r => r.RutBeneficiario)
            .ToDictionary(g => g.Key, g => g.ToList());

        var dto = new FuncionarioDetalleDto
        {
            Id = funcionario.Id,
            RutFuncionario = funcionario.RutFuncionario,
            DvFuncionario = funcionario.DvFuncionario,
            RutFormateado = RutHelper.Formatear(funcionario.RutFuncionario, funcionario.DvFuncionario),
            ApellidoPaterno = funcionario.ApellidoPaterno,
            ApellidoMaterno = funcionario.ApellidoMaterno,
            Nombres = funcionario.Nombres,
            IdSistema = funcionario.IdSistema,
            Activo = funcionario.Activo,
            CantidadBeneficiarios = beneficiarios.Count,
            MontoTotalRetenciones = retenciones.Sum(r => r.Monto),
            Beneficiarios = beneficiarios.Select(b =>
            {
                var benefDto = new BeneficiarioResumenDto
                {
                    RutBeneficiario = b.RutBeneficiario,
                    DvBeneficiario = b.DvBeneficiario,
                    RutFormateado = RutHelper.Formatear(b.RutBeneficiario, b.DvBeneficiario),
                    NombreBeneficiario = b.NombreBeneficiario,
                    CodBanco = b.CodBanco,
                    TipoCuenta = b.TipoCuenta,
                    CtaEstado = b.CtaEstado,
                    CtaOtBanco = b.CtaOtBanco
                };

                if (b.CodBanco.HasValue && bancos.TryGetValue(b.CodBanco.Value, out var nombreBanco))
                    benefDto.NombreBanco = nombreBanco;

                if (b.TipoCuenta.HasValue && tiposCuenta.TryGetValue(b.TipoCuenta.Value, out var tipoCuentaDesc))
                    benefDto.TipoCuentaDescripcion = tipoCuentaDesc;

                if (retencionesPorBenef.TryGetValue(b.RutBeneficiario, out var rets))
                {
                    benefDto.Retenciones = rets.Select(r => new RetencionResumenDto
                    {
                        Id = r.Id,
                        Monto = r.Monto,
                        CodRetencion = r.CodRetencion,
                        TipoPago = r.TipoPago,
                        PeriodoProceso = r.PeriodoProceso,
                        Estado = r.Estado
                    }).ToList();
                }

                return benefDto;
            }).ToList()
        };

        return dto;
    }

    public async Task<FuncionarioDto> ActualizarAsync(long rut, ActualizarFuncionarioRequest request, string usuario)
    {
        var entity = await _db.Funcionarios
            .FirstOrDefaultAsync(f => f.RutFuncionario == rut)
            ?? throw new KeyNotFoundException("Funcionario no encontrado");

        // Registrar cambios en auditoria
        var cambios = new List<(string campo, string? anterior, string? nuevo)>();

        if (request.ApellidoPaterno != null && request.ApellidoPaterno != entity.ApellidoPaterno)
            cambios.Add(("APELLIDO_PATERNO", entity.ApellidoPaterno, request.ApellidoPaterno));
        if (request.ApellidoMaterno != null && request.ApellidoMaterno != entity.ApellidoMaterno)
            cambios.Add(("APELLIDO_MATERNO", entity.ApellidoMaterno, request.ApellidoMaterno));
        if (request.Nombres != null && request.Nombres != entity.Nombres)
            cambios.Add(("NOMBRES", entity.Nombres, request.Nombres));
        if (request.IdSistema != null && request.IdSistema != entity.IdSistema)
            cambios.Add(("ID_SISTEMA", entity.IdSistema, request.IdSistema));

        // Actualizar campos
        if (request.ApellidoPaterno != null) entity.ApellidoPaterno = request.ApellidoPaterno;
        if (request.ApellidoMaterno != null) entity.ApellidoMaterno = request.ApellidoMaterno;
        if (request.Nombres != null) entity.Nombres = request.Nombres;
        if (request.IdSistema != null) entity.IdSistema = request.IdSistema;

        foreach (var (campo, anterior, nuevo) in cambios)
        {
            _db.AuditoriaCambios.Add(new AuditoriaCambios
            {
                Entidad = "FUNCIONARIO",
                IdEntidad = entity.Id,
                RutAfectado = RutHelper.Formatear(entity.RutFuncionario, entity.DvFuncionario),
                Accion = "ACTUALIZAR",
                CampoModificado = campo,
                ValorAnterior = anterior,
                ValorNuevo = nuevo,
                Usuario = usuario,
                Fecha = DateTime.Now
            });
        }

        await _db.SaveChangesAsync();
        return MapToDto(entity);
    }

    public async Task<bool> InactivarAsync(long rut, string usuario)
    {
        var entity = await _db.Funcionarios
            .FirstOrDefaultAsync(f => f.RutFuncionario == rut);

        if (entity == null) return false;

        entity.Activo = "N";

        _db.AuditoriaCambios.Add(new AuditoriaCambios
        {
            Entidad = "FUNCIONARIO",
            IdEntidad = entity.Id,
            RutAfectado = RutHelper.Formatear(entity.RutFuncionario, entity.DvFuncionario),
            Accion = "INACTIVAR",
            CampoModificado = "ACTIVO",
            ValorAnterior = "S",
            ValorNuevo = "N",
            Usuario = usuario,
            Fecha = DateTime.Now
        });

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<FuncionarioStatsDto> ObtenerStatsAsync()
    {
        var totalFuncionarios = await _db.Funcionarios.AsNoTracking().CountAsync();
        var totalActivos = await _db.Funcionarios.AsNoTracking().CountAsync(f => f.Activo == "S");

        // Obtener ultimo periodo y monto total de retenciones activas
        var ultimoPeriodo = await _db.RetenidosJudiciales.AsNoTracking()
            .Where(r => r.Estado == "A")
            .OrderByDescending(r => r.PeriodoProceso)
            .Select(r => r.PeriodoProceso)
            .FirstOrDefaultAsync();

        decimal montoMensualTotal = 0;
        if (ultimoPeriodo != null)
        {
            montoMensualTotal = await _db.RetenidosJudiciales.AsNoTracking()
                .Where(r => r.Estado == "A" && r.PeriodoProceso == ultimoPeriodo)
                .SumAsync(r => r.Monto);
        }

        return new FuncionarioStatsDto
        {
            TotalFuncionarios = totalFuncionarios,
            TotalActivos = totalActivos,
            TotalInactivos = totalFuncionarios - totalActivos,
            MontoMensualTotal = montoMensualTotal,
            PeriodoActual = ultimoPeriodo
        };
    }

    public async Task<byte[]> ExportarExcelAsync(string? q = null, string? activo = null)
    {
        var funcionarios = await ObtenerFuncionariosParaExportar(q, activo);

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Funcionarios");

        var headers = new[] { "RUT", "Apellido Paterno", "Apellido Materno", "Nombres",
            "Beneficiarios", "Monto Total", "Estado" };
        for (int c = 0; c < headers.Length; c++)
        {
            ws.Cells[1, c + 1].Value = headers[c];
            ws.Cells[1, c + 1].Style.Font.Bold = true;
            ws.Cells[1, c + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            ws.Cells[1, c + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(44, 62, 80));
            ws.Cells[1, c + 1].Style.Font.Color.SetColor(System.Drawing.Color.White);
        }

        for (int i = 0; i < funcionarios.Count; i++)
        {
            var f = funcionarios[i];
            int row = i + 2;
            ws.Cells[row, 1].Value = f.RutFormateado;
            ws.Cells[row, 2].Value = f.ApellidoPaterno;
            ws.Cells[row, 3].Value = f.ApellidoMaterno;
            ws.Cells[row, 4].Value = f.Nombres;
            ws.Cells[row, 5].Value = f.CantidadBeneficiarios;
            ws.Cells[row, 6].Value = f.MontoTotalRetenciones;
            ws.Cells[row, 6].Style.Numberformat.Format = "#,##0";
            ws.Cells[row, 7].Value = f.Activo == "S" ? "Activo" : "Inactivo";
        }

        ws.Cells[ws.Dimension.Address].AutoFitColumns();
        return package.GetAsByteArray();
    }

    public async Task<byte[]> ExportarCsvAsync(string? q = null, string? activo = null)
    {
        var funcionarios = await ObtenerFuncionariosParaExportar(q, activo);

        var sb = new StringBuilder();
        sb.AppendLine("RUT;Apellido Paterno;Apellido Materno;Nombres;Beneficiarios;Monto Total;Estado");

        foreach (var f in funcionarios)
        {
            sb.AppendLine(string.Join(";",
                f.RutFormateado,
                f.ApellidoPaterno ?? "",
                f.ApellidoMaterno ?? "",
                f.Nombres ?? "",
                f.CantidadBeneficiarios,
                f.MontoTotalRetenciones,
                f.Activo == "S" ? "Activo" : "Inactivo"
            ));
        }

        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    private async Task<List<FuncionarioDto>> ObtenerFuncionariosParaExportar(string? q, string? activo)
    {
        var query = _db.Funcionarios.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(q))
        {
            var busqueda = q.Trim().ToUpper();
            if (long.TryParse(busqueda.Replace(".", "").Replace("-", ""), out var rut))
                query = query.Where(f => f.RutFuncionario == rut);
            else
                query = query.Where(f =>
                    (f.ApellidoPaterno != null && f.ApellidoPaterno.ToUpper().Contains(busqueda)) ||
                    (f.ApellidoMaterno != null && f.ApellidoMaterno.ToUpper().Contains(busqueda)) ||
                    (f.Nombres != null && f.Nombres.ToUpper().Contains(busqueda)));
        }

        if (!string.IsNullOrEmpty(activo))
            query = query.Where(f => f.Activo == activo);

        var items = await query
            .OrderBy(f => f.ApellidoPaterno).ThenBy(f => f.ApellidoMaterno).ThenBy(f => f.Nombres)
            .Select(f => MapToDto(f))
            .ToListAsync();

        // Enriquecer sin filtrar por RUT (evita limite de IN en Oracle al exportar todo)
        var beneficiariosPorFunc = await _db.RetenidosJudiciales.AsNoTracking()
            .Where(r => r.Estado == "A")
            .Select(r => new { r.RutTitular, r.RutBeneficiario })
            .Distinct()
            .GroupBy(r => r.RutTitular)
            .Select(g => new { Rut = g.Key, Cantidad = g.Count() })
            .ToDictionaryAsync(x => x.Rut, x => x.Cantidad);

        var ultimoPeriodo = await _db.RetenidosJudiciales.AsNoTracking()
            .Where(r => r.Estado == "A")
            .OrderByDescending(r => r.PeriodoProceso)
            .Select(r => r.PeriodoProceso)
            .FirstOrDefaultAsync();

        var montoPorFunc = ultimoPeriodo == null
            ? new Dictionary<long, decimal>()
            : await _db.RetenidosJudiciales.AsNoTracking()
                .Where(r => r.Estado == "A" && r.PeriodoProceso == ultimoPeriodo)
                .GroupBy(r => r.RutTitular)
                .Select(g => new { Rut = g.Key, Monto = g.Sum(r => r.Monto) })
                .ToDictionaryAsync(x => x.Rut, x => x.Monto);

        foreach (var item in items)
        {
            if (beneficiariosPorFunc.TryGetValue(item.RutFuncionario, out var cantidad))
                item.CantidadBeneficiarios = cantidad;
            if (montoPorFunc.TryGetValue(item.RutFuncionario, out var monto))
                item.MontoTotalRetenciones = monto;
        }

        return items;
    }

    private static FuncionarioDto MapToDto(Funcionario f) => new()
    {
        Id = f.Id,
        RutFuncionario = f.RutFuncionario,
        DvFuncionario = f.DvFuncionario,
        RutFormateado = RutHelper.Formatear(f.RutFuncionario, f.DvFuncionario),
        ApellidoPaterno = f.ApellidoPaterno,
        ApellidoMaterno = f.ApellidoMaterno,
        Nombres = f.Nombres,
        IdSistema = f.IdSistema,
        Activo = f.Activo
    };
}
