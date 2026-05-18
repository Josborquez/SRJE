using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SRJE.Web.Infrastructure.Data;
using SRJE.Web.Models;
using SRJE.Web.Models.Entities;
using SRJE.Web.Models.Exceptions;
using SRJE.Web.Models.Requests;
using SRJE.Web.Services;

namespace SRJE.Tests.Services;

public class CuentaBeneficiarioServiceTests : IDisposable
{
    private readonly SrjeDbContext _db;
    private readonly BeneficiarioService _service;

    public CuentaBeneficiarioServiceTests()
    {
        var options = new DbContextOptionsBuilder<SrjeDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new SrjeDbContext(options);
        var settings = Options.Create(new SrjeSettings());
        _service = new BeneficiarioService(_db, settings);
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    private async Task<Beneficiario> SeedBeneficiario(
        long rut = 7051537, string dv = "7", string nombre = "MORENO PEREZ JUAN")
    {
        var entity = new Beneficiario
        {
            RutBeneficiario = rut,
            DvBeneficiario = dv,
            NombreBeneficiario = nombre,
            Estado = "A",
            UsuarioCreacion = "test"
        };
        _db.Beneficiarios.Add(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    private async Task SeedBanco(long cod = 1, string nombre = "BANCO DE CHILE")
    {
        _db.Bancos.Add(new Banco { CodBanco = cod, NombreBanco = nombre });
        await _db.SaveChangesAsync();
    }

    private async Task SeedTipoCuenta(long cod = 1, string desc = "Cuenta Corriente")
    {
        _db.TiposCuenta.Add(new TipoCuenta { CodTipoCuenta = cod, Descripcion = desc });
        await _db.SaveChangesAsync();
    }

    // --- AgregarCuentaAsync ---

    [Fact]
    public async Task AgregarCuentaAsync_DeberiaCrearCuenta()
    {
        await SeedBeneficiario();
        await SeedBanco();
        await SeedTipoCuenta();

        var result = await _service.AgregarCuentaAsync(7051537, new CrearCuentaBeneficiarioRequest
        {
            CodBanco = 1,
            TipoCuenta = 1,
            NumeroCuenta = "123456789",
            Alias = "Mi cuenta"
        }, "test");

        Assert.Equal(1, result.CodBanco);
        Assert.Equal("123456789", result.NumeroCuenta);
        Assert.Equal("Mi cuenta", result.Alias);
        Assert.Equal("BANCO DE CHILE", result.NombreBanco);

        var enDb = await _db.CuentasBeneficiario.CountAsync();
        Assert.Equal(1, enDb);
    }

    [Fact]
    public async Task AgregarCuentaAsync_DeberiaFallarSiBeneficiarioNoExiste()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.AgregarCuentaAsync(99999, new CrearCuentaBeneficiarioRequest
            {
                CodBanco = 1,
                TipoCuenta = 1,
                NumeroCuenta = "123"
            }, "test"));
    }

    [Fact]
    public async Task AgregarCuentaAsync_DeberiaFallarSiDuplicado()
    {
        await SeedBeneficiario();
        _db.CuentasBeneficiario.Add(new CuentaBeneficiario
        {
            RutBeneficiario = 7051537,
            CodBanco = 1,
            TipoCuenta = 1,
            NumeroCuenta = "123456789",
            UsuarioCreacion = "test"
        });
        await _db.SaveChangesAsync();

        await Assert.ThrowsAsync<BusinessConflictException>(() =>
            _service.AgregarCuentaAsync(7051537, new CrearCuentaBeneficiarioRequest
            {
                CodBanco = 1,
                TipoCuenta = 1,
                NumeroCuenta = "123456789"
            }, "test"));
    }

    // --- ListarCuentasAsync ---

    [Fact]
    public async Task ListarCuentasAsync_DeberiaRetornarCuentasOrdenadas()
    {
        await SeedBeneficiario();
        await SeedBanco(1, "BANCO DE CHILE");
        await SeedBanco(12, "BANCOESTADO");
        await SeedTipoCuenta();

        _db.CuentasBeneficiario.Add(new CuentaBeneficiario
        {
            RutBeneficiario = 7051537, CodBanco = 12, TipoCuenta = 1,
            NumeroCuenta = "999", Orden = 1, UsuarioCreacion = "test"
        });
        _db.CuentasBeneficiario.Add(new CuentaBeneficiario
        {
            RutBeneficiario = 7051537, CodBanco = 1, TipoCuenta = 1,
            NumeroCuenta = "111", Orden = 0, UsuarioCreacion = "test"
        });
        await _db.SaveChangesAsync();

        var result = await _service.ListarCuentasAsync(7051537);

        Assert.Equal(2, result.Count);
        Assert.Equal(0, result[0].Orden);
        Assert.Equal("111", result[0].NumeroCuenta);
        Assert.Equal(1, result[1].Orden);
    }

    // --- EliminarCuentaAsync ---

    [Fact]
    public async Task EliminarCuentaAsync_DeberiaEliminarCuenta()
    {
        await SeedBeneficiario();
        var cuenta = new CuentaBeneficiario
        {
            RutBeneficiario = 7051537, CodBanco = 1, TipoCuenta = 1,
            NumeroCuenta = "123", UsuarioCreacion = "test"
        };
        _db.CuentasBeneficiario.Add(cuenta);
        await _db.SaveChangesAsync();

        var result = await _service.EliminarCuentaAsync(7051537, cuenta.Id, "test");

        Assert.True(result);
        Assert.Equal(0, await _db.CuentasBeneficiario.CountAsync());
    }

    [Fact]
    public async Task EliminarCuentaAsync_DeberiaRetornarFalseSiNoExiste()
    {
        await SeedBeneficiario();

        var result = await _service.EliminarCuentaAsync(7051537, 999, "test");

        Assert.False(result);
    }

    // --- AgregarCuentaAsync orden automatico ---

    [Fact]
    public async Task AgregarCuentaAsync_DeberiaAutoIncrementarOrden()
    {
        await SeedBeneficiario();
        _db.CuentasBeneficiario.Add(new CuentaBeneficiario
        {
            RutBeneficiario = 7051537, CodBanco = 1, TipoCuenta = 1,
            NumeroCuenta = "111", Orden = 0, UsuarioCreacion = "test"
        });
        await _db.SaveChangesAsync();

        var result = await _service.AgregarCuentaAsync(7051537, new CrearCuentaBeneficiarioRequest
        {
            CodBanco = 1,
            TipoCuenta = 2,
            NumeroCuenta = "222"
        }, "test");

        Assert.Equal(1, result.Orden);
    }

    // --- AgregarCuentaAsync genera auditoria ---

    [Fact]
    public async Task AgregarCuentaAsync_DeberiaRegistrarAuditoria()
    {
        await SeedBeneficiario();

        await _service.AgregarCuentaAsync(7051537, new CrearCuentaBeneficiarioRequest
        {
            CodBanco = 1,
            TipoCuenta = 1,
            NumeroCuenta = "123"
        }, "operador1");

        var audit = await _db.AuditoriaCambios.FirstOrDefaultAsync(a =>
            a.Entidad == "CUENTA_BENEFICIARIO" && a.Accion == "INSERT");

        Assert.NotNull(audit);
        Assert.Equal("operador1", audit.Usuario);
        Assert.Contains("123", audit.ValorNuevo!);
    }
}
