using System.ComponentModel.DataAnnotations;

namespace SRJE.Web.Models.Requests;

public class CrearBancoRequest
{
    [Required]
    public long CodBanco { get; set; }

    [Required, MaxLength(100)]
    public string NombreBanco { get; set; } = string.Empty;

    [MaxLength(1)]
    public string UsaCtaOtBanco { get; set; } = "S";
}

public class ActualizarBancoRequest
{
    [Required, MaxLength(100)]
    public string NombreBanco { get; set; } = string.Empty;

    [MaxLength(1)]
    public string UsaCtaOtBanco { get; set; } = "S";

    [MaxLength(1)]
    public string Activo { get; set; } = "S";
}

public class CrearTipoCuentaRequest
{
    [Required]
    public long CodTipoCuenta { get; set; }

    [Required, MaxLength(100)]
    public string Descripcion { get; set; } = string.Empty;
}

public class ActualizarTipoCuentaRequest
{
    [Required, MaxLength(100)]
    public string Descripcion { get; set; } = string.Empty;

    [MaxLength(1)]
    public string Activo { get; set; } = "S";
}
