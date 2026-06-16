using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SRJE.Web.Infrastructure.Data;
using SRJE.Web.Models.Entities;
using SRJE.Web.Models.Exceptions;
using SRJE.Web.Models.Requests;
using SRJE.Web.Services;

namespace SRJE.Tests.Services;

public class UsuariosServiceTests : IDisposable
{
    private readonly SrjeDbContext _db;
    private readonly UsuariosService _service;

    public UsuariosServiceTests()
    {
        var options = new DbContextOptionsBuilder<SrjeDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new SrjeDbContext(options);
        _service = new UsuariosService(_db);
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    private async Task SeedUsuario(string usuario = "admin", string rol = "admin", string estado = "A")
    {
        _db.UsuariosSistema.Add(new UsuarioSistema
        {
            Usuario = usuario,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Secreta#2026"),
            NombreCompleto = "Usuario Test",
            Rol = rol,
            Estado = estado
        });
        await _db.SaveChangesAsync();
    }

    private static CrearUsuarioRequest RequestValido(string usuario = "nuevo") => new()
    {
        Usuario = usuario,
        Password = "Clave#2026",
        NombreCompleto = "Nuevo Usuario",
        Rol = "operador"
    };

    [Fact]
    public async Task Crear_HasheaPassword_YNoExponeHash()
    {
        var dto = await _service.CrearAsync(RequestValido());

        dto.Usuario.Should().Be("nuevo");
        dto.Rol.Should().Be("operador");
        dto.Estado.Should().Be("A");

        var entidad = await _db.UsuariosSistema.SingleAsync(u => u.Usuario == "nuevo");
        entidad.PasswordHash.Should().NotBe("Clave#2026");
        BCrypt.Net.BCrypt.Verify("Clave#2026", entidad.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task Crear_UsuarioDuplicado_LanzaConflict()
    {
        await SeedUsuario("nuevo", rol: "operador");

        var act = () => _service.CrearAsync(RequestValido("nuevo"));

        await act.Should().ThrowAsync<BusinessConflictException>();
    }

    [Fact]
    public async Task Crear_RolInvalido_LanzaArgumentException()
    {
        var request = RequestValido();
        request.Rol = "superadmin";

        var act = () => _service.CrearAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Crear_NormalizaUsuarioAMinusculas()
    {
        var dto = await _service.CrearAsync(RequestValido("  NuEvO "));

        dto.Usuario.Should().Be("nuevo");
    }

    [Fact]
    public async Task Actualizar_CambiaNombreYRol()
    {
        await SeedUsuario("operador1", rol: "operador");

        var dto = await _service.ActualizarAsync("operador1",
            new ActualizarUsuarioRequest { NombreCompleto = "Otro Nombre", Rol = "consulta" },
            usuarioActual: "admin");

        dto.NombreCompleto.Should().Be("Otro Nombre");
        dto.Rol.Should().Be("consulta");
    }

    [Fact]
    public async Task Actualizar_PropioRol_LanzaArgumentException()
    {
        await SeedUsuario("admin", rol: "admin");

        var act = () => _service.ActualizarAsync("admin",
            new ActualizarUsuarioRequest { NombreCompleto = "Admin", Rol = "operador" },
            usuarioActual: "admin");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Actualizar_UsuarioInexistente_LanzaKeyNotFound()
    {
        var act = () => _service.ActualizarAsync("fantasma",
            new ActualizarUsuarioRequest { NombreCompleto = "X", Rol = "consulta" },
            usuarioActual: "admin");

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task CambiarPassword_ActualizaHash()
    {
        await SeedUsuario("operador1", rol: "operador");
        var hashAnterior = (await _db.UsuariosSistema.SingleAsync(u => u.Usuario == "operador1")).PasswordHash;

        await _service.CambiarPasswordAsync("operador1", "NuevaClave#1");

        var entidad = await _db.UsuariosSistema.SingleAsync(u => u.Usuario == "operador1");
        entidad.PasswordHash.Should().NotBe(hashAnterior);
        BCrypt.Net.BCrypt.Verify("NuevaClave#1", entidad.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task Toggle_PropiaCuenta_LanzaArgumentException()
    {
        await SeedUsuario("admin", rol: "admin");

        var act = () => _service.ToggleEstadoAsync("admin", usuarioActual: "admin");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Toggle_OtroUsuario_AlternaEstado()
    {
        await SeedUsuario("operador1", rol: "operador", estado: "A");

        var dto = await _service.ToggleEstadoAsync("operador1", usuarioActual: "admin");
        dto.Estado.Should().Be("I");

        dto = await _service.ToggleEstadoAsync("operador1", usuarioActual: "admin");
        dto.Estado.Should().Be("A");
    }

    [Fact]
    public async Task ListarAccesos_FiltraYPagina()
    {
        for (var i = 0; i < 5; i++)
        {
            _db.LogAccesos.Add(new LogAcceso { Usuario = "admin", Evento = "login_ok", Fecha = DateTime.Now.AddMinutes(-i) });
        }
        _db.LogAccesos.Add(new LogAcceso { Usuario = "otro", Evento = "login_fail", Fecha = DateTime.Now });
        await _db.SaveChangesAsync();

        var result = await _service.ListarAccesosAsync(new BuscarAccesosQuery { Usuario = "admin", Page = 1, PageSize = 3 });

        result.TotalCount.Should().Be(5);
        result.Items.Should().HaveCount(3);
        result.Items.Should().OnlyContain(a => a.Usuario == "admin");
        result.Items.Should().BeInDescendingOrder(a => a.Fecha);

        var porEvento = await _service.ListarAccesosAsync(new BuscarAccesosQuery { Evento = "login_fail" });
        porEvento.TotalCount.Should().Be(1);
        porEvento.Items[0].Usuario.Should().Be("otro");
    }

    [Fact]
    public async Task ListarCargas_FiltraPorUsuarioYPagina()
    {
        for (var i = 0; i < 4; i++)
        {
            _db.LogCargas.Add(new LogCarga
            {
                TipoCarga = "remuneraciones",
                Usuario = "admin",
                FechaInicio = DateTime.Now.AddHours(-i)
            });
        }
        _db.LogCargas.Add(new LogCarga { TipoCarga = "temge", Usuario = "otro", FechaInicio = DateTime.Now });
        await _db.SaveChangesAsync();

        var result = await _service.ListarCargasAsync(new BuscarCargasQuery { Usuario = "admin", Page = 1, PageSize = 2 });

        result.TotalCount.Should().Be(4);
        result.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(c => c.Usuario == "admin");
        result.Items.Should().BeInDescendingOrder(c => c.FechaInicio);
    }
}
