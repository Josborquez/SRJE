using SRJE.Web.Models.Requests;
using SRJE.Web.Models.ViewModels;

namespace SRJE.Web.Services;

public interface IBeneficiarioService
{
    Task<PagedResult<BeneficiarioDto>> ListarAsync(BuscarBeneficiarioQuery query);
    Task<BeneficiarioDetalleDto?> ObtenerPorRutAsync(long rut);
    Task<BeneficiarioDto> CrearAsync(CrearBeneficiarioRequest request, string usuario);
    Task<BeneficiarioDto> ActualizarAsync(long rut, ActualizarBeneficiarioRequest request, string usuario);
    Task<bool> InactivarAsync(long rut, string usuario);
    Task<List<RetencionDto>> ObtenerRetencionesAsync(long rut);
    Task<RetencionDto> ActualizarRetencionAsync(long rut, long id, ActualizarRetencionRequest request, string usuario);
    Task<List<BeneficiarioDto>> BuscarAsync(string query);
    Task<byte[]> ExportarExcelAsync(string? estado = null);
    Task<byte[]> ExportarCsvAsync(string? estado = null);
    Task<List<CuentaBeneficiarioDto>> ListarCuentasAsync(long rut);
    Task<CuentaBeneficiarioDto> AgregarCuentaAsync(long rut, CrearCuentaBeneficiarioRequest request, string usuario);
    Task<CuentaBeneficiarioDto> ActualizarCuentaAsync(long rut, long id, ActualizarCuentaBeneficiarioRequest request, string usuario);
    Task<bool> EliminarCuentaAsync(long rut, long id, string usuario);
}
