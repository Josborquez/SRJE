namespace SRJE.Web.Models.Entities;

public class DetallePagoTemge
{
    public long Id { get; set; }
    public long IdHistorial { get; set; }
    public long? IdRetenidoJudicial { get; set; }
    public long RutBeneficiario { get; set; }
    public decimal? MontoPagado { get; set; }
    public long? CodBanco { get; set; }
    public long? TipoCuenta { get; set; }
    public string EstadoLinea { get; set; } = "P";
    public string? MotivoExclusion { get; set; }

    public HistorialPagosTemge? Historial { get; set; }
}
