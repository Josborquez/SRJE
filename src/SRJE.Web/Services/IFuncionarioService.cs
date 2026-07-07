using SRJE.Web.Models.Requests;
using SRJE.Web.Models.ViewModels;

namespace SRJE.Web.Services;

public interface IFuncionarioService
{
    Task<PagedResult<FuncionarioDto>> ListarAsync(BuscarFuncionarioQuery query);
    Task<FuncionarioDetalleDto?> ObtenerPorRutAsync(long rut);
    Task<FuncionarioDto> ActualizarAsync(long rut, ActualizarFuncionarioRequest request, string usuario);
    Task<bool> InactivarAsync(long rut, string usuario);
    Task<FuncionarioStatsDto> ObtenerStatsAsync();
    Task<byte[]> ExportarExcelAsync(string? q = null, string? activo = null);
    Task<byte[]> ExportarCsvAsync(string? q = null, string? activo = null);
}
