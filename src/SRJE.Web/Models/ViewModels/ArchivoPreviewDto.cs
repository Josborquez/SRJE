namespace SRJE.Web.Models.ViewModels;

public class ArchivoPreviewDto
{
    public List<PreviewLineaDto> Lineas { get; set; } = new();
    public int TotalLineas { get; set; }
    public int LineasOk { get; set; }
    public int LineasAdvertencia { get; set; }
    public int LineasError { get; set; }
    public int LineasNuevas { get; set; }
    public int LineasMulticuenta { get; set; }
    public decimal MontoTotal { get; set; }
}

public class PreviewLineaDto
{
    public int NumeroLinea { get; set; }
    public long RutBeneficiario { get; set; }
    public string DvBeneficiario { get; set; } = string.Empty;
    public string NombreBeneficiario { get; set; } = string.Empty;
    public long? RutFuncionario { get; set; }
    public string? DvFuncionario { get; set; }
    public decimal? Monto { get; set; }
    public string? CodRetencion { get; set; }
    public string? TipoPago { get; set; }
    public string? NumeroCuenta { get; set; }
    public long? CodBanco { get; set; }
    public long? TipoCuenta { get; set; }
    public bool EsMulticuenta { get; set; }
    public string EstadoLinea { get; set; } = "OK";  // OK, ADVERTENCIA, ERROR, NUEVO
    public string? Mensaje { get; set; }
    public bool Incluir { get; set; } = true;
}

public class ConfirmarImportacionRequest
{
    public List<PreviewLineaDto> Lineas { get; set; } = new();
    public string? PeriodoProceso { get; set; }
}

public class ResultadoImportacionDto
{
    public long IdCarga { get; set; }
    public int Insertados { get; set; }
    public int Actualizados { get; set; }
    public int Excluidos { get; set; }
    public int Errores { get; set; }
    public decimal MontoTotal { get; set; }
    public string? Mensaje { get; set; }
}
