using Microsoft.EntityFrameworkCore;
using SRJE.Web.Helpers;
using SRJE.Web.Infrastructure.Data;
using SRJE.Web.Models.Entities;
using SRJE.Web.Models.Requests;
using SRJE.Web.Models.ViewModels;

namespace SRJE.Web.Services;

public class BeneficiarioService : IBeneficiarioService
{
    private readonly SrjeDbContext _db;

    public BeneficiarioService(SrjeDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<BeneficiarioDto>> ListarAsync(BuscarBeneficiarioQuery query)
    {
        var q = _db.Beneficiarios.AsQueryable();

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

        var items = await q
            .OrderBy(b => b.NombreBeneficiario)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(b => MapToDto(b))
            .ToListAsync();

        return new PagedResult<BeneficiarioDto>
        {
            Items = items,
            TotalCount = total,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }

    public async Task<BeneficiarioDetalleDto?> ObtenerPorRutAsync(long rut)
    {
        var beneficiario = await _db.Beneficiarios
            .FirstOrDefaultAsync(b => b.RutBeneficiario == rut);

        if (beneficiario == null)
            return null;

        var retenciones = await _db.RetenidosJudiciales
            .Where(r => r.RutBeneficiario == rut && r.Estado == "A")
            .ToListAsync();

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
            CodBanco = beneficiario.CodBanco,
            CtaEstado = beneficiario.CtaEstado,
            Sucursal = beneficiario.Sucursal,
            RutFuncionario = beneficiario.RutFuncionario,
            DvFuncionario = beneficiario.DvFuncionario,
            RutFuncionarioFormateado = beneficiario.RutFuncionario.HasValue && beneficiario.DvFuncionario != null
                ? RutHelper.Formatear(beneficiario.RutFuncionario.Value, beneficiario.DvFuncionario)
                : null,
            NombreFuncionario = beneficiario.NombreFuncionario,
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
                Monto = r.Monto,
                CodRetencion = r.CodRetencion,
                TipoPago = r.TipoPago,
                Estado = r.Estado,
                PeriodoProceso = r.PeriodoProceso
            }).ToList()
        };

        // Obtener nombres de funcionarios
        var rutsTitulares = retenciones.Select(r => r.RutTitular).Distinct().ToList();
        var funcionarios = await _db.Funcionarios
            .Where(f => rutsTitulares.Contains(f.RutFuncionario))
            .ToDictionaryAsync(f => f.RutFuncionario);

        foreach (var ret in dto.Retenciones)
        {
            if (funcionarios.TryGetValue(ret.RutTitular, out var func))
                ret.NombreFuncionario = $"{func.ApellidoPaterno} {func.ApellidoMaterno} {func.Nombres}".Trim();
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
            throw new InvalidOperationException("Ya existe un beneficiario con ese RUT");

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
            CtaOtBanco = request.CtaOtBanco,
            TipoCuenta = request.TipoCuenta,
            CodBanco = request.CodBanco,
            CtaEstado = request.CodBanco == 12 ? request.CtaEstado : null,
            Sucursal = request.Sucursal,
            RutFuncionario = request.RutFuncionario,
            DvFuncionario = request.DvFuncionario?.ToUpper(),
            NombreFuncionario = request.NombreFuncionario,
            UsuarioCreacion = usuario
        };

        _db.Beneficiarios.Add(entity);
        await _db.SaveChangesAsync();

        // Auditoria
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
        entity.CtaOtBanco = request.CtaOtBanco;
        entity.TipoCuenta = request.TipoCuenta;
        entity.CodBanco = request.CodBanco;
        entity.CtaEstado = request.CodBanco == 12 ? request.CtaEstado : null;
        entity.Sucursal = request.Sucursal;
        entity.RutFuncionario = request.RutFuncionario;
        entity.DvFuncionario = request.DvFuncionario?.ToUpper();
        entity.NombreFuncionario = request.NombreFuncionario;
        entity.FechaModificacion = DateTime.Now;

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
        return await _db.RetenidosJudiciales
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
        return await _db.Beneficiarios
            .Where(b => b.Estado == "A" &&
                (b.NombreBeneficiario.ToUpper().Contains(busqueda) ||
                 b.RutBeneficiario.ToString().Contains(busqueda)))
            .Take(20)
            .Select(b => MapToDto(b))
            .ToListAsync();
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
        CodBanco = b.CodBanco,
        CtaEstado = b.CtaEstado,
        Sucursal = b.Sucursal,
        RutFuncionario = b.RutFuncionario,
        DvFuncionario = b.DvFuncionario,
        RutFuncionarioFormateado = b.RutFuncionario.HasValue && b.DvFuncionario != null
            ? RutHelper.Formatear(b.RutFuncionario.Value, b.DvFuncionario)
            : null,
        NombreFuncionario = b.NombreFuncionario,
        Estado = b.Estado,
        FechaCreacion = b.FechaCreacion
    };
}
