using SRJE.Web.Models;

namespace SRJE.Web.Services;

/// <summary>
/// Interfaz de autenticacion pluggable.
/// En desarrollo se usa DevAuthService con usuarios en memoria.
/// En produccion se puede reemplazar por LDAP, OAuth, Active Directory, etc.
/// </summary>
public interface IAuthService
{
    Task<UsuarioInfo?> ValidarCredencialesAsync(string usuario, string password);
}
