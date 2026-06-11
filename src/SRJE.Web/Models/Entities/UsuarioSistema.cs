namespace SRJE.Web.Models.Entities;

public class UsuarioSistema
{
    public string Usuario { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Rol { get; set; } = "consulta";
    public string Estado { get; set; } = "A";
    public DateTime FechaCreacion { get; set; } // default SYSDATE en BD
}
