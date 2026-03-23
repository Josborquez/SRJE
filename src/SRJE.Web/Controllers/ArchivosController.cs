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
    private readonly IAuditoriaBeneficiariosService _auditoria;

    public ArchivosController(
        IRemuneracionesService remuneraciones,
        ITemgeService temge,
        INuevasCuentasService nuevasCuentas,
        IAuditoriaBeneficiariosService auditoria)
    {
        _remuneraciones = remuneraciones;
        _temge = temge;
        _nuevasCuentas = nuevasCuentas;
        _auditoria = auditoria;
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

    /// <summary>GET /api/archivos/temge/generar — Genera y descarga archivo TEMGE para un periodo</summary>
    [HttpGet("temge/generar")]
    public async Task<IActionResult> GenerarTemge([FromQuery] string? periodo = null)
    {
        var usuario = User.Identity?.Name ?? "sistema";
        var archivo = await _temge.GenerarTemgeAsync(usuario, periodo);
        var sufijo = !string.IsNullOrEmpty(periodo) ? periodo : DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var nombre = $"TEMGE_{sufijo}.txt";
        return File(archivo, "text/plain", nombre);
    }

    /// <summary>POST /api/archivos/auditoria/comparar — Comparar archivo vs BD</summary>
    [HttpPost("auditoria/comparar")]
    public async Task<IActionResult> CompararBeneficiarios(IFormFile archivo)
    {
        if (archivo == null || archivo.Length == 0)
            return BadRequest(new { error = "Archivo requerido" });

        using var stream = archivo.OpenReadStream();
        var result = await _auditoria.CompararAsync(stream);
        return Ok(result);
    }

    /// <summary>POST /api/archivos/auditoria/exportar-excel — Exportar comparacion a Excel</summary>
    [HttpPost("auditoria/exportar-excel")]
    public async Task<IActionResult> ExportarAuditoriaExcel(IFormFile archivo)
    {
        if (archivo == null || archivo.Length == 0)
            return BadRequest(new { error = "Archivo requerido" });

        using var stream = archivo.OpenReadStream();
        var bytes = await _auditoria.ExportarExcelAsync(stream);
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Auditoria_Beneficiarios_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
    }

    /// <summary>POST /api/archivos/auditoria/exportar-csv — Exportar comparacion a CSV</summary>
    [HttpPost("auditoria/exportar-csv")]
    public async Task<IActionResult> ExportarAuditoriaCsv(IFormFile archivo)
    {
        if (archivo == null || archivo.Length == 0)
            return BadRequest(new { error = "Archivo requerido" });

        using var stream = archivo.OpenReadStream();
        var bytes = await _auditoria.ExportarCsvAsync(stream);
        return File(bytes, "text/csv", $"Auditoria_Beneficiarios_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
    }
}
