using System.ComponentModel.DataAnnotations;

namespace SRJE.Web.Models.Requests;

public class CrearBeneficiarioRequest
{
    [Required]
    public long RutBeneficiario { get; set; }

    [Required, MaxLength(1)]
    public string DvBeneficiario { get; set; } = string.Empty;

    [Required, MaxLength(39)]
    public string NombreBeneficiario { get; set; } = string.Empty;

    public DateTime? FechaNacimiento { get; set; }
    public string? Sexo { get; set; }
    public string? EstadoCivil { get; set; }

    [MaxLength(100)]
    public string? Domicilio { get; set; }

    [MaxLength(50)]
    public string? Comuna { get; set; }

    [MaxLength(20)]
    public string? Telefono { get; set; }

    [MaxLength(15)]
    public string? CtaOtBanco { get; set; }

    public long? TipoCuenta { get; set; }
    public long? CodBanco { get; set; }

    [MaxLength(15)]
    public string? CtaEstado { get; set; }

    [MaxLength(60)]
    public string? Sucursal { get; set; }
}

public class ActualizarBeneficiarioRequest : CrearBeneficiarioRequest
{
}

public class BuscarBeneficiarioQuery
{
    public string? Q { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Estado { get; set; }
}
