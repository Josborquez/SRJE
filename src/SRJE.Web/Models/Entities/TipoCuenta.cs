namespace SRJE.Web.Models.Entities;

public class TipoCuenta
{
    public long CodTipoCuenta { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string Activo { get; set; } = "S";
}
