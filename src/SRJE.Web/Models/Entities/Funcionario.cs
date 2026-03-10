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
}
