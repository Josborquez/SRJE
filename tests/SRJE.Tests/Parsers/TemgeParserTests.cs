using System.Text;
using SRJE.Web.Parsers;

namespace SRJE.Tests.Parsers;

public class TemgeParserTests
{
    /// <summary>
    /// Genera un archivo TEMGE completo en memoria para tests.
    /// </summary>
    private static Stream BuildTemgeStream(
        List<(long rut, string dv, string nombre, decimal monto)>? registros = null)
    {
        registros ??= new() { (7051537, "7", "MORENO PEREZ JUAN", 272116) };

        var sb = new StringBuilder();

        // Cabecera (tipo 1) - 129 chars
        sb.Append('1');
        sb.Append("06110104519640100572"); // Pos 2-21: CodEmpresa (20 chars)
        sb.Append(new string(' ', 12));     // Pos 22-33
        sb.Append("20260312");              // Pos 34-41: Fecha
        sb.Append(new string(' ', 8));      // Pos 42-49
        sb.Append("143000");                // Pos 50-55: Hora
        sb.Append(new string(' ', 74));     // Pos 56-129
        sb.Append("\r\n");

        decimal montoTotal = 0;
        foreach (var (rut, dv, nombre, monto) in registros)
        {
            // Detalle (tipo 2) - 130 chars
            sb.Append('2');
            sb.Append(rut.ToString().PadLeft(9, '0'));  // Pos 2-10
            sb.Append(dv);                               // Pos 11
            sb.Append(nombre.PadRight(39));              // Pos 12-50
            sb.Append('*');                               // Pos 51
            sb.Append(' ');                               // Pos 52
            sb.Append("000000000000000");                 // Pos 53-67
            sb.Append("00000000000");                     // Pos 68-78
            sb.Append("02");                              // Pos 79-80: TipoCuenta
            sb.Append("012");                             // Pos 81-83: CodBanco
            sb.Append("41762633599");                     // Pos 84-94: CtaEstado
            sb.Append(((long)monto).ToString().PadLeft(11, '0')); // Pos 95-105
            sb.Append("00");                              // Pos 106-107
            sb.Append("0000000000");                      // Pos 108-117
            sb.Append("00000000000");                     // Pos 118-128
            sb.Append("  ");                              // Pos 129-130
            sb.Append("\r\n");
            montoTotal += monto;
        }

        // Cierre (tipo 3) - 129 chars
        sb.Append('3');
        sb.Append(((long)montoTotal).ToString().PadLeft(13, '0'));  // Pos 2-14
        sb.Append("00");                                             // Pos 15-16
        sb.Append(registros.Count.ToString().PadLeft(5, '0'));       // Pos 17-21
        sb.Append(new string(' ', 109));                             // Pos 22-130
        sb.Append("\r\n");

        return new MemoryStream(Encoding.Latin1.GetBytes(sb.ToString()));
    }

    [Fact]
    public void Parsear_DeberiaLeerCabeceraCorrectamente()
    {
        using var stream = BuildTemgeStream();

        var resultado = TemgeParser.Parsear(stream);

        Assert.Equal("06110104519640100572", resultado.CodEmpresa);
        Assert.Equal("20260312", resultado.FechaProceso);
        Assert.Equal("143000", resultado.HoraProceso);
    }

    [Fact]
    public void Parsear_DeberiaLeerDetallesCorrectamente()
    {
        using var stream = BuildTemgeStream();

        var resultado = TemgeParser.Parsear(stream);

        Assert.Single(resultado.Lineas);
        var linea = resultado.Lineas[0];
        Assert.Equal(7051537, linea.RutBeneficiario);
        Assert.Equal("7", linea.DvBeneficiario);
        Assert.Contains("MORENO", linea.NombreBeneficiario);
        Assert.Equal(272116, linea.Monto);
        Assert.Equal(12, linea.CodBanco);
        Assert.Equal(2, linea.TipoCuenta);
    }

    [Fact]
    public void Parsear_DeberiaLeerCierreCorrectamente()
    {
        using var stream = BuildTemgeStream();

        var resultado = TemgeParser.Parsear(stream);

        Assert.Equal(272116, resultado.MontoTotalCierre);
        Assert.Equal(1, resultado.TotalRegistrosCierre);
    }

    [Fact]
    public void IntegridadOk_DeberiaSerTrue_CuandoTotalesCoinciden()
    {
        using var stream = BuildTemgeStream();

        var resultado = TemgeParser.Parsear(stream);

        Assert.True(resultado.IntegridadOk);
    }

    [Fact]
    public void Parsear_DeberiaLeerMultiplesRegistros()
    {
        var registros = new List<(long, string, string, decimal)>
        {
            (7051537, "7", "MORENO PEREZ JUAN", 100000),
            (12345678, "5", "GONZALEZ LOPEZ MARIA", 200000)
        };
        using var stream = BuildTemgeStream(registros);

        var resultado = TemgeParser.Parsear(stream);

        Assert.Equal(2, resultado.Lineas.Count);
        Assert.Equal(300000, resultado.MontoTotalCierre);
        Assert.True(resultado.IntegridadOk);
    }

    [Fact]
    public void Parsear_DeberiaMarcarAdvertencia_CuandoRutEsInvalido()
    {
        var registros = new List<(long, string, string, decimal)>
        {
            (7051537, "0", "MORENO PEREZ JUAN", 272116) // DV incorrecto
        };
        using var stream = BuildTemgeStream(registros);

        var resultado = TemgeParser.Parsear(stream);

        Assert.Single(resultado.Lineas);
        Assert.Equal("ADVERTENCIA", resultado.Lineas[0].EstadoLinea);
    }
}
