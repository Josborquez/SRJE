namespace SRJE.Web.Models.Entities;

public class AuditoriaCambios
{
    public long Id { get; set; }
    public string Entidad { get; set; } = string.Empty;
    public long IdEntidad { get; set; }
    public string? RutAfectado { get; set; }
    public string Accion { get; set; } = string.Empty;
    public string? CampoModificado { get; set; }
    public string? ValorAnterior { get; set; }
    public string? ValorNuevo { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public DateTime Fecha { get; set; } = DateTime.Now;
    public string? Ip { get; set; }
    public string? Motivo { get; set; }
}
