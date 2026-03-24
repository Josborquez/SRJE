using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SRJE.Web.Models.Requests;
using SRJE.Web.Services;

namespace SRJE.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BeneficiariosController : ControllerBase
{
    private readonly IBeneficiarioService _service;

    public BeneficiariosController(IBeneficiarioService service)
    {
        _service = service;
    }

    /// <summary>GET /api/beneficiarios — Listar con paginacion y filtros</summary>
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] BuscarBeneficiarioQuery query)
    {
        var result = await _service.ListarAsync(query);
        return Ok(result);
    }

    /// <summary>GET /api/beneficiarios/{rut} — Ficha completa por RUT</summary>
    [HttpGet("{rut:long}")]
    public async Task<IActionResult> ObtenerPorRut(long rut)
    {
        var result = await _service.ObtenerPorRutAsync(rut);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>POST /api/beneficiarios — Crear nuevo beneficiario</summary>
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearBeneficiarioRequest request)
    {
        try
        {
            var usuario = User.Identity?.Name ?? "sistema";
            var result = await _service.CrearAsync(request, usuario);
            return CreatedAtAction(nameof(ObtenerPorRut),
                new { rut = result.RutBeneficiario }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    /// <summary>PUT /api/beneficiarios/{rut} — Actualizar ficha</summary>
    [HttpPut("{rut:long}")]
    public async Task<IActionResult> Actualizar(long rut, [FromBody] ActualizarBeneficiarioRequest request)
    {
        try
        {
            var usuario = User.Identity?.Name ?? "sistema";
            var result = await _service.ActualizarAsync(rut, request, usuario);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>DELETE /api/beneficiarios/{rut} — Inactivar beneficiario</summary>
    [HttpDelete("{rut:long}")]
    public async Task<IActionResult> Inactivar(long rut)
    {
        var usuario = User.Identity?.Name ?? "sistema";
        var result = await _service.InactivarAsync(rut, usuario);
        if (!result) return NotFound();
        return Ok(new { inactivado = true });
    }

    /// <summary>GET /api/beneficiarios/{rut}/retenciones — Retenciones del beneficiario</summary>
    [HttpGet("{rut:long}/retenciones")]
    public async Task<IActionResult> ObtenerRetenciones(long rut)
    {
        var result = await _service.ObtenerRetencionesAsync(rut);
        return Ok(result);
    }

    /// <summary>GET /api/beneficiarios/buscar?q= — Busqueda por nombre o RUT</summary>
    [HttpGet("buscar")]
    public async Task<IActionResult> Buscar([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q)) return Ok(Array.Empty<object>());
        var result = await _service.BuscarAsync(q);
        return Ok(result);
    }

    /// <summary>GET /api/beneficiarios/exportar/excel — Exportar a Excel</summary>
    [HttpGet("exportar/excel")]
    public async Task<IActionResult> ExportarExcel([FromQuery] string? estado = null)
    {
        var bytes = await _service.ExportarExcelAsync(estado);
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Beneficiarios_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
    }

    /// <summary>GET /api/beneficiarios/exportar/csv — Exportar a CSV</summary>
    [HttpGet("exportar/csv")]
    public async Task<IActionResult> ExportarCsv([FromQuery] string? estado = null)
    {
        var bytes = await _service.ExportarCsvAsync(estado);
        return File(bytes, "text/csv", $"Beneficiarios_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
    }
}
