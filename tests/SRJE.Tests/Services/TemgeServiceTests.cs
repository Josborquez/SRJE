using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SRJE.Web.Infrastructure.Data;
using SRJE.Web.Models;
using SRJE.Web.Models.Entities;
using SRJE.Web.Services;

namespace SRJE.Tests.Services;

public class TemgeServiceTests : IDisposable
{
    private readonly SrjeDbContext _db;
    private readonly TemgeService _service;

    public TemgeServiceTests()
    {
        var options = new DbContextOptionsBuilder<SrjeDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(
                Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _db = new SrjeDbContext(options);
        var settings = Options.Create(new SrjeSettings());
        var logger = new Mock<ILogger<TemgeService>>();
        _service = new TemgeService(_db, logger.Object, settings);
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    private Beneficiario CrearBeneficiario(long rut, string dv = "7", string nombre = "TEST BENEFICIARIO",
        long codBanco = 12, long tipoCuenta = 2, string? ctaEstado = "41762633599", string? ctaOtBanco = null)
    {
        return new Beneficiario
        {
            RutBeneficiario = rut,
            DvBeneficiario = dv,
            NombreBeneficiario = nombre,
            CodBanco = codBanco,
            TipoCuenta = tipoCuenta,
            CtaEstado = ctaEstado,
            CtaOtBanco = ctaOtBanco,
            Estado = "A"
        };
    }

    private RetenidoJudicial CrearRetencion(long rutBenef, decimal monto, string dv = "7",
        long? codBanco = null, long? tipoCuenta = null, string? ctaEstado = null, string? ctaOtBanco = null)
    {
        return new RetenidoJudicial
        {
            IdRetencion = 0,
            RutTitular = 12345678,
            DvTitular = "5",
            RutBeneficiario = rutBenef,
            DvBeneficiario = dv,
            Monto = monto,
            Estado = "A",
            PeriodoProceso = "260301",
            CodBanco = codBanco,
            TipoCuenta = tipoCuenta,
            CtaEstado = ctaEstado,
            CtaOtBanco = ctaOtBanco
        };
    }

    [Fact]
    public async Task GenerarTemge_DeberiaProducirUnaLineaPorRetencion()
    {
        // 1 beneficiario con 2 retenciones = 2 lineas de detalle
        var benef = CrearBeneficiario(7051537);
        _db.Beneficiarios.Add(benef);
        _db.RetenidosJudiciales.Add(CrearRetencion(7051537, 100000));
        _db.RetenidosJudiciales.Add(CrearRetencion(7051537, 200000));
        await _db.SaveChangesAsync();

        var archivo = await _service.GenerarTemgeAsync("test");
        var contenido = Encoding.Latin1.GetString(archivo);
        var lineas = contenido.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);

        // Cabecera + 2 detalles + cierre = 4 lineas
        lineas.Should().HaveCount(4);
        lineas.Count(l => l[0] == '2').Should().Be(2);
    }

    [Fact]
    public async Task GenerarTemge_DeberiaUsarCuentaDeRetencionSobreBeneficiario()
    {
        // Beneficiario con Banco Estado, retencion con otro banco
        var benef = CrearBeneficiario(7051537, codBanco: 12, ctaEstado: "11111111111");
        _db.Beneficiarios.Add(benef);
        _db.RetenidosJudiciales.Add(CrearRetencion(7051537, 100000,
            codBanco: 1, tipoCuenta: 1, ctaOtBanco: "222222222222222"));
        await _db.SaveChangesAsync();

        var archivo = await _service.GenerarTemgeAsync("test");
        var contenido = Encoding.Latin1.GetString(archivo);
        var detalle = contenido.Split("\r\n")[1];

        // Pos 52 (0-based 51): '=' indica otro banco (no Banco Estado)
        detalle[51].Should().Be('=');
        // Cod banco en pos 81-83 (0-based 80-82): "001"
        detalle.Substring(80, 3).Should().Be("001");
    }

    [Fact]
    public async Task GenerarTemge_DeberiaUsarFallbackACuentaBeneficiario()
    {
        // Retencion sin datos bancarios, debe usar los del beneficiario
        var benef = CrearBeneficiario(7051537, codBanco: 12, ctaEstado: "41762633599");
        _db.Beneficiarios.Add(benef);
        _db.RetenidosJudiciales.Add(CrearRetencion(7051537, 150000));
        await _db.SaveChangesAsync();

        var archivo = await _service.GenerarTemgeAsync("test");
        var contenido = Encoding.Latin1.GetString(archivo);
        var detalle = contenido.Split("\r\n")[1];

        // Banco Estado: espacio en pos 52
        detalle[51].Should().Be(' ');
        // Cod banco 012
        detalle.Substring(80, 3).Should().Be("012");
        // Cta Estado en pos 84-94
        detalle.Substring(83, 11).Should().Be("41762633599");
    }

    [Fact]
    public async Task GenerarTemge_DeberiaExcluirRetencionesSinCuentaBancaria()
    {
        // Beneficiario sin cuenta, retencion sin cuenta -> excluido
        var benef = new Beneficiario
        {
            RutBeneficiario = 7051537,
            DvBeneficiario = "7",
            NombreBeneficiario = "SIN CUENTA",
            Estado = "A"
            // Sin CtaEstado ni CtaOtBanco
        };
        _db.Beneficiarios.Add(benef);
        _db.RetenidosJudiciales.Add(CrearRetencion(7051537, 100000));
        await _db.SaveChangesAsync();

        var archivo = await _service.GenerarTemgeAsync("test");
        var contenido = Encoding.Latin1.GetString(archivo);
        var lineas = contenido.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);

        // Solo cabecera y cierre, sin detalles
        lineas.Should().HaveCount(2);
        lineas.Count(l => l[0] == '2').Should().Be(0);
    }

    [Fact]
    public async Task GenerarTemge_DeberiaRegistrarIdRetenidoJudicial()
    {
        var benef = CrearBeneficiario(7051537);
        _db.Beneficiarios.Add(benef);
        _db.RetenidosJudiciales.Add(CrearRetencion(7051537, 100000));
        await _db.SaveChangesAsync();

        await _service.GenerarTemgeAsync("test");

        var detalles = await _db.DetallePagosTemge.ToListAsync();
        detalles.Should().HaveCount(1);
        detalles[0].IdRetenidoJudicial.Should().NotBeNull();
        detalles[0].IdRetenidoJudicial.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GenerarTemge_BeneficiarioConRetencionesCuentasDiferentes()
    {
        // Caso real: 1 beneficiario, 2 retenciones con cuentas distintas
        var benef = CrearBeneficiario(16773206, dv: "2", codBanco: 12, ctaEstado: "01065282360");
        _db.Beneficiarios.Add(benef);

        // Retencion 1: usa cuenta del beneficiario (fallback)
        _db.RetenidosJudiciales.Add(CrearRetencion(16773206, 323400, dv: "2"));

        // Retencion 2: cuenta propia diferente
        _db.RetenidosJudiciales.Add(CrearRetencion(16773206, 488684, dv: "2",
            codBanco: 12, tipoCuenta: 2, ctaEstado: "01065932012"));

        await _db.SaveChangesAsync();

        var archivo = await _service.GenerarTemgeAsync("test");
        var contenido = Encoding.Latin1.GetString(archivo);
        var lineas = contenido.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);

        // 2 lineas de detalle (no agrupadas)
        lineas.Count(l => l[0] == '2').Should().Be(2);

        // Verificar que las cuentas son diferentes
        var detalles = lineas.Where(l => l[0] == '2').ToList();
        var cta1 = detalles[0].Substring(83, 11);
        var cta2 = detalles[1].Substring(83, 11);
        new[] { cta1, cta2 }.Should().Contain("01065282360");
        new[] { cta1, cta2 }.Should().Contain("01065932012");

        // Monto total en cierre
        var cierre = lineas.Last();
        var montoTotal = long.Parse(cierre.Substring(1, 13));
        montoTotal.Should().Be(323400 + 488684);
    }

    [Fact]
    public async Task GenerarTemge_DeberiaExcluirBeneficiariosInactivos()
    {
        var benef = CrearBeneficiario(7051537);
        benef.Estado = "I";
        _db.Beneficiarios.Add(benef);
        _db.RetenidosJudiciales.Add(CrearRetencion(7051537, 100000));
        await _db.SaveChangesAsync();

        var archivo = await _service.GenerarTemgeAsync("test");
        var contenido = Encoding.Latin1.GetString(archivo);
        var lineas = contenido.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);

        lineas.Count(l => l[0] == '2').Should().Be(0);
    }

    [Fact]
    public async Task GenerarTemge_DeberiaExcluirRetencionesInactivas()
    {
        var benef = CrearBeneficiario(7051537);
        _db.Beneficiarios.Add(benef);
        var ret = CrearRetencion(7051537, 100000);
        ret.Estado = "I";
        _db.RetenidosJudiciales.Add(ret);
        await _db.SaveChangesAsync();

        var archivo = await _service.GenerarTemgeAsync("test");
        var contenido = Encoding.Latin1.GetString(archivo);
        var lineas = contenido.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);

        lineas.Count(l => l[0] == '2').Should().Be(0);
    }

    [Fact]
    public async Task GenerarTemge_DeberiaFiltrarPorPeriodo()
    {
        var benef = CrearBeneficiario(7051537);
        _db.Beneficiarios.Add(benef);

        // Retencion periodo febrero
        var retFeb = CrearRetencion(7051537, 100000);
        retFeb.PeriodoProceso = "260201";
        _db.RetenidosJudiciales.Add(retFeb);

        // Retencion periodo marzo
        var retMar = CrearRetencion(7051537, 200000);
        retMar.PeriodoProceso = "260301";
        _db.RetenidosJudiciales.Add(retMar);

        await _db.SaveChangesAsync();

        // Sin filtro: ambas retenciones
        var archivoTodo = await _service.GenerarTemgeAsync("test");
        var lineasTodo = Encoding.Latin1.GetString(archivoTodo).Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
        lineasTodo.Count(l => l[0] == '2').Should().Be(2);

        // Solo marzo
        var archivoMarzo = await _service.GenerarTemgeAsync("test", "260301");
        var lineasMarzo = Encoding.Latin1.GetString(archivoMarzo).Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
        lineasMarzo.Count(l => l[0] == '2').Should().Be(1);

        // Verificar monto de marzo
        var detalle = lineasMarzo.First(l => l[0] == '2');
        var monto = long.Parse(detalle.Substring(94, 11));
        monto.Should().Be(200000);
    }
}
