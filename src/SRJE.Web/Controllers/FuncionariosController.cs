using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SRJE.Web.Models.Requests;
using SRJE.Web.Services;

namespace SRJE.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FuncionariosController : ControllerBase
{
    private readonly IFuncionarioService _service;

    public FuncionariosController(IFuncionarioService service)
    {
        _service = service;
    }

    /// <summary>GET /api/funcionarios — Listar con paginacion y filtros</summary>
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] BuscarFuncionarioQuery query)
    {
        var result = await _service.ListarAsync(query);
        return Ok(result);
    }

    /// <summary>GET /api/funcionarios/stats — Estadisticas generales</summary>
    [HttpGet("stats")]
    public async Task<IActionResult> Stats()
    {
        var result = await _service.ObtenerStatsAsync();
        return Ok(result);
    }

    /// <summary>GET /api/funcionarios/exportar/excel — Exportar a Excel</summary>
    [HttpGet("exportar/excel")]
    public async Task<IActionResult> ExportarExcel([FromQuery] string? q = null, [FromQuery] string? activo = null)
    {
        var bytes = await _service.ExportarExcelAsync(q, activo);
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Funcionarios_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
    }

    /// <summary>GET /api/funcionarios/exportar/csv — Exportar a CSV</summary>
    [HttpGet("exportar/csv")]
    public async Task<IActionResult> ExportarCsv([FromQuery] string? q = null, [FromQuery] string? activo = null)
    {
        var bytes = await _service.ExportarCsvAsync(q, activo);
        return File(bytes, "text/csv", $"Funcionarios_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
    }

    /// <summary>GET /api/funcionarios/{rut} — Ficha completa por RUT</summary>
    [HttpGet("{rut:long}")]
    public async Task<IActionResult> ObtenerPorRut(long rut)
    {
        var result = await _service.ObtenerPorRutAsync(rut);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>PUT /api/funcionarios/{rut} — Actualizar ficha</summary>
    [HttpPut("{rut:long}")]
    [Authorize(Roles = "admin,operador")]
    public async Task<IActionResult> Actualizar(long rut, [FromBody] ActualizarFuncionarioRequest request)
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

    /// <summary>DELETE /api/funcionarios/{rut} — Inactivar funcionario</summary>
    [HttpDelete("{rut:long}")]
    [Authorize(Roles = "admin,operador")]
    public async Task<IActionResult> Inactivar(long rut)
    {
        var usuario = User.Identity?.Name ?? "sistema";
        var result = await _service.InactivarAsync(rut, usuario);
        if (!result) return NotFound();
        return Ok(new { inactivado = true });
    }
}
