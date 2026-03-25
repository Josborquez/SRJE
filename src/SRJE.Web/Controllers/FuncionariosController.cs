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
    public async Task<IActionResult> Inactivar(long rut)
    {
        var usuario = User.Identity?.Name ?? "sistema";
        var result = await _service.InactivarAsync(rut, usuario);
        if (!result) return NotFound();
        return Ok(new { inactivado = true });
    }
}
