namespace SRJE.Web.Models.Entities;

public class Beneficiario
{
    public long Id { get; set; }
    public long RutBeneficiario { get; set; }
    public string DvBeneficiario { get; set; } = string.Empty;
    public string NombreBeneficiario { get; set; } = string.Empty;
    public DateTime? FechaNacimiento { get; set; }
    public string? Sexo { get; set; }
    public string? EstadoCivil { get; set; }
    public string? Domicilio { get; set; }
    public string? Comuna { get; set; }
    public string? Telefono { get; set; }
    public string? CtaOtBanco { get; set; }
    public long? TipoCuenta { get; set; }
    public long? CodBanco { get; set; }
    public string? CtaEstado { get; set; }
    public string? Sucursal { get; set; }
    public long? RutFuncionario { get; set; }
    public string? DvFuncionario { get; set; }
    public string? NombreFuncionario { get; set; }
    public string Estado { get; set; } = "A";
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? FechaModificacion { get; set; }
    public string? UsuarioCreacion { get; set; }
}
