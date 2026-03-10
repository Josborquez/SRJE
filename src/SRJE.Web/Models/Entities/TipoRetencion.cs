namespace SRJE.Web.Models.Entities;

public class TipoRetencion
{
    public string CodRetencion { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Moneda { get; set; } = "CLP";
    public string Activo { get; set; } = "S";
}
