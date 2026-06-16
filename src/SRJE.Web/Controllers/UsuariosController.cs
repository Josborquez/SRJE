using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SRJE.Web.Models.Requests;
using SRJE.Web.Services;

namespace SRJE.Web.Controllers;

[ApiController]
[Route("api/usuarios")]
[Authorize(Roles = "admin")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuariosService _usuarios;

    public UsuariosController(IUsuariosService usuarios)
    {
        _usuarios = usuarios;
    }

    /// <summary>GET /api/usuarios — Listar usuarios del sistema</summary>
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        return Ok(await _usuarios.ListarAsync());
    }

    /// <summary>GET /api/usuarios/accesos — Historial de login/logout</summary>
    [HttpGet("accesos")]
    public async Task<IActionResult> Accesos([FromQuery] BuscarAccesosQuery query)
    {
        return Ok(await _usuarios.ListarAccesosAsync(query));
    }

    /// <summary>GET /api/usuarios/cargas — Archivos trabajados por usuario</summary>
    [HttpGet("cargas")]
    public async Task<IActionResult> Cargas([FromQuery] BuscarCargasQuery query)
    {
        return Ok(await _usuarios.ListarCargasAsync(query));
    }

    /// <summary>GET /api/usuarios/{usuario}</summary>
    [HttpGet("{usuario}")]
    public async Task<IActionResult> Obtener(string usuario)
    {
        return Ok(await _usuarios.ObtenerAsync(usuario));
    }

    /// <summary>POST /api/usuarios — Crear usuario</summary>
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearUsuarioRequest request)
    {
        var dto = await _usuarios.CrearAsync(request);
        return CreatedAtAction(nameof(Obtener), new { usuario = dto.Usuario }, dto);
    }

    /// <summary>PUT /api/usuarios/{usuario} — Actualizar nombre/rol</summary>
    [HttpPut("{usuario}")]
    public async Task<IActionResult> Actualizar(string usuario, [FromBody] ActualizarUsuarioRequest request)
    {
        return Ok(await _usuarios.ActualizarAsync(usuario, request, User.Identity!.Name!));
    }

    /// <summary>PUT /api/usuarios/{usuario}/password — Restablecer contraseña</summary>
    [HttpPut("{usuario}/password")]
    public async Task<IActionResult> CambiarPassword(string usuario, [FromBody] CambiarPasswordRequest request)
    {
        await _usuarios.CambiarPasswordAsync(usuario, request.Password);
        return Ok(new { message = "Contraseña actualizada" });
    }

    /// <summary>PATCH /api/usuarios/{usuario}/toggle — Activar/Inactivar</summary>
    [HttpPatch("{usuario}/toggle")]
    public async Task<IActionResult> ToggleEstado(string usuario)
    {
        return Ok(await _usuarios.ToggleEstadoAsync(usuario, User.Identity!.Name!));
    }
}
