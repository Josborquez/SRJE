using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SRJE.Web.Infrastructure.Data;
using SRJE.Web.Models.Entities;
using SRJE.Web.Models.Requests;

namespace SRJE.Web.Controllers;

[ApiController]
[Route("api/mantenedores")]
[Authorize(Roles = "admin")]
public class MantenedoresController : ControllerBase
{
    private readonly SrjeDbContext _db;

    public MantenedoresController(SrjeDbContext db)
    {
        _db = db;
    }

    // ===================== BANCOS =====================

    /// <summary>GET /api/mantenedores/bancos — Listar todos los bancos</summary>
    [HttpGet("bancos")]
    public async Task<IActionResult> ListarBancos()
    {
        var bancos = await _db.Bancos
            .OrderBy(b => b.CodBanco)
            .ToListAsync();
        return Ok(bancos);
    }

    /// <summary>GET /api/mantenedores/bancos/{cod} — Obtener banco por codigo</summary>
    [HttpGet("bancos/{cod:long}")]
    public async Task<IActionResult> ObtenerBanco(long cod)
    {
        var banco = await _db.Bancos.FindAsync(cod);
        if (banco == null) return NotFound();
        return Ok(banco);
    }

    /// <summary>POST /api/mantenedores/bancos — Crear banco</summary>
    [HttpPost("bancos")]
    public async Task<IActionResult> CrearBanco([FromBody] CrearBancoRequest request)
    {
        var existe = await _db.Bancos.AnyAsync(b => b.CodBanco == request.CodBanco);
        if (existe)
            return Conflict(new { error = $"Ya existe un banco con codigo {request.CodBanco}" });

        var entity = new Banco
        {
            CodBanco = request.CodBanco,
            NombreBanco = request.NombreBanco,
            UsaCtaOtBanco = request.UsaCtaOtBanco
        };

        _db.Bancos.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(ObtenerBanco), new { cod = entity.CodBanco }, entity);
    }

    /// <summary>PUT /api/mantenedores/bancos/{cod} — Actualizar banco</summary>
    [HttpPut("bancos/{cod:long}")]
    public async Task<IActionResult> ActualizarBanco(long cod, [FromBody] ActualizarBancoRequest request)
    {
        var banco = await _db.Bancos.FindAsync(cod);
        if (banco == null) return NotFound();

        banco.NombreBanco = request.NombreBanco;
        banco.UsaCtaOtBanco = request.UsaCtaOtBanco;
        banco.Activo = request.Activo;

        await _db.SaveChangesAsync();
        return Ok(banco);
    }

    /// <summary>PATCH /api/mantenedores/bancos/{cod}/toggle — Activar/Inactivar banco</summary>
    [HttpPatch("bancos/{cod:long}/toggle")]
    public async Task<IActionResult> ToggleBanco(long cod)
    {
        var banco = await _db.Bancos.FindAsync(cod);
        if (banco == null) return NotFound();

        banco.Activo = banco.Activo == "S" ? "N" : "S";
        await _db.SaveChangesAsync();
        return Ok(banco);
    }

    // ===================== TIPOS DE CUENTA =====================

    /// <summary>GET /api/mantenedores/tipos-cuenta — Listar todos los tipos de cuenta</summary>
    [HttpGet("tipos-cuenta")]
    public async Task<IActionResult> ListarTiposCuenta()
    {
        var tipos = await _db.TiposCuenta
            .OrderBy(t => t.CodTipoCuenta)
            .ToListAsync();
        return Ok(tipos);
    }

    /// <summary>GET /api/mantenedores/tipos-cuenta/{cod} — Obtener tipo cuenta</summary>
    [HttpGet("tipos-cuenta/{cod:long}")]
    public async Task<IActionResult> ObtenerTipoCuenta(long cod)
    {
        var tipo = await _db.TiposCuenta.FindAsync(cod);
        if (tipo == null) return NotFound();
        return Ok(tipo);
    }

    /// <summary>POST /api/mantenedores/tipos-cuenta — Crear tipo cuenta</summary>
    [HttpPost("tipos-cuenta")]
    public async Task<IActionResult> CrearTipoCuenta([FromBody] CrearTipoCuentaRequest request)
    {
        var existe = await _db.TiposCuenta.AnyAsync(t => t.CodTipoCuenta == request.CodTipoCuenta);
        if (existe)
            return Conflict(new { error = $"Ya existe un tipo de cuenta con codigo {request.CodTipoCuenta}" });

        var entity = new TipoCuenta
        {
            CodTipoCuenta = request.CodTipoCuenta,
            Descripcion = request.Descripcion
        };

        _db.TiposCuenta.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(ObtenerTipoCuenta), new { cod = entity.CodTipoCuenta }, entity);
    }

    /// <summary>PUT /api/mantenedores/tipos-cuenta/{cod} — Actualizar tipo cuenta</summary>
    [HttpPut("tipos-cuenta/{cod:long}")]
    public async Task<IActionResult> ActualizarTipoCuenta(long cod, [FromBody] ActualizarTipoCuentaRequest request)
    {
        var tipo = await _db.TiposCuenta.FindAsync(cod);
        if (tipo == null) return NotFound();

        tipo.Descripcion = request.Descripcion;
        tipo.Activo = request.Activo;

        await _db.SaveChangesAsync();
        return Ok(tipo);
    }

    /// <summary>PATCH /api/mantenedores/tipos-cuenta/{cod}/toggle — Activar/Inactivar tipo cuenta</summary>
    [HttpPatch("tipos-cuenta/{cod:long}/toggle")]
    public async Task<IActionResult> ToggleTipoCuenta(long cod)
    {
        var tipo = await _db.TiposCuenta.FindAsync(cod);
        if (tipo == null) return NotFound();

        tipo.Activo = tipo.Activo == "S" ? "N" : "S";
        await _db.SaveChangesAsync();
        return Ok(tipo);
    }
}
