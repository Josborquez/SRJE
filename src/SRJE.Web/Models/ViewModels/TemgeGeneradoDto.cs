namespace SRJE.Web.Models.ViewModels;

public class TemgeGeneradoDto
{
    public byte[] Archivo { get; set; } = Array.Empty<byte>();
    public string NombreArchivo { get; set; } = string.Empty;
    public int CantidadRegistros { get; set; }
    public decimal MontoTotal { get; set; }
    public List<TemgeExcluidoDto> Excluidos { get; set; } = new();
    public decimal MontoExcluido => Excluidos.Sum(e => e.Monto);
}

public class TemgeExcluidoDto
{
    public long RutBeneficiario { get; set; }
    public string DvBeneficiario { get; set; } = string.Empty;
    public string? NombreBeneficiario { get; set; }
    public decimal Monto { get; set; }
    public string Motivo { get; set; } = string.Empty;
}
