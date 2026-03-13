using System.Text;
using SRJE.Web.Parsers;

namespace SRJE.Tests.Parsers;

public class RemuneracionesParserTests
{
    private const int LargoLinea = 126;

    /// <summary>
    /// Construye una linea de ancho fijo de 126 chars con los datos indicados.
    /// Layout real: RUT_FUNC(9) DV_FUNC(1) RUT_BENEF(9) DV_BENEF(1) AP_PAT(20) AP_MAT(20) NOMBRES(30) MONTO(8) COD_RET(11) TIPO_PAGO(17)
    /// Nota: en el archivo, primero viene el funcionario (titular retenido) y luego el beneficiario.
    /// </summary>
    private static string BuildLinea(
        long rutFunc = 12345678, string dvFunc = "5",
        long rutBenef = 7051537, string dvBenef = "7",
        string apPaterno = "MORENO", string apMaterno = "PEREZ",
        string nombres = "JUAN CARLOS",
        long monto = 272116,
        string codRetencion = "DURETENF   ",
        string tipoPago = "PERMANENTE SR    ")
    {
        var sb = new StringBuilder();
        sb.Append(rutFunc.ToString().PadLeft(9, '0'));         // 0-8: Funcionario
        sb.Append(dvFunc.PadRight(1));                         // 9
        sb.Append(rutBenef.ToString().PadLeft(9, '0'));        // 10-18: Beneficiario
        sb.Append(dvBenef.PadRight(1));                        // 19
        sb.Append(apPaterno.PadRight(20));                     // 20-39
        sb.Append(apMaterno.PadRight(20));                     // 40-59
        sb.Append(nombres.PadRight(30));                       // 60-89
        sb.Append(monto.ToString().PadLeft(8, '0'));           // 90-97
        sb.Append(codRetencion.PadRight(11));                  // 98-108
        sb.Append(tipoPago.PadRight(17));                      // 109-125

        var linea = sb.ToString();
        // Asegurar largo exacto
        if (linea.Length < LargoLinea)
            linea = linea.PadRight(LargoLinea);
        return linea;
    }

    private static Stream ToStream(string contenido)
    {
        return new MemoryStream(Encoding.Latin1.GetBytes(contenido));
    }

    [Fact]
    public void Parsear_DeberiaLeerLineaCorrectamente()
    {
        var linea = BuildLinea();
        using var stream = ToStream(linea);

        var resultado = RemuneracionesParser.Parsear(stream);

        Assert.Single(resultado);
        var dto = resultado[0];
        Assert.Equal(7051537, dto.RutBeneficiario);
        Assert.Equal("7", dto.DvBeneficiario);
        Assert.Equal(12345678, dto.RutFuncionario);
        Assert.Equal("5", dto.DvFuncionario);
        Assert.Contains("MORENO", dto.NombreBeneficiario);
        Assert.Contains("PEREZ", dto.NombreBeneficiario);
        Assert.Contains("JUAN CARLOS", dto.NombreBeneficiario);
        Assert.Equal(272116, dto.Monto);
        Assert.Equal("OK", dto.EstadoLinea);
    }

    [Fact]
    public void Parsear_DeberiaIgnorarLineasVacias()
    {
        var contenido = "\n\n" + BuildLinea() + "\n\n";
        using var stream = ToStream(contenido);

        var resultado = RemuneracionesParser.Parsear(stream);

        Assert.Single(resultado);
    }

    [Fact]
    public void Parsear_DeberiaMarcarAdvertencia_CuandoRutEsInvalido()
    {
        // RUT beneficiario 7051537 con DV incorrecto "0" (real es "7")
        var linea = BuildLinea(dvBenef: "0");
        using var stream = ToStream(linea);

        var resultado = RemuneracionesParser.Parsear(stream);

        Assert.Single(resultado);
        Assert.Equal("ADVERTENCIA", resultado[0].EstadoLinea);
        Assert.Contains("RUT beneficiario", resultado[0].Mensaje);
    }

    [Fact]
    public void Parsear_DeberiaTruncarNombreA39Chars()
    {
        var linea = BuildLinea(
            apPaterno: "GONZALEZ DE LA FUENT",
            apMaterno: "RODRIGUEZ HERNANDEZ",
            nombres: "MARIA CRISTINA DEL ROSARIO AB"
        );
        using var stream = ToStream(linea);

        var resultado = RemuneracionesParser.Parsear(stream);

        Assert.Single(resultado);
        Assert.True(resultado[0].NombreBeneficiario.Length <= 39);
    }

    [Fact]
    public void Parsear_DeberiaMarcarError_CuandoLineaEsCorta()
    {
        using var stream = ToStream("LINEA_CORTA");

        var resultado = RemuneracionesParser.Parsear(stream);

        Assert.Single(resultado);
        Assert.Equal("ERROR", resultado[0].EstadoLinea);
    }

    [Fact]
    public void Parsear_DeberiaProcesarMultiplesLineas()
    {
        var contenido = BuildLinea(rutBenef: 7051537, dvBenef: "7") + "\n"
                      + BuildLinea(rutBenef: 12345678, dvBenef: "5");
        using var stream = ToStream(contenido);

        var resultado = RemuneracionesParser.Parsear(stream);

        Assert.Equal(2, resultado.Count);
        Assert.Equal(7051537, resultado[0].RutBeneficiario);
        Assert.Equal(12345678, resultado[1].RutBeneficiario);
    }
}
