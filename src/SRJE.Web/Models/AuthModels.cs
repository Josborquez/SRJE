namespace SRJE.Web.Models;

public class LoginRequest
{
    public string Usuario { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class UsuarioInfo
{
    public string Usuario { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
}
