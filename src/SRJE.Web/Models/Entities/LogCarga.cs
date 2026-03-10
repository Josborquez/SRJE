namespace SRJE.Web.Models.Entities;

public class LogCarga
{
    public long Id { get; set; }
    public string TipoCarga { get; set; } = string.Empty;
    public string? NombreArchivo { get; set; }
    public string? HashArchivo { get; set; }
    public long? TamanioBytes { get; set; }
    public string? PeriodoProceso { get; set; }
    public DateTime FechaInicio { get; set; } = DateTime.Now;
    public DateTime? FechaFin { get; set; }
    public long? DuracionMs { get; set; }
    public string Estado { get; set; } = "P";
    public int? TotalLineas { get; set; }
    public int? RegistrosInsertados { get; set; }
    public int? RegistrosActualizados { get; set; }
    public int? RegistrosExcluidos { get; set; }
    public int? RegistrosError { get; set; }
    public decimal? MontoTotal { get; set; }
    public string? Usuario { get; set; }
    public string? IpUsuario { get; set; }
    public string? Observaciones { get; set; }
    public string PuedeRevertir { get; set; } = "S";

    public ICollection<LogCargaDetalle> Detalles { get; set; } = new List<LogCargaDetalle>();
}
