namespace SRJE.Web.Models.Entities;

public class Banco
{
    public long CodBanco { get; set; }
    public string NombreBanco { get; set; } = string.Empty;
    public string UsaCtaOtBanco { get; set; } = "S";
    public string Activo { get; set; } = "S";
}
