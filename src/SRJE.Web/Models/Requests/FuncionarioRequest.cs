using System.ComponentModel.DataAnnotations;

namespace SRJE.Web.Models.Requests;

public class ActualizarFuncionarioRequest
{
    [MaxLength(20)]
    public string? ApellidoPaterno { get; set; }

    [MaxLength(20)]
    public string? ApellidoMaterno { get; set; }

    [MaxLength(30)]
    public string? Nombres { get; set; }

    [MaxLength(8)]
    public string? IdSistema { get; set; }
}

public class BuscarFuncionarioQuery
{
    public string? Q { get; set; }

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 20;

    public string? Activo { get; set; }
}
