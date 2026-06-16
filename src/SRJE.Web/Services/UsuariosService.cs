using Microsoft.EntityFrameworkCore;
using SRJE.Web.Infrastructure.Data;
using SRJE.Web.Models.Entities;
using SRJE.Web.Models.Exceptions;
using SRJE.Web.Models.Requests;
using SRJE.Web.Models.ViewModels;

namespace SRJE.Web.Services;

public class UsuariosService : IUsuariosService
{
    private static readonly string[] RolesValidos = { "admin", "operador", "consulta" };

    private readonly SrjeDbContext _db;

    public UsuariosService(SrjeDbContext db)
    {
        _db = db;
    }

    public async Task<List<UsuarioSistemaDto>> ListarAsync()
    {
        return await _db.UsuariosSistema.AsNoTracking()
            .OrderBy(u => u.Usuario)
            .Select(u => MapToDto(u))
            .ToListAsync();
    }

    public async Task<UsuarioSistemaDto> ObtenerAsync(string usuario)
    {
        var user = await BuscarAsync(usuario);
        return MapToDto(user);
    }

    public async Task<UsuarioSistemaDto> CrearAsync(CrearUsuarioRequest request)
    {
        var normalizado = request.Usuario.Trim().ToLower();
        if (string.IsNullOrEmpty(normalizado))
            throw new ArgumentException("El nombre de usuario es obligatorio");

        ValidarRol(request.Rol);

        // CountAsync en vez de AnyAsync por compatibilidad Oracle
        var existe = await _db.UsuariosSistema.CountAsync(u => u.Usuario == normalizado) > 0;
        if (existe)
            throw new BusinessConflictException($"El usuario '{normalizado}' ya existe");

        var user = new UsuarioSistema
        {
            Usuario = normalizado,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            NombreCompleto = request.NombreCompleto.Trim(),
            Rol = request.Rol,
            Estado = "A",
            FechaCreacion = DateTime.Now
        };

        _db.UsuariosSistema.Add(user);
        await _db.SaveChangesAsync();
        return MapToDto(user);
    }

    public async Task<UsuarioSistemaDto> ActualizarAsync(string usuario, ActualizarUsuarioRequest request, string usuarioActual)
    {
        ValidarRol(request.Rol);

        var user = await BuscarAsync(usuario);

        if (user.Usuario == usuarioActual.Trim().ToLower() && user.Rol != request.Rol)
            throw new ArgumentException("No puede cambiar su propio rol");

        user.NombreCompleto = request.NombreCompleto.Trim();
        user.Rol = request.Rol;
        await _db.SaveChangesAsync();
        return MapToDto(user);
    }

    public async Task CambiarPasswordAsync(string usuario, string password)
    {
        var user = await BuscarAsync(usuario);
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
        await _db.SaveChangesAsync();
    }

    public async Task<UsuarioSistemaDto> ToggleEstadoAsync(string usuario, string usuarioActual)
    {
        var user = await BuscarAsync(usuario);

        if (user.Usuario == usuarioActual.Trim().ToLower())
            throw new ArgumentException("No puede inactivar su propia cuenta");

        user.Estado = user.Estado == "A" ? "I" : "A";
        await _db.SaveChangesAsync();
        return MapToDto(user);
    }

    public async Task<PagedResult<AccesoDto>> ListarAccesosAsync(BuscarAccesosQuery query)
    {
        var q = _db.LogAccesos.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(query.Usuario))
        {
            var usuario = query.Usuario.Trim().ToLower();
            q = q.Where(a => a.Usuario == usuario);
        }

        if (!string.IsNullOrEmpty(query.Evento))
            q = q.Where(a => a.Evento == query.Evento);

        if (query.Desde.HasValue)
            q = q.Where(a => a.Fecha >= query.Desde.Value);

        if (query.Hasta.HasValue)
            q = q.Where(a => a.Fecha < query.Hasta.Value.Date.AddDays(1));

        var total = await q.CountAsync();
        var items = await q
            .OrderByDescending(a => a.Fecha)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(a => new AccesoDto
            {
                Id = a.Id,
                Usuario = a.Usuario,
                Evento = a.Evento,
                Ip = a.Ip,
                Fecha = a.Fecha
            })
            .ToListAsync();

        return new PagedResult<AccesoDto>
        {
            Items = items,
            TotalCount = total,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }

    public async Task<PagedResult<CargaResumenDto>> ListarCargasAsync(BuscarCargasQuery query)
    {
        var q = _db.LogCargas.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(query.Usuario))
        {
            var usuario = query.Usuario.Trim().ToLower();
            q = q.Where(c => c.Usuario != null && c.Usuario.ToLower() == usuario);
        }

        var total = await q.CountAsync();
        var items = await q
            .OrderByDescending(c => c.FechaInicio)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(c => new CargaResumenDto
            {
                Id = c.Id,
                TipoCarga = c.TipoCarga,
                NombreArchivo = c.NombreArchivo,
                FechaInicio = c.FechaInicio,
                Estado = c.Estado,
                TotalLineas = c.TotalLineas,
                RegistrosInsertados = c.RegistrosInsertados,
                RegistrosActualizados = c.RegistrosActualizados,
                RegistrosError = c.RegistrosError,
                Usuario = c.Usuario,
                IpUsuario = c.IpUsuario
            })
            .ToListAsync();

        return new PagedResult<CargaResumenDto>
        {
            Items = items,
            TotalCount = total,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }

    private async Task<UsuarioSistema> BuscarAsync(string usuario)
    {
        var normalizado = usuario.Trim().ToLower();
        var user = await _db.UsuariosSistema.FirstOrDefaultAsync(u => u.Usuario == normalizado);
        if (user == null)
            throw new KeyNotFoundException($"Usuario '{normalizado}' no encontrado");
        return user;
    }

    private static void ValidarRol(string rol)
    {
        if (!RolesValidos.Contains(rol))
            throw new ArgumentException($"Rol invalido: '{rol}'. Valores permitidos: admin, operador, consulta");
    }

    private static UsuarioSistemaDto MapToDto(UsuarioSistema u) => new()
    {
        Usuario = u.Usuario,
        NombreCompleto = u.NombreCompleto,
        Rol = u.Rol,
        Estado = u.Estado,
        FechaCreacion = u.FechaCreacion
    };
}
