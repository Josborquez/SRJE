using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using SRJE.Web.Helpers;
using SRJE.Web.Infrastructure.Data;
using SRJE.Web.Models;
using SRJE.Web.Models.Entities;
using SRJE.Web.Models.Exceptions;
using SRJE.Web.Models.Requests;
using SRJE.Web.Models.ViewModels;

namespace SRJE.Web.Services;

public class BeneficiarioService : IBeneficiarioService
{
    private readonly SrjeDbContext _db;
    private readonly SrjeSettings _settings;

    public BeneficiarioService(SrjeDbContext db, IOptions<SrjeSettings> settings)
    {
        _db = db;
        _settings = settings.Value;
    }

    public async Task<PagedResult<BeneficiarioDto>> ListarAsync(BuscarBeneficiarioQuery query)
    {
        var q = _db.Beneficiarios.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(query.Q))
        {
            var busqueda = query.Q.Trim().ToUpper();
            if (long.TryParse(busqueda.Replace(".", "").Replace("-", ""), out var rut))
                q = q.Where(b => b.RutBeneficiario == rut);
            else
                q = q.Where(b => b.NombreBeneficiario.ToUpper().Contains(busqueda));
        }

        if (!string.IsNullOrEmpty(query.Estado))
            q = q.Where(b => b.Estado == query.Estado);

        var total = await q.CountAsync();

        // Contadores globales (sin filtro de busqueda ni estado)
        var totalInscritos = await _db.Beneficiarios.AsNoTracking().CountAsync();
        var totalActivos = await _db.Beneficiarios.AsNoTracking().CountAsync(b => b.Estado == "A");

        var items = await q
            .OrderBy(b => b.NombreBeneficiario)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(b => MapToDto(b))
            .ToListAsync();

        // Enriquecer con nombres de banco y tipos de cuenta
        var codsBanco = items.Where(i => i.CodBanco.HasValue).Select(i => i.CodBanco!.Value).Distinct().ToList();
        if (codsBanco.Count > 0)
        {
            var bancos = await _db.Bancos.AsNoTracking()
                .Where(b => codsBanco.Contains(b.CodBanco))
                .ToDictionaryAsync(b => b.CodBanco, b => b.NombreBanco);
            foreach (var item in items)
            {
                if (item.CodBanco.HasValue && bancos.TryGetValue(item.CodBanco.Value, out var nombre))
                    item.NombreBanco = nombre;
            }
        }

        var codsTipoCuenta = items.Where(i => i.TipoCuenta.HasValue).Select(i => i.TipoCuenta!.Value).Distinct().ToList();
        if (codsTipoCuenta.Count > 0)
        {
            var tiposCuenta = await _db.TiposCuenta.AsNoTracking()
                .Where(t => codsTipoCuenta.Contains(t.CodTipoCuenta))
                .ToDictionaryAsync(t => t.CodTipoCuenta, t => t.Descripcion);
            foreach (var item in items)
            {
                if (item.TipoCuenta.HasValue && tiposCuenta.TryGetValue(item.TipoCuenta.Value, out var desc))
                    item.TipoCuentaDescripcion = desc;
            }
        }

        // Enriquecer con nombre del funcionario desde tabla FUNCIONARIOS
        var rutsFuncionario = items.Where(i => i.RutFuncionario.HasValue)
            .Select(i => i.RutFuncionario!.Value).Distinct().ToList();
        if (rutsFuncionario.Count > 0)
        {
            var funcionarios = await _db.Funcionarios.AsNoTracking()
                .Where(f => rutsFuncionario.Contains(f.RutFuncionario))
                .ToDictionaryAsync(f => f.RutFuncionario);
            foreach (var item in items)
            {
                if (item.RutFuncionario.HasValue &&
                    funcionarios.TryGetValue(item.RutFuncionario.Value, out var func))
                    item.NombreFuncionario = $"{func.ApellidoPaterno} {func.ApellidoMaterno} {func.Nombres}".Trim();
            }
        }

        // Enriquecer con retenciones activas del ultimo periodo
        var rutsItems = items.Select(i => i.RutBeneficiario).ToList();
        if (rutsItems.Count > 0)
        {
            // Obtener el ultimo periodo disponible
            var ultimoPeriodo = await _db.RetenidosJudiciales.AsNoTracking()
                .Where(r => r.Estado == "A")
                .OrderByDescending(r => r.PeriodoProceso)
                .Select(r => r.PeriodoProceso)
                .FirstOrDefaultAsync();

            if (ultimoPeriodo != null)
            {
                var retencionesPorRut = await _db.RetenidosJudiciales.AsNoTracking()
                    .Where(r => r.Estado == "A"
                        && r.PeriodoProceso == ultimoPeriodo
                        && rutsItems.Contains(r.RutBeneficiario))
                    .GroupBy(r => r.RutBeneficiario)
                    .Select(g => new { Rut = g.Key, Cantidad = g.Count(), Monto = g.Sum(r => r.Monto) })
                    .ToListAsync();

                var retDict = retencionesPorRut.ToDictionary(r => r.Rut);
                foreach (var item in items)
                {
                    if (retDict.TryGetValue(item.RutBeneficiario, out var ret))
                    {
                        item.CantidadRetenciones = ret.Cantidad;
                        item.MontoTotalRetenciones = ret.Monto;
                    }
                }
            }
        }

        return new PagedResult<BeneficiarioDto>
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

    public async Task<BeneficiarioDetalleDto?> ObtenerPorRutAsync(long rut)
    {
        var beneficiario = await _db.Beneficiarios.AsNoTracking()
            .FirstOrDefaultAsync(b => b.RutBeneficiario == rut);

        if (beneficiario == null)
            return null;

        var retenciones = await _db.RetenidosJudiciales.AsNoTracking()
            .Where(r => r.RutBeneficiario == rut && r.Estado == "A")
            .ToListAsync();

        // Obtener nombre del banco y tipo de cuenta en una sola consulta batch
        string? nombreBanco = null;
        string? tipoCuentaDescripcion = null;
        if (beneficiario.CodBanco.HasValue)
        {
            nombreBanco = await _db.Bancos.AsNoTracking()
                .Where(b => b.CodBanco == beneficiario.CodBanco.Value)
                .Select(b => b.NombreBanco)
                .FirstOrDefaultAsync();
        }
        if (beneficiario.TipoCuenta.HasValue)
        {
            tipoCuentaDescripcion = await _db.TiposCuenta.AsNoTracking()
                .Where(t => t.CodTipoCuenta == beneficiario.TipoCuenta.Value)
                .Select(t => t.Descripcion)
                .FirstOrDefaultAsync();
        }

        var dto = new BeneficiarioDetalleDto
        {
            Id = beneficiario.Id,
            RutBeneficiario = beneficiario.RutBeneficiario,
            DvBeneficiario = beneficiario.DvBeneficiario,
            RutFormateado = RutHelper.Formatear(beneficiario.RutBeneficiario, beneficiario.DvBeneficiario),
            NombreBeneficiario = beneficiario.NombreBeneficiario,
            FechaNacimiento = beneficiario.FechaNacimiento,
            Sexo = beneficiario.Sexo,
            EstadoCivil = beneficiario.EstadoCivil,
            Domicilio = beneficiario.Domicilio,
            Comuna = beneficiario.Comuna,
            Telefono = beneficiario.Telefono,
            CtaOtBanco = beneficiario.CtaOtBanco,
            TipoCuenta = beneficiario.TipoCuenta,
            TipoCuentaDescripcion = tipoCuentaDescripcion,
            CodBanco = beneficiario.CodBanco,
            NombreBanco = nombreBanco,
            CtaEstado = beneficiario.CtaEstado,
            Sucursal = beneficiario.Sucursal,
            RutFuncionario = beneficiario.RutFuncionario,
            DvFuncionario = beneficiario.DvFuncionario,
            RutFuncionarioFormateado = beneficiario.RutFuncionario.HasValue && beneficiario.DvFuncionario != null
                ? RutHelper.Formatear(beneficiario.RutFuncionario.Value, beneficiario.DvFuncionario)
                : null,
            Estado = beneficiario.Estado,
            FechaCreacion = beneficiario.FechaCreacion,
            FechaModificacion = beneficiario.FechaModificacion,
            UsuarioCreacion = beneficiario.UsuarioCreacion,
            Retenciones = retenciones.Select(r => new RetencionDto
            {
                Id = r.Id,
                IdRetencion = r.IdRetencion,
                RutTitular = r.RutTitular,
                DvTitular = r.DvTitular,
                RutTitularFormateado = RutHelper.Formatear(r.RutTitular, r.DvTitular),
                Monto = r.Monto,
                CodRetencion = r.CodRetencion,
                TipoPago = r.TipoPago,
                Estado = r.Estado,
                PeriodoProceso = r.PeriodoProceso
            }).ToList()
        };

        // Obtener nombres de funcionarios (para retenciones y para la ficha)
        var rutsTitulares = retenciones.Select(r => r.RutTitular).Distinct().ToList();
        if (beneficiario.RutFuncionario.HasValue && !rutsTitulares.Contains(beneficiario.RutFuncionario.Value))
            rutsTitulares.Add(beneficiario.RutFuncionario.Value);

        var funcionarios = await _db.Funcionarios.AsNoTracking()
            .Where(f => rutsTitulares.Contains(f.RutFuncionario))
            .ToDictionaryAsync(f => f.RutFuncionario);

        foreach (var ret in dto.Retenciones)
        {
            if (funcionarios.TryGetValue(ret.RutTitular, out var func))
                ret.NombreFuncionario = $"{func.ApellidoPaterno} {func.ApellidoMaterno} {func.Nombres}".Trim();
        }

        // Obtener nombre del funcionario desde la tabla FUNCIONARIOS
        if (beneficiario.RutFuncionario.HasValue
            && funcionarios.TryGetValue(beneficiario.RutFuncionario.Value, out var funcBenef))
        {
            dto.NombreFuncionario = $"{funcBenef.ApellidoPaterno} {funcBenef.ApellidoMaterno} {funcBenef.Nombres}".Trim();
        }

        return dto;
    }

    public async Task<BeneficiarioDto> CrearAsync(CrearBeneficiarioRequest request, string usuario)
    {
        if (!RutHelper.Validar(request.RutBeneficiario, request.DvBeneficiario))
            throw new ArgumentException("RUT beneficiario invalido");

        var existe = await _db.Beneficiarios
            .AnyAsync(b => b.RutBeneficiario == request.RutBeneficiario);
        if (existe)
            throw new BusinessConflictException("Ya existe un beneficiario con ese RUT");

        var entity = new Beneficiario
        {
            RutBeneficiario = request.RutBeneficiario,
            DvBeneficiario = request.DvBeneficiario.ToUpper(),
            NombreBeneficiario = request.NombreBeneficiario,
            FechaNacimiento = request.FechaNacimiento,
            Sexo = request.Sexo,
            EstadoCivil = request.EstadoCivil,
            Domicilio = request.Domicilio,
            Comuna = request.Comuna,
            Telefono = request.Telefono,
            TipoCuenta = request.TipoCuenta,
            CodBanco = request.CodBanco,
            CtaEstado = request.CodBanco == _settings.CodBancoEstado ? request.CtaEstado : null,
            CtaOtBanco = request.CodBanco != _settings.CodBancoEstado ? request.CtaOtBanco : null,
            Sucursal = request.Sucursal,
            RutFuncionario = request.RutFuncionario,
            DvFuncionario = request.DvFuncionario?.ToUpper(),
            UsuarioCreacion = usuario
        };

        await using var transaction = await _db.Database.BeginTransactionAsync();

        _db.Beneficiarios.Add(entity);
        await _db.SaveChangesAsync();

        // Auditoria (necesita entity.Id generado por DB)
        _db.AuditoriaCambios.Add(new AuditoriaCambios
        {
            Entidad = "BENEFICIARIO",
            IdEntidad = entity.Id,
            RutAfectado = RutHelper.Formatear(entity.RutBeneficiario, entity.DvBeneficiario),
            Accion = "INSERT",
            Usuario = usuario,
            Fecha = DateTime.Now
        });
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        return MapToDto(entity);
    }

    public async Task<BeneficiarioDto> ActualizarAsync(long rut, ActualizarBeneficiarioRequest request, string usuario)
    {
        var entity = await _db.Beneficiarios
            .FirstOrDefaultAsync(b => b.RutBeneficiario == rut)
            ?? throw new KeyNotFoundException("Beneficiario no encontrado");

        entity.NombreBeneficiario = request.NombreBeneficiario;
        entity.FechaNacimiento = request.FechaNacimiento;
        entity.Sexo = request.Sexo;
        entity.EstadoCivil = request.EstadoCivil;
        entity.Domicilio = request.Domicilio;
        entity.Comuna = request.Comuna;
        entity.Telefono = request.Telefono;
        entity.TipoCuenta = request.TipoCuenta;
        entity.CodBanco = request.CodBanco;
        entity.CtaEstado = request.CodBanco == _settings.CodBancoEstado ? request.CtaEstado : null;
        entity.CtaOtBanco = request.CodBanco != _settings.CodBancoEstado ? request.CtaOtBanco : null;
        entity.Sucursal = request.Sucursal;
        entity.RutFuncionario = request.RutFuncionario;
        entity.DvFuncionario = request.DvFuncionario?.ToUpper();
        entity.FechaModificacion = DateTime.Now;

        _db.AuditoriaCambios.Add(new AuditoriaCambios
        {
            Entidad = "BENEFICIARIO",
            IdEntidad = entity.Id,
            RutAfectado = RutHelper.Formatear(entity.RutBeneficiario, entity.DvBeneficiario),
            Accion = "ACTUALIZAR",
            Usuario = usuario,
            Fecha = DateTime.Now
        });

        await _db.SaveChangesAsync();
        return MapToDto(entity);
    }

    public async Task<bool> InactivarAsync(long rut, string usuario)
    {
        var entity = await _db.Beneficiarios
            .FirstOrDefaultAsync(b => b.RutBeneficiario == rut);

        if (entity == null) return false;

        entity.Estado = "I";
        entity.FechaModificacion = DateTime.Now;

        _db.AuditoriaCambios.Add(new AuditoriaCambios
        {
            Entidad = "BENEFICIARIO",
            IdEntidad = entity.Id,
            RutAfectado = RutHelper.Formatear(entity.RutBeneficiario, entity.DvBeneficiario),
            Accion = "INACTIVAR",
            CampoModificado = "ESTADO",
            ValorAnterior = "A",
            ValorNuevo = "I",
            Usuario = usuario,
            Fecha = DateTime.Now
        });

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<RetencionDto>> ObtenerRetencionesAsync(long rut)
    {
        return await _db.RetenidosJudiciales.AsNoTracking()
            .Where(r => r.RutBeneficiario == rut)
            .Select(r => new RetencionDto
            {
                Id = r.Id,
                IdRetencion = r.IdRetencion,
                RutTitular = r.RutTitular,
                DvTitular = r.DvTitular,
                Monto = r.Monto,
                CodRetencion = r.CodRetencion,
                TipoPago = r.TipoPago,
                Estado = r.Estado,
                PeriodoProceso = r.PeriodoProceso
            })
            .ToListAsync();
    }

    public async Task<List<BeneficiarioDto>> BuscarAsync(string query)
    {
        var busqueda = query.Trim().ToUpper();
        return await _db.Beneficiarios.AsNoTracking()
            .Where(b => b.Estado == "A" &&
                (b.NombreBeneficiario.ToUpper().Contains(busqueda) ||
                 b.RutBeneficiario.ToString().Contains(busqueda)))
            .Take(20)
            .Select(b => MapToDto(b))
            .ToListAsync();
    }

    public async Task<byte[]> ExportarExcelAsync(string? estado = null)
    {
        var beneficiarios = await ObtenerBeneficiariosParaExportar(estado);

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Beneficiarios");

        // Headers
        var headers = new[] { "RUT", "Nombre", "Estado", "Banco", "Tipo Cuenta",
            "Cta Banco Estado", "Cta Otro Banco", "RUT Funcionario", "Nombre Funcionario",
            "Retenciones", "Monto Retenciones", "Fecha Creacion" };
        for (int c = 0; c < headers.Length; c++)
        {
            ws.Cells[1, c + 1].Value = headers[c];
            ws.Cells[1, c + 1].Style.Font.Bold = true;
            ws.Cells[1, c + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            ws.Cells[1, c + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(44, 62, 80));
            ws.Cells[1, c + 1].Style.Font.Color.SetColor(System.Drawing.Color.White);
        }

        for (int i = 0; i < beneficiarios.Count; i++)
        {
            var b = beneficiarios[i];
            int row = i + 2;
            ws.Cells[row, 1].Value = b.RutFormateado;
            ws.Cells[row, 2].Value = b.NombreBeneficiario;
            ws.Cells[row, 3].Value = b.Estado == "A" ? "Activo" : "Inactivo";
            ws.Cells[row, 4].Value = b.NombreBanco ?? b.CodBanco?.ToString();
            ws.Cells[row, 5].Value = b.TipoCuentaDescripcion;
            ws.Cells[row, 6].Value = b.CtaEstado;
            ws.Cells[row, 7].Value = b.CtaOtBanco;
            ws.Cells[row, 8].Value = b.RutFuncionarioFormateado;
            ws.Cells[row, 9].Value = b.NombreFuncionario;
            ws.Cells[row, 10].Value = b.CantidadRetenciones;
            ws.Cells[row, 11].Value = b.MontoTotalRetenciones;
            ws.Cells[row, 11].Style.Numberformat.Format = "#,##0";
            ws.Cells[row, 12].Value = b.FechaCreacion?.ToString("dd/MM/yyyy");
        }

        ws.Cells[ws.Dimension.Address].AutoFitColumns();
        return package.GetAsByteArray();
    }

    public async Task<byte[]> ExportarCsvAsync(string? estado = null)
    {
        var beneficiarios = await ObtenerBeneficiariosParaExportar(estado);

        var sb = new StringBuilder();
        sb.AppendLine("RUT;Nombre;Estado;Banco;Tipo Cuenta;Cta Banco Estado;Cta Otro Banco;RUT Funcionario;Nombre Funcionario;Retenciones;Monto Retenciones;Fecha Creacion");

        foreach (var b in beneficiarios)
        {
            sb.AppendLine(string.Join(";",
                b.RutFormateado,
                b.NombreBeneficiario,
                b.Estado == "A" ? "Activo" : "Inactivo",
                b.NombreBanco ?? b.CodBanco?.ToString() ?? "",
                b.TipoCuentaDescripcion ?? "",
                b.CtaEstado ?? "",
                b.CtaOtBanco ?? "",
                b.RutFuncionarioFormateado ?? "",
                b.NombreFuncionario ?? "",
                b.CantidadRetenciones,
                b.MontoTotalRetenciones,
                b.FechaCreacion?.ToString("dd/MM/yyyy") ?? ""
            ));
        }

        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    private async Task<List<BeneficiarioDto>> ObtenerBeneficiariosParaExportar(string? estado)
    {
        var q = _db.Beneficiarios.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(estado))
            q = q.Where(b => b.Estado == estado);

        var items = await q.OrderBy(b => b.NombreBeneficiario)
            .Select(b => MapToDto(b))
            .ToListAsync();

        // Enriquecer con bancos
        var codsBanco = items.Where(i => i.CodBanco.HasValue).Select(i => i.CodBanco!.Value).Distinct().ToList();
        if (codsBanco.Count > 0)
        {
            var bancos = await _db.Bancos.AsNoTracking()
                .Where(b => codsBanco.Contains(b.CodBanco))
                .ToDictionaryAsync(b => b.CodBanco, b => b.NombreBanco);
            foreach (var item in items)
            {
                if (item.CodBanco.HasValue && bancos.TryGetValue(item.CodBanco.Value, out var nombre))
                    item.NombreBanco = nombre;
            }
        }

        // Enriquecer con nombre del funcionario desde tabla FUNCIONARIOS
        var rutsFuncionario = items.Where(i => i.RutFuncionario.HasValue)
            .Select(i => i.RutFuncionario!.Value).Distinct().ToList();
        if (rutsFuncionario.Count > 0)
        {
            var funcionarios = await _db.Funcionarios.AsNoTracking()
                .Where(f => rutsFuncionario.Contains(f.RutFuncionario))
                .ToDictionaryAsync(f => f.RutFuncionario);
            foreach (var item in items)
            {
                if (item.RutFuncionario.HasValue &&
                    funcionarios.TryGetValue(item.RutFuncionario.Value, out var func))
                    item.NombreFuncionario = $"{func.ApellidoPaterno} {func.ApellidoMaterno} {func.Nombres}".Trim();
            }
        }

        // Enriquecer con retenciones del ultimo periodo
        var ultimoPeriodo = await _db.RetenidosJudiciales.AsNoTracking()
            .Where(r => r.Estado == "A")
            .OrderByDescending(r => r.PeriodoProceso)
            .Select(r => r.PeriodoProceso)
            .FirstOrDefaultAsync();

        if (ultimoPeriodo != null)
        {
            var rutsItems = items.Select(i => i.RutBeneficiario).ToList();
            var retencionesPorRut = await _db.RetenidosJudiciales.AsNoTracking()
                .Where(r => r.Estado == "A" && r.PeriodoProceso == ultimoPeriodo
                    && rutsItems.Contains(r.RutBeneficiario))
                .GroupBy(r => r.RutBeneficiario)
                .Select(g => new { Rut = g.Key, Cantidad = g.Count(), Monto = g.Sum(r => r.Monto) })
                .ToListAsync();

            var retDict = retencionesPorRut.ToDictionary(r => r.Rut);
            foreach (var item in items)
            {
                if (retDict.TryGetValue(item.RutBeneficiario, out var ret))
                {
                    item.CantidadRetenciones = ret.Cantidad;
                    item.MontoTotalRetenciones = ret.Monto;
                }
            }
        }

        return items;
    }

    private static BeneficiarioDto MapToDto(Beneficiario b) => new()
    {
        Id = b.Id,
        RutBeneficiario = b.RutBeneficiario,
        DvBeneficiario = b.DvBeneficiario,
        RutFormateado = RutHelper.Formatear(b.RutBeneficiario, b.DvBeneficiario),
        NombreBeneficiario = b.NombreBeneficiario,
        Sexo = b.Sexo,
        EstadoCivil = b.EstadoCivil,
        Domicilio = b.Domicilio,
        Comuna = b.Comuna,
        Telefono = b.Telefono,
        CtaOtBanco = b.CtaOtBanco,
        TipoCuenta = b.TipoCuenta,
        TipoCuentaDescripcion = b.TipoCuenta switch
        {
            1 => "Cuenta Corriente",
            2 => "Cuenta de Ahorro / CuentaRUT",
            3 => "Cuenta Vista",
            _ => null
        },
        CodBanco = b.CodBanco,
        CtaEstado = b.CtaEstado,
        Sucursal = b.Sucursal,
        RutFuncionario = b.RutFuncionario,
        DvFuncionario = b.DvFuncionario,
        RutFuncionarioFormateado = b.RutFuncionario.HasValue && b.DvFuncionario != null
            ? RutHelper.Formatear(b.RutFuncionario.Value, b.DvFuncionario)
            : null,
        Estado = b.Estado,
        FechaCreacion = b.FechaCreacion
    };
}
