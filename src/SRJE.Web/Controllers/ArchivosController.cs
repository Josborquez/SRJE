using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SRJE.Web.Models.ViewModels;
using SRJE.Web.Services;

namespace SRJE.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ArchivosController : ControllerBase
{
    private readonly IRemuneracionesService _remuneraciones;
    private readonly ITemgeService _temge;
    private readonly INuevasCuentasService _nuevasCuentas;

    public ArchivosController(
        IRemuneracionesService remuneraciones,
        ITemgeService temge,
        INuevasCuentasService nuevasCuentas)
    {
        _remuneraciones = remuneraciones;
        _temge = temge;
        _nuevasCuentas = nuevasCuentas;
    }

    /// <summary>POST /api/archivos/remuneraciones/preview — Parsear sin persistir</summary>
    [HttpPost("remuneraciones/preview")]
    public async Task<IActionResult> PreviewRemuneraciones(IFormFile archivo)
    {
        if (archivo == null || archivo.Length == 0)
            return BadRequest(new { error = "Archivo requerido" });

        using var stream = archivo.OpenReadStream();
        var result = await _remuneraciones.PreviewRemuneracionesAsync(stream, archivo.FileName);
        return Ok(result);
    }

    /// <summary>POST /api/archivos/remuneraciones/confirmar — Confirmar importacion</summary>
    [HttpPost("remuneraciones/confirmar")]
    public async Task<IActionResult> ConfirmarRemuneraciones([FromBody] ConfirmarImportacionRequest request)
    {
        var usuario = User.Identity?.Name ?? "sistema";
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        var result = await _remuneraciones.ConfirmarRemuneracionesAsync(request, usuario, ip);
        return Ok(result);
    }

    /// <summary>POST /api/archivos/temge/preview — Parsear archivo TEMGE</summary>
    [HttpPost("temge/preview")]
    public async Task<IActionResult> PreviewTemge(IFormFile archivo)
    {
        if (archivo == null || archivo.Length == 0)
            return BadRequest(new { error = "Archivo requerido" });

        using var stream = archivo.OpenReadStream();
        var result = await _temge.PreviewTemgeAsync(stream, archivo.FileName);
        return Ok(result);
    }

    /// <summary>POST /api/archivos/nuevas-cuentas/preview — Parsear Excel nuevas cuentas</summary>
    [HttpPost("nuevas-cuentas/preview")]
    public async Task<IActionResult> PreviewNuevasCuentas(IFormFile archivo)
    {
        if (archivo == null || archivo.Length == 0)
            return BadRequest(new { error = "Archivo requerido" });

        using var stream = archivo.OpenReadStream();
        var result = await _nuevasCuentas.PreviewNuevasCuentasAsync(stream, archivo.FileName);
        return Ok(result);
    }

    /// <summary>POST /api/archivos/nuevas-cuentas/confirmar — Confirmar nuevas cuentas</summary>
    [HttpPost("nuevas-cuentas/confirmar")]
    public async Task<IActionResult> ConfirmarNuevasCuentas([FromBody] ConfirmarImportacionRequest request)
    {
        var usuario = User.Identity?.Name ?? "sistema";
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        var result = await _nuevasCuentas.ConfirmarNuevasCuentasAsync(request, usuario, ip);
        return Ok(result);
    }

    /// <summary>POST /api/archivos/temge/confirmar — Confirmar importacion TEMGE</summary>
    [HttpPost("temge/confirmar")]
    public async Task<IActionResult> ConfirmarTemge([FromBody] ConfirmarImportacionRequest request)
    {
        var usuario = User.Identity?.Name ?? "sistema";
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        var result = await _temge.ConfirmarTemgeAsync(request, usuario, ip);
        return Ok(result);
    }

    /// <summary>GET /api/archivos/temge/generar — Genera y descarga archivo TEMGE</summary>
    [HttpGet("temge/generar")]
    public async Task<IActionResult> GenerarTemge()
    {
        var usuario = User.Identity?.Name ?? "sistema";
        var archivo = await _temge.GenerarTemgeAsync(usuario);
        var nombre = $"TEMGE_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
        return File(archivo, "text/plain", nombre);
    }
}
