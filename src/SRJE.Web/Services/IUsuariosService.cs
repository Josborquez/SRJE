using SRJE.Web.Models.Requests;
using SRJE.Web.Models.ViewModels;

namespace SRJE.Web.Services;

public interface IUsuariosService
{
    Task<List<UsuarioSistemaDto>> ListarAsync();
    Task<UsuarioSistemaDto> ObtenerAsync(string usuario);
    Task<UsuarioSistemaDto> CrearAsync(CrearUsuarioRequest request);
    Task<UsuarioSistemaDto> ActualizarAsync(string usuario, ActualizarUsuarioRequest request, string usuarioActual);
    Task CambiarPasswordAsync(string usuario, string password);
    Task<UsuarioSistemaDto> ToggleEstadoAsync(string usuario, string usuarioActual);
    Task<PagedResult<AccesoDto>> ListarAccesosAsync(BuscarAccesosQuery query);
    Task<PagedResult<CargaResumenDto>> ListarCargasAsync(BuscarCargasQuery query);
}
