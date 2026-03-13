using System.Text;
using SRJE.Web.Models;
using SRJE.Web.Parsers;

namespace SRJE.Tests.Parsers;

public class TemgeBuilderTests
{
    private static RegistroTemge CrearRegistro(
        long rut = 7051537, string dv = "7", string nombre = "MORENO PEREZ JUAN",
        long codBanco = 12, long tipoCuenta = 2, string? ctaEstado = "41762633599",
        string? numeroCuenta = null, decimal monto = 272116)
    {
        return new RegistroTemge
        {
            RutBeneficiario = rut,
            DvBeneficiario = dv,
            NombreBeneficiario = nombre,
            CodBanco = codBanco,
            TipoCuenta = tipoCuenta,
            CtaEstado = ctaEstado,
            NumeroCuenta = numeroCuenta,
            Monto = monto
        };
    }

    [Fact]
    public void Generar_DeberiaCrearArchivoCon3Secciones()
    {
        var registros = new List<RegistroTemge> { CrearRegistro() };
        var builder = new TemgeBuilder(new SrjeSettings());
        var fecha = new DateTime(2026, 3, 12, 10, 30, 0);

        var bytes = builder.Generar(registros, fecha);
        var contenido = Encoding.Latin1.GetString(bytes);
        var lineas = contenido.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal(3, lineas.Length);
        Assert.Equal('1', lineas[0][0]); // Cabecera
        Assert.Equal('2', lineas[1][0]); // Detalle
        Assert.Equal('3', lineas[2][0]); // Cierre
    }

    [Fact]
    public void Generar_CabeceraDeberiaContenerFechaYHora()
    {
        var registros = new List<RegistroTemge> { CrearRegistro() };
        var builder = new TemgeBuilder(new SrjeSettings());
        var fecha = new DateTime(2026, 3, 12, 14, 25, 30);

        var bytes = builder.Generar(registros, fecha);
        var contenido = Encoding.Latin1.GetString(bytes);
        var cabecera = contenido.Split("\r\n")[0];

        Assert.Contains("20260312", cabecera); // Fecha
        Assert.Contains("142530", cabecera);   // Hora
    }

    [Fact]
    public void Generar_CabeceraDeberiaContenerCodEmpresa()
    {
        var registros = new List<RegistroTemge> { CrearRegistro() };
        var builder = new TemgeBuilder(new SrjeSettings());

        var bytes = builder.Generar(registros, DateTime.Now);
        var contenido = Encoding.Latin1.GetString(bytes);

        Assert.Contains("06110104519640100572", contenido);
    }

    [Fact]
    public void Generar_CierreDeberiaContenerMontoTotalYCantidad()
    {
        var registros = new List<RegistroTemge>
        {
            CrearRegistro(monto: 100000),
            CrearRegistro(rut: 12345678, dv: "5", monto: 200000)
        };
        var builder = new TemgeBuilder(new SrjeSettings());

        var bytes = builder.Generar(registros, DateTime.Now);
        var contenido = Encoding.Latin1.GetString(bytes);
        var lineas = contenido.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
        var cierre = lineas[^1];

        Assert.Equal('3', cierre[0]);
        // Monto total: 300000 en 13 posiciones
        var montoStr = cierre.Substring(1, 13);
        Assert.Equal(300000, long.Parse(montoStr));
        // Cantidad: 2 en 5 posiciones (pos 17-21, 0-based: 16-20)
        var cantStr = cierre.Substring(16, 5);
        Assert.Equal(2, int.Parse(cantStr));
    }

    [Fact]
    public void Generar_BancoEstado_DeberiaUsarCtaEstadoYEspacioEnIndicador()
    {
        var reg = CrearRegistro(codBanco: 12, ctaEstado: "41762633599");
        var builder = new TemgeBuilder(new SrjeSettings());

        var bytes = builder.Generar(new List<RegistroTemge> { reg }, DateTime.Now);
        var contenido = Encoding.Latin1.GetString(bytes);
        var detalle = contenido.Split("\r\n")[1];

        // Pos 52 (0-based: 51): espacio para BancoEstado
        Assert.Equal(' ', detalle[51]);
        // Pos 53-67 (0-based: 52-66): ceros para BancoEstado
        Assert.Equal("000000000000000", detalle.Substring(52, 15));
    }

    [Fact]
    public void Generar_OtroBanco_DeberiaUsarCtaOtBancoEIgualEnIndicador()
    {
        var reg = CrearRegistro(codBanco: 1, numeroCuenta: "12345678901", ctaEstado: null);
        var builder = new TemgeBuilder(new SrjeSettings());

        var bytes = builder.Generar(new List<RegistroTemge> { reg }, DateTime.Now);
        var contenido = Encoding.Latin1.GetString(bytes);
        var detalle = contenido.Split("\r\n")[1];

        // Pos 52 (0-based: 51): '=' para otro banco
        Assert.Equal('=', detalle[51]);
    }

    [Fact]
    public void Generar_DeberiaUsarEncodingLatin1()
    {
        var reg = CrearRegistro(nombre: "MUNOZ GONZALEZ JOSE");
        var builder = new TemgeBuilder(new SrjeSettings());

        var bytes = builder.Generar(new List<RegistroTemge> { reg }, DateTime.Now);

        // Should be valid Latin1
        var contenido = Encoding.Latin1.GetString(bytes);
        Assert.Contains("MUNOZ", contenido);
    }

    [Fact]
    public void Generar_SinRegistros_DeberiaTenerSoloCabeceraYCierre()
    {
        var builder = new TemgeBuilder(new SrjeSettings());

        var bytes = builder.Generar(new List<RegistroTemge>(), DateTime.Now);
        var contenido = Encoding.Latin1.GetString(bytes);
        var lineas = contenido.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal(2, lineas.Length); // Solo cabecera y cierre
        Assert.Equal('1', lineas[0][0]);
        Assert.Equal('3', lineas[1][0]);
    }
}
