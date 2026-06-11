using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SRJE.Web.Infrastructure.Data;
using SRJE.Web.Models.Entities;
using SRJE.Web.Services;

namespace SRJE.Tests.Services;

public class DbAuthServiceTests : IDisposable
{
    private readonly SrjeDbContext _db;
    private readonly DbAuthService _service;

    public DbAuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<SrjeDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new SrjeDbContext(options);
        _service = new DbAuthService(_db);
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    private async Task SeedUsuario(
        string usuario = "admin", string password = "Secreta#2026",
        string rol = "admin", string estado = "A")
    {
        _db.UsuariosSistema.Add(new UsuarioSistema
        {
            Usuario = usuario,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            NombreCompleto = "Usuario Test",
            Rol = rol,
            Estado = estado
        });
        await _db.SaveChangesAsync();
    }

    [Fact]
    public async Task ValidarCredenciales_PasswordCorrecta_RetornaUsuario()
    {
        await SeedUsuario();

        var result = await _service.ValidarCredencialesAsync("admin", "Secreta#2026");

        result.Should().NotBeNull();
        result!.Usuario.Should().Be("admin");
        result.Rol.Should().Be("admin");
    }

    [Fact]
    public async Task ValidarCredenciales_NormalizaUsuario_RetornaUsuario()
    {
        await SeedUsuario();

        var result = await _service.ValidarCredencialesAsync("  ADMIN ", "Secreta#2026");

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ValidarCredenciales_PasswordIncorrecta_RetornaNull()
    {
        await SeedUsuario();

        var result = await _service.ValidarCredencialesAsync("admin", "incorrecta");

        result.Should().BeNull();
    }

    [Fact]
    public async Task ValidarCredenciales_UsuarioInactivo_RetornaNull()
    {
        await SeedUsuario(estado: "I");

        var result = await _service.ValidarCredencialesAsync("admin", "Secreta#2026");

        result.Should().BeNull();
    }

    [Fact]
    public async Task ValidarCredenciales_UsuarioInexistente_RetornaNull()
    {
        var result = await _service.ValidarCredencialesAsync("nadie", "loquesea");

        result.Should().BeNull();
    }

    [Theory]
    [InlineData("", "pass")]
    [InlineData("admin", "")]
    [InlineData("  ", "pass")]
    public async Task ValidarCredenciales_EntradaVacia_RetornaNull(string usuario, string password)
    {
        var result = await _service.ValidarCredencialesAsync(usuario, password);

        result.Should().BeNull();
    }
}
