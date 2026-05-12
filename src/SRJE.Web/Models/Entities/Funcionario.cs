namespace SRJE.Web.Models.Entities;

public class Funcionario
{
    public long Id { get; set; }
    public long RutFuncionario { get; set; }
    public string DvFuncionario { get; set; } = string.Empty;
    public string? ApellidoPaterno { get; set; }
    public string? ApellidoMaterno { get; set; }
    public string? Nombres { get; set; }
    public string? IdSistema { get; set; }
    public string Activo { get; set; } = "S";
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? FechaModificacion { get; set; }
    public string? UsuarioCreacion { get; set; }
}
