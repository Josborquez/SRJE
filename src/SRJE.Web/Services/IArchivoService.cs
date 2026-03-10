using SRJE.Web.Models.ViewModels;
using SRJE.Web.Parsers;

namespace SRJE.Web.Services;

public interface IArchivoService
{
    Task<ArchivoPreviewDto> PreviewRemuneracionesAsync(Stream stream, string nombreArchivo);
    Task<TemgeArchivoDto> PreviewTemgeAsync(Stream stream, string nombreArchivo);
    Task<ArchivoPreviewDto> PreviewNuevasCuentasAsync(Stream stream, string nombreArchivo);
    Task<ResultadoImportacionDto> ConfirmarRemuneracionesAsync(ConfirmarImportacionRequest request, string usuario, string ip);
    Task<ResultadoImportacionDto> ConfirmarNuevasCuentasAsync(ConfirmarImportacionRequest request, string usuario, string ip);
    Task<byte[]> GenerarTemgeAsync(string usuario);
}
