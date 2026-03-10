using Microsoft.AspNetCore.Mvc;
using SRJE.Web.Models.ViewModels;
using SRJE.Web.Services;

namespace SRJE.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArchivosController : ControllerBase
{
    private readonly IArchivoService _service;

    public ArchivosController(IArchivoService service)
    {
        _service = service;
    }

    /// <summary>POST /api/archivos/remuneraciones/preview — Parsear sin persistir</summary>
    [HttpPost("remuneraciones/preview")]
    public async Task<IActionResult> PreviewRemuneraciones(IFormFile archivo)
    {
        if (archivo == null || archivo.Length == 0)
            return BadRequest(new { error = "Archivo requerido" });

        using var stream = archivo.OpenReadStream();
        var result = await _service.PreviewRemuneracionesAsync(stream, archivo.FileName);
        return Ok(result);
    }

    /// <summary>POST /api/archivos/remuneraciones/confirmar — Confirmar importacion</summary>
    [HttpPost("remuneraciones/confirmar")]
    public async Task<IActionResult> ConfirmarRemuneraciones([FromBody] ConfirmarImportacionRequest request)
    {
        var usuario = User.Identity?.Name ?? "sistema";
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        var result = await _service.ConfirmarRemuneracionesAsync(request, usuario, ip);
        return Ok(result);
    }

    /// <summary>POST /api/archivos/temge/preview — Parsear archivo TEMGE</summary>
    [HttpPost("temge/preview")]
    public async Task<IActionResult> PreviewTemge(IFormFile archivo)
    {
        if (archivo == null || archivo.Length == 0)
            return BadRequest(new { error = "Archivo requerido" });

        using var stream = archivo.OpenReadStream();
        var result = await _service.PreviewTemgeAsync(stream, archivo.FileName);
        return Ok(result);
    }

    /// <summary>POST /api/archivos/nuevas-cuentas/preview — Parsear Excel nuevas cuentas</summary>
    [HttpPost("nuevas-cuentas/preview")]
    public async Task<IActionResult> PreviewNuevasCuentas(IFormFile archivo)
    {
        if (archivo == null || archivo.Length == 0)
            return BadRequest(new { error = "Archivo requerido" });

        using var stream = archivo.OpenReadStream();
        var result = await _service.PreviewNuevasCuentasAsync(stream, archivo.FileName);
        return Ok(result);
    }

    /// <summary>POST /api/archivos/nuevas-cuentas/confirmar — Confirmar nuevas cuentas</summary>
    [HttpPost("nuevas-cuentas/confirmar")]
    public async Task<IActionResult> ConfirmarNuevasCuentas([FromBody] ConfirmarImportacionRequest request)
    {
        var usuario = User.Identity?.Name ?? "sistema";
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        var result = await _service.ConfirmarNuevasCuentasAsync(request, usuario, ip);
        return Ok(result);
    }

    /// <summary>GET /api/archivos/temge/generar — Genera y descarga archivo TEMGE</summary>
    [HttpGet("temge/generar")]
    public async Task<IActionResult> GenerarTemge()
    {
        var usuario = User.Identity?.Name ?? "sistema";
        var archivo = await _service.GenerarTemgeAsync(usuario);
        var nombre = $"TEMGE_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
        return File(archivo, "text/plain", nombre);
    }
}
