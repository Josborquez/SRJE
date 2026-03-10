namespace SRJE.Web.Models.Entities;

public class LogCargaDetalle
{
    public long Id { get; set; }
    public long IdCarga { get; set; }
    public int? NumeroLinea { get; set; }
    public string? RutReferencia { get; set; }
    public string? Accion { get; set; }
    public string? Estado { get; set; }
    public string? DatosOriginales { get; set; }
    public string? DatosAnteriores { get; set; }
    public string? DatosNuevos { get; set; }
    public string? Mensajes { get; set; }

    public LogCarga? Carga { get; set; }
}
