using SRJE.Web.Helpers;

namespace SRJE.Tests.Helpers;

public class RutHelperTests
{
    [Theory]
    [InlineData(7051537, "7")]
    [InlineData(12345678, "5")]
    [InlineData(11111111, "1")]
    [InlineData(22222222, "2")]
    [InlineData(1, "9")]
    public void CalcularDv_DeberiaRetornarDvCorrecto(long rut, string dvEsperado)
    {
        var resultado = RutHelper.CalcularDv(rut);
        Assert.Equal(dvEsperado, resultado);
    }

    [Theory]
    [InlineData(18585543, "K")]
    public void CalcularDv_DeberiaRetornarK_CuandoRestoEs10(long rut, string dvEsperado)
    {
        var resultado = RutHelper.CalcularDv(rut);
        Assert.Equal(dvEsperado, resultado);
    }

    [Theory]
    [InlineData(7051537, "7", true)]
    [InlineData(7051537, "8", false)]
    [InlineData(18585543, "K", true)]
    [InlineData(18585543, "k", true)]   // Case insensitive
    [InlineData(18585543, "1", false)]
    [InlineData(0, "0", false)]          // RUT invalido
    [InlineData(-1, "0", false)]         // RUT negativo
    public void Validar_DeberiaRetornarResultadoEsperado(long rut, string dv, bool esperado)
    {
        var resultado = RutHelper.Validar(rut, dv);
        Assert.Equal(esperado, resultado);
    }

    [Fact]
    public void Validar_DeberiaRetornarFalse_CuandoDvEsVacio()
    {
        Assert.False(RutHelper.Validar(7051537, ""));
        Assert.False(RutHelper.Validar(7051537, null!));
    }

    [Theory]
    [InlineData(7051537, "7", "7.051.537-7")]
    [InlineData(12345678, "5", "12.345.678-5")]
    [InlineData(1, "9", "1-9")]
    public void Formatear_DeberiaRetornarFormatoCorrecto(long rut, string dv, string esperado)
    {
        var resultado = RutHelper.Formatear(rut, dv);
        Assert.Equal(esperado, resultado);
    }

    [Theory]
    [InlineData("7.051.537-7", 7051537, "7")]
    [InlineData("7051537-7", 7051537, "7")]
    [InlineData("12345678-5", 12345678, "5")]
    [InlineData("18.585.543-K", 18585543, "K")]
    [InlineData("18585543-k", 18585543, "K")]
    public void Parsear_DeberiaExtraerRutYDv(string input, long rutEsperado, string dvEsperado)
    {
        var resultado = RutHelper.Parsear(input);
        Assert.NotNull(resultado);
        Assert.Equal(rutEsperado, resultado.Value.rut);
        Assert.Equal(dvEsperado, resultado.Value.dv);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    [InlineData("X")]
    public void Parsear_DeberiaRetornarNull_CuandoInputEsInvalido(string? input)
    {
        var resultado = RutHelper.Parsear(input!);
        Assert.Null(resultado);
    }
}
