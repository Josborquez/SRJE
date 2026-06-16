namespace SRJE.Web.Services;

public interface IRegistroAccesoService
{
    /// <summary>Registra un evento de acceso (login_ok, login_fail, logout). Nunca lanza excepcion.</summary>
    Task RegistrarAsync(string usuario, string evento, string? ip);
}
