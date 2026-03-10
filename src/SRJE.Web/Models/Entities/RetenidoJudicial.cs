namespace SRJE.Web.Models.Entities;

public class RetenidoJudicial
{
    public long Id { get; set; }
    public long IdRetencion { get; set; }
    public long RutTitular { get; set; }
    public string DvTitular { get; set; } = string.Empty;
    public long RutBeneficiario { get; set; }
    public string DvBeneficiario { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string? CodRetencion { get; set; }
    public string? TipoPago { get; set; }
    public string Estado { get; set; } = "A";
    public DateTime? FechaVigencia { get; set; }
    public string? PeriodoProceso { get; set; }
}
