namespace SRJE.Web.Models.ViewModels;

public class UsuarioSistemaDto
{
    public string Usuario { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string Estado { get; set; } = "A";
    public DateTime FechaCreacion { get; set; }
}

public class AccesoDto
{
    public long Id { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string Evento { get; set; } = string.Empty;
    public string? Ip { get; set; }
    public DateTime Fecha { get; set; }
}

public class CargaResumenDto
{
    public long Id { get; set; }
    public string TipoCarga { get; set; } = string.Empty;
    public string? NombreArchivo { get; set; }
    public DateTime FechaInicio { get; set; }
    public string Estado { get; set; } = string.Empty;
    public int? TotalLineas { get; set; }
    public int? RegistrosInsertados { get; set; }
    public int? RegistrosActualizados { get; set; }
    public int? RegistrosError { get; set; }
    public string? Usuario { get; set; }
    public string? IpUsuario { get; set; }
}
