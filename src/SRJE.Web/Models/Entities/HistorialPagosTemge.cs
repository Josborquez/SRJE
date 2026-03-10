namespace SRJE.Web.Models.Entities;

public class HistorialPagosTemge
{
    public long Id { get; set; }
    public DateTime FechaProceso { get; set; }
    public string? HoraProceso { get; set; }
    public string? CodEmpresa { get; set; }
    public decimal? MontoTotal { get; set; }
    public int? CantidadRegistros { get; set; }
    public string? NombreArchivo { get; set; }
    public string Estado { get; set; } = "G";
    public string? UsuarioGenera { get; set; }

    public ICollection<DetallePagoTemge> Detalles { get; set; } = new List<DetallePagoTemge>();
}
