using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SRJE.Web.Infrastructure.Data;
using SRJE.Web.Services;

namespace SRJE.Tests.Services;

public class RegistroAccesoServiceTests
{
    private static SrjeDbContext CrearDb()
    {
        var options = new DbContextOptionsBuilder<SrjeDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new SrjeDbContext(options);
    }

    [Fact]
    public async Task Registrar_InsertaEvento()
    {
        using var db = CrearDb();
        var service = new RegistroAccesoService(db);

        await service.RegistrarAsync("  ADMIN ", "login_ok", "10.0.0.1");

        var acceso = await db.LogAccesos.SingleAsync();
        acceso.Usuario.Should().Be("admin");
        acceso.Evento.Should().Be("login_ok");
        acceso.Ip.Should().Be("10.0.0.1");
    }

    [Fact]
    public async Task Registrar_ErrorDeBd_NoLanzaExcepcion()
    {
        var db = CrearDb();
        var service = new RegistroAccesoService(db);
        db.Dispose(); // fuerza error al guardar

        var act = () => service.RegistrarAsync("admin", "login_ok", null);

        await act.Should().NotThrowAsync();
    }
}
