using System.ComponentModel.DataAnnotations;

namespace SRJE.Web.Models.Requests;

public class CrearUsuarioRequest
{
    [Required, MaxLength(50)]
    public string Usuario { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required]
    public string Rol { get; set; } = string.Empty;
}

public class ActualizarUsuarioRequest
{
    [Required, MaxLength(100)]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required]
    public string Rol { get; set; } = string.Empty;
}

public class CambiarPasswordRequest
{
    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;
}

public class BuscarAccesosQuery
{
    public string? Usuario { get; set; }
    public string? Evento { get; set; }
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class BuscarCargasQuery
{
    public string? Usuario { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
