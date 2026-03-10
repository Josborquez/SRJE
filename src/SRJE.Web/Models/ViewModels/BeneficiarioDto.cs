namespace SRJE.Web.Models.ViewModels;

public class BeneficiarioDto
{
    public long Id { get; set; }
    public long RutBeneficiario { get; set; }
    public string DvBeneficiario { get; set; } = string.Empty;
    public string RutFormateado { get; set; } = string.Empty;
    public string NombreBeneficiario { get; set; } = string.Empty;
    public string? Sexo { get; set; }
    public string? EstadoCivil { get; set; }
    public string? Domicilio { get; set; }
    public string? Comuna { get; set; }
    public string? Telefono { get; set; }
    public string? CtaOtBanco { get; set; }
    public long? TipoCuenta { get; set; }
    public string? TipoCuentaDescripcion { get; set; }
    public long? CodBanco { get; set; }
    public string? NombreBanco { get; set; }
    public string? CtaEstado { get; set; }
    public string? Sucursal { get; set; }
    public long? RutFuncionario { get; set; }
    public string? DvFuncionario { get; set; }
    public string? RutFuncionarioFormateado { get; set; }
    public string? NombreFuncionario { get; set; }
    public string Estado { get; set; } = "A";
    public DateTime? FechaCreacion { get; set; }
}

public class BeneficiarioDetalleDto : BeneficiarioDto
{
    public DateTime? FechaNacimiento { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public string? UsuarioCreacion { get; set; }
    public List<RetencionDto> Retenciones { get; set; } = new();
}

public class RetencionDto
{
    public long Id { get; set; }
    public long IdRetencion { get; set; }
    public long RutTitular { get; set; }
    public string DvTitular { get; set; } = string.Empty;
    public string? NombreFuncionario { get; set; }
    public decimal Monto { get; set; }
    public string? CodRetencion { get; set; }
    public string? TipoPago { get; set; }
    public string Estado { get; set; } = "A";
    public string? PeriodoProceso { get; set; }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
