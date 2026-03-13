using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SRJE.Web.Infrastructure.Data;

namespace SRJE.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CatalogosController : ControllerBase
{
    private readonly SrjeDbContext _db;

    public CatalogosController(SrjeDbContext db)
    {
        _db = db;
    }

    /// <summary>GET /api/catalogos/bancos</summary>
    [HttpGet("bancos")]
    public async Task<IActionResult> Bancos()
    {
        var bancos = await _db.Bancos
            .Where(b => b.Activo == "S")
            .OrderBy(b => b.NombreBanco)
            .Select(b => new { b.CodBanco, b.NombreBanco, b.UsaCtaOtBanco })
            .ToListAsync();
        return Ok(bancos);
    }

    /// <summary>GET /api/catalogos/tipos-retencion</summary>
    [HttpGet("tipos-retencion")]
    public async Task<IActionResult> TiposRetencion()
    {
        var tipos = await _db.TiposRetencion
            .Where(t => t.Activo == "S")
            .OrderBy(t => t.CodRetencion)
            .Select(t => new { t.CodRetencion, t.Descripcion, t.Moneda })
            .ToListAsync();
        return Ok(tipos);
    }

    /// <summary>GET /api/catalogos/tipos-cuenta</summary>
    [HttpGet("tipos-cuenta")]
    public async Task<IActionResult> TiposCuenta()
    {
        var tipos = await _db.TiposCuenta
            .Where(t => t.Activo == "S")
            .OrderBy(t => t.CodTipoCuenta)
            .Select(t => new { Codigo = t.CodTipoCuenta, t.Descripcion })
            .ToListAsync();
        return Ok(tipos);
    }
}
