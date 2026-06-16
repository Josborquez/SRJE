using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SRJE.Web.Models;
using SRJE.Web.Services;

namespace SRJE.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IRegistroAccesoService _registroAcceso;

    public AuthController(IAuthService authService, IRegistroAccesoService registroAcceso)
    {
        _authService = authService;
        _registroAcceso = registroAcceso;
    }

    private string? IpCliente => HttpContext.Connection.RemoteIpAddress?.ToString();

    /// <summary>POST /api/auth/login</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var usuario = await _authService.ValidarCredencialesAsync(request.Usuario, request.Password);
        if (usuario == null)
        {
            await _registroAcceso.RegistrarAsync(request.Usuario, "login_fail", IpCliente);
            return Unauthorized(new { error = "Usuario o contraseña incorrectos" });
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, usuario.Usuario),
            new(ClaimTypes.GivenName, usuario.NombreCompleto),
            new(ClaimTypes.Role, usuario.Rol)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            });

        await _registroAcceso.RegistrarAsync(usuario.Usuario, "login_ok", IpCliente);

        return Ok(usuario);
    }

    /// <summary>POST /api/auth/logout</summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var usuario = User.Identity?.Name;
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (!string.IsNullOrEmpty(usuario))
            await _registroAcceso.RegistrarAsync(usuario, "logout", IpCliente);
        return Ok(new { message = "Sesion cerrada" });
    }

    /// <summary>GET /api/auth/me — Obtener usuario actual</summary>
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        return Ok(new UsuarioInfo
        {
            Usuario = User.Identity?.Name ?? "",
            NombreCompleto = User.FindFirstValue(ClaimTypes.GivenName) ?? "",
            Rol = User.FindFirstValue(ClaimTypes.Role) ?? ""
        });
    }
}
