namespace SRJE.Web.Models.ViewModels;

public class AuditoriaComparacionDto
{
    public List<AuditoriaItemDto> SoloEnArchivo { get; set; } = new();
    public List<AuditoriaItemDto> SoloEnSistema { get; set; } = new();
    public List<AuditoriaDiferenciaDto> ConDiferencias { get; set; } = new();
    public int TotalArchivo { get; set; }
    public int TotalSistema { get; set; }
    public int TotalCoincidentes { get; set; }
}

public class AuditoriaItemDto
{
    public long RutBeneficiario { get; set; }
    public string DvBeneficiario { get; set; } = string.Empty;
    public string RutFormateado { get; set; } = string.Empty;
    public string NombreBeneficiario { get; set; } = string.Empty;
    public string? RutFuncionarioFormateado { get; set; }
    public string? Estado { get; set; }
}

public class AuditoriaDiferenciaDto
{
    public long RutBeneficiario { get; set; }
    public string DvBeneficiario { get; set; } = string.Empty;
    public string RutFormateado { get; set; } = string.Empty;
    public string NombreArchivo { get; set; } = string.Empty;
    public string NombreSistema { get; set; } = string.Empty;
    public List<CampoDiferencia> Diferencias { get; set; } = new();
}

public class CampoDiferencia
{
    public string Campo { get; set; } = string.Empty;
    public string? ValorArchivo { get; set; }
    public string? ValorSistema { get; set; }
}
