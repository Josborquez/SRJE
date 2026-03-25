namespace SRJE.Web.Models.ViewModels;

public class FuncionarioDto
{
    public long Id { get; set; }
    public long RutFuncionario { get; set; }
    public string DvFuncionario { get; set; } = string.Empty;
    public string RutFormateado { get; set; } = string.Empty;
    public string? ApellidoPaterno { get; set; }
    public string? ApellidoMaterno { get; set; }
    public string? Nombres { get; set; }
    public string NombreCompleto => $"{ApellidoPaterno} {ApellidoMaterno} {Nombres}".Trim();
    public string? IdSistema { get; set; }
    public string Activo { get; set; } = "S";
    public int CantidadBeneficiarios { get; set; }
    public decimal MontoTotalRetenciones { get; set; }
}

public class FuncionarioDetalleDto : FuncionarioDto
{
    public List<BeneficiarioResumenDto> Beneficiarios { get; set; } = new();
}

public class BeneficiarioResumenDto
{
    public long RutBeneficiario { get; set; }
    public string DvBeneficiario { get; set; } = string.Empty;
    public string RutFormateado { get; set; } = string.Empty;
    public string NombreBeneficiario { get; set; } = string.Empty;
    public long? CodBanco { get; set; }
    public string? NombreBanco { get; set; }
    public long? TipoCuenta { get; set; }
    public string? TipoCuentaDescripcion { get; set; }
    public string? CtaEstado { get; set; }
    public string? CtaOtBanco { get; set; }
    public List<RetencionResumenDto> Retenciones { get; set; } = new();
}

public class RetencionResumenDto
{
    public long Id { get; set; }
    public decimal Monto { get; set; }
    public string? CodRetencion { get; set; }
    public string? TipoPago { get; set; }
    public string? PeriodoProceso { get; set; }
    public string Estado { get; set; } = "A";
}

public class FuncionarioStatsDto
{
    public int TotalFuncionarios { get; set; }
    public int TotalActivos { get; set; }
    public int TotalInactivos { get; set; }
    public decimal MontoMensualTotal { get; set; }
    public string? PeriodoActual { get; set; }
}
