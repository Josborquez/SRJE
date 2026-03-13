using SRJE.Web.Models;

namespace SRJE.Web.Services;

/// <summary>
/// Servicio de autenticacion para desarrollo.
/// Usuarios hardcodeados en memoria — reemplazar por otro IAuthService en produccion.
/// </summary>
public class DevAuthService : IAuthService
{
    private static readonly List<(string Usuario, string Password, string Nombre, string Rol)> Usuarios =
    [
        ("admin", "admin123", "Administrador SRJE", "admin"),
        ("operador", "operador123", "Operador SRJE", "operador"),
        ("consulta", "consulta123", "Usuario Consulta", "consulta")
    ];

    public Task<UsuarioInfo?> ValidarCredencialesAsync(string usuario, string password)
    {
        var user = Usuarios.FirstOrDefault(u =>
            u.Usuario.Equals(usuario, StringComparison.OrdinalIgnoreCase) &&
            u.Password == password);

        if (user == default)
            return Task.FromResult<UsuarioInfo?>(null);

        return Task.FromResult<UsuarioInfo?>(new UsuarioInfo
        {
            Usuario = user.Usuario,
            NombreCompleto = user.Nombre,
            Rol = user.Rol
        });
    }
}
