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

public class CrearCuentaBeneficiarioRequest
{
    [Required]
    public long CodBanco { get; set; }

    [Required]
    public long TipoCuenta { get; set; }

    [Required, MaxLength(15)]
    public string NumeroCuenta { get; set; } = string.Empty;

    [MaxLength(60)]
    public string? Alias { get; set; }
}

public class ActualizarCuentaBeneficiarioRequest : CrearCuentaBeneficiarioRequest
{
}

public class BuscarBeneficiarioQuery
{
    public string? Q { get; set; }

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 20;

    public string? Estado { get; set; }
}
