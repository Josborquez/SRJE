namespace SRJE.Web.Models.Entities;

public class CuentaBeneficiario
{
    public long Id { get; set; }
    public long RutBeneficiario { get; set; }
    public long CodBanco { get; set; }
    public long TipoCuenta { get; set; }
    public string NumeroCuenta { get; set; } = string.Empty;
    public string? Alias { get; set; }
    public int Orden { get; set; } = 1;
    public string Estado { get; set; } = "A";
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public string? UsuarioCreacion { get; set; }
}
