using Microsoft.EntityFrameworkCore;
using SRJE.Web.Infrastructure.Data;
using SRJE.Web.Models;

namespace SRJE.Web.Services;

/// <summary>
/// Autenticacion contra tabla USUARIOS_SISTEMA (Oracle) con BCrypt.
/// Se activa con Auth:Provider = "Database".
/// </summary>
public class DbAuthService : IAuthService
{
    // Hash dummy para igualar el tiempo de respuesta cuando el usuario no existe
    // (mitiga enumeracion de usuarios por timing)
    private static readonly string DummyHash = BCrypt.Net.BCrypt.HashPassword("dummy");

    private readonly SrjeDbContext _db;

    public DbAuthService(SrjeDbContext db)
    {
        _db = db;
    }

    public async Task<UsuarioInfo?> ValidarCredencialesAsync(string usuario, string password)
    {
        if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(password))
            return null;

        var normalizado = usuario.Trim().ToLower();
        var user = await _db.UsuariosSistema.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Usuario == normalizado && u.Estado == "A");

        if (!BCrypt.Net.BCrypt.Verify(password, user?.PasswordHash ?? DummyHash) || user == null)
            return null;

        return new UsuarioInfo
        {
            Usuario = user.Usuario,
            NombreCompleto = user.NombreCompleto,
            Rol = user.Rol
        };
    }
}
