using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SRJE.Web.Infrastructure.Data;
using SRJE.Web.Models;
using SRJE.Web.Models.Entities;
using SRJE.Web.Models.Requests;
using SRJE.Web.Services;

namespace SRJE.Tests.Services;

public class BeneficiarioServiceTests : IDisposable
{
    private readonly SrjeDbContext _db;
    private readonly BeneficiarioService _service;

    public BeneficiarioServiceTests()
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
        long rut = 7051537, string dv = "7", string nombre = "MORENO PEREZ JUAN",
        string estado = "A")
    {
        var entity = new Beneficiario
        {
            RutBeneficiario = rut,
            DvBeneficiario = dv,
            NombreBeneficiario = nombre,
            Estado = estado,
            UsuarioCreacion = "test"
        };
        _db.Beneficiarios.Add(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    // --- ListarAsync ---

    [Fact]
    public async Task ListarAsync_DeberiaRetornarPaginaConResultados()
    {
        await SeedBeneficiario(rut: 7051537, dv: "7", nombre: "MORENO PEREZ");
        await SeedBeneficiario(rut: 12345678, dv: "5", nombre: "GONZALEZ LOPEZ");

        var result = await _service.ListarAsync(new BuscarBeneficiarioQuery { Page = 1, PageSize = 10 });

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(1, result.Page);
    }

    [Fact]
    public async Task ListarAsync_DeberiaFiltrarPorNombre()
    {
        await SeedBeneficiario(rut: 7051537, dv: "7", nombre: "MORENO PEREZ");
        await SeedBeneficiario(rut: 12345678, dv: "5", nombre: "GONZALEZ LOPEZ");

        var result = await _service.ListarAsync(new BuscarBeneficiarioQuery { Q = "MORENO", Page = 1, PageSize = 10 });

        Assert.Equal(1, result.TotalCount);
        Assert.Equal("MORENO PEREZ", result.Items[0].NombreBeneficiario);
    }

    [Fact]
    public async Task ListarAsync_DeberiaFiltrarPorRut()
    {
        await SeedBeneficiario(rut: 7051537, dv: "7", nombre: "MORENO PEREZ");
        await SeedBeneficiario(rut: 12345678, dv: "5", nombre: "GONZALEZ LOPEZ");

        var result = await _service.ListarAsync(new BuscarBeneficiarioQuery { Q = "7051537", Page = 1, PageSize = 10 });

        Assert.Equal(1, result.TotalCount);
        Assert.Equal(7051537, result.Items[0].RutBeneficiario);
    }

    [Fact]
    public async Task ListarAsync_DeberiaFiltrarPorEstado()
    {
        await SeedBeneficiario(rut: 7051537, dv: "7", estado: "A");
        await SeedBeneficiario(rut: 12345678, dv: "5", estado: "I");

        var result = await _service.ListarAsync(new BuscarBeneficiarioQuery { Estado = "A", Page = 1, PageSize = 10 });

        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task ListarAsync_DeberiaPaginar()
    {
        for (int i = 1; i <= 25; i++)
            await SeedBeneficiario(rut: 10000000 + i, dv: "0", nombre: $"BENEFICIARIO {i:D3}");

        var page1 = await _service.ListarAsync(new BuscarBeneficiarioQuery { Page = 1, PageSize = 10 });
        var page2 = await _service.ListarAsync(new BuscarBeneficiarioQuery { Page = 2, PageSize = 10 });
        var page3 = await _service.ListarAsync(new BuscarBeneficiarioQuery { Page = 3, PageSize = 10 });

        Assert.Equal(25, page1.TotalCount);
        Assert.Equal(10, page1.Items.Count);
        Assert.Equal(10, page2.Items.Count);
        Assert.Equal(5, page3.Items.Count);
    }

    // --- ObtenerPorRutAsync ---

    [Fact]
    public async Task ObtenerPorRutAsync_DeberiaRetornarDetalle()
    {
        await SeedBeneficiario(rut: 7051537, dv: "7", nombre: "MORENO PEREZ");

        var result = await _service.ObtenerPorRutAsync(7051537);

        Assert.NotNull(result);
        Assert.Equal(7051537, result!.RutBeneficiario);
        Assert.Equal("7.051.537-7", result.RutFormateado);
        Assert.Equal("MORENO PEREZ", result.NombreBeneficiario);
    }

    [Fact]
    public async Task ObtenerPorRutAsync_DeberiaRetornarNull_CuandoNoExiste()
    {
        var result = await _service.ObtenerPorRutAsync(99999999);
        Assert.Null(result);
    }

    [Fact]
    public async Task ObtenerPorRutAsync_DeberiaIncluirRetenciones()
    {
        var benef = await SeedBeneficiario(rut: 7051537, dv: "7");
        _db.RetenidosJudiciales.Add(new RetenidoJudicial
        {
            IdRetencion = 1,
            RutTitular = 12345678,
            DvTitular = "5",
            RutBeneficiario = 7051537,
            DvBeneficiario = "7",
            Monto = 100000,
            Estado = "A"
        });
        await _db.SaveChangesAsync();

        var result = await _service.ObtenerPorRutAsync(7051537);

        Assert.NotNull(result);
        Assert.Single(result!.Retenciones);
        Assert.Equal(100000, result.Retenciones[0].Monto);
    }

    // --- CrearAsync ---

    [Fact]
    public async Task CrearAsync_DeberiaCrearBeneficiarioYAuditoria()
    {
        var request = new CrearBeneficiarioRequest
        {
            RutBeneficiario = 7051537,
            DvBeneficiario = "7",
            NombreBeneficiario = "MORENO PEREZ JUAN"
        };

        var result = await _service.CrearAsync(request, "test_user");

        Assert.Equal(7051537, result.RutBeneficiario);
        Assert.Equal("7.051.537-7", result.RutFormateado);

        // Verificar en BD
        var enBd = await _db.Beneficiarios.FirstOrDefaultAsync(b => b.RutBeneficiario == 7051537);
        Assert.NotNull(enBd);

        // Verificar auditoria
        var auditoria = await _db.AuditoriaCambios.FirstOrDefaultAsync();
        Assert.NotNull(auditoria);
        Assert.Equal("INSERT", auditoria!.Accion);
        Assert.Equal("test_user", auditoria.Usuario);
    }

    [Fact]
    public async Task CrearAsync_DeberiaLanzarExcepcion_CuandoRutEsInvalido()
    {
        var request = new CrearBeneficiarioRequest
        {
            RutBeneficiario = 7051537,
            DvBeneficiario = "0", // DV incorrecto
            NombreBeneficiario = "TEST"
        };

        await Assert.ThrowsAsync<ArgumentException>(() => _service.CrearAsync(request, "test"));
    }

    [Fact]
    public async Task CrearAsync_DeberiaLanzarExcepcion_CuandoRutYaExiste()
    {
        await SeedBeneficiario(rut: 7051537, dv: "7");

        var request = new CrearBeneficiarioRequest
        {
            RutBeneficiario = 7051537,
            DvBeneficiario = "7",
            NombreBeneficiario = "DUPLICADO"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CrearAsync(request, "test"));
    }

    // --- ActualizarAsync ---

    [Fact]
    public async Task ActualizarAsync_DeberiaModificarDatos()
    {
        await SeedBeneficiario(rut: 7051537, dv: "7", nombre: "ORIGINAL");

        var request = new ActualizarBeneficiarioRequest
        {
            RutBeneficiario = 7051537,
            DvBeneficiario = "7",
            NombreBeneficiario = "ACTUALIZADO"
        };

        var result = await _service.ActualizarAsync(7051537, request, "test");

        Assert.Equal("ACTUALIZADO", result.NombreBeneficiario);
    }

    [Fact]
    public async Task ActualizarAsync_DeberiaLanzarExcepcion_CuandoNoExiste()
    {
        var request = new ActualizarBeneficiarioRequest
        {
            NombreBeneficiario = "TEST"
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.ActualizarAsync(99999, request, "test"));
    }

    // --- InactivarAsync ---

    [Fact]
    public async Task InactivarAsync_DeberiaCambiarEstadoYRegistrarAuditoria()
    {
        await SeedBeneficiario(rut: 7051537, dv: "7", estado: "A");

        var result = await _service.InactivarAsync(7051537, "test");

        Assert.True(result);
        var enBd = await _db.Beneficiarios.FirstAsync(b => b.RutBeneficiario == 7051537);
        Assert.Equal("I", enBd.Estado);

        var auditoria = await _db.AuditoriaCambios.FirstOrDefaultAsync(a => a.Accion == "INACTIVAR");
        Assert.NotNull(auditoria);
    }

    [Fact]
    public async Task InactivarAsync_DeberiaRetornarFalse_CuandoNoExiste()
    {
        var result = await _service.InactivarAsync(99999, "test");
        Assert.False(result);
    }

    // --- BuscarAsync ---

    [Fact]
    public async Task BuscarAsync_DeberiaBuscarPorNombre()
    {
        await SeedBeneficiario(rut: 7051537, dv: "7", nombre: "MORENO PEREZ", estado: "A");
        await SeedBeneficiario(rut: 12345678, dv: "5", nombre: "GONZALEZ LOPEZ", estado: "A");

        var result = await _service.BuscarAsync("MORENO");

        Assert.Single(result);
        Assert.Equal("MORENO PEREZ", result[0].NombreBeneficiario);
    }

    [Fact]
    public async Task BuscarAsync_NoDeberiaIncluirInactivos()
    {
        await SeedBeneficiario(rut: 7051537, dv: "7", nombre: "MORENO PEREZ", estado: "I");

        var result = await _service.BuscarAsync("MORENO");

        Assert.Empty(result);
    }
}
