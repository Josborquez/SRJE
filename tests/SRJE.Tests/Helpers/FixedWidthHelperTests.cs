using SRJE.Web.Helpers;

namespace SRJE.Tests.Helpers;

public class FixedWidthHelperTests
{
    // --- NumIzq (long) ---

    [Theory]
    [InlineData(7051537, 9, "007051537")]
    [InlineData(0, 5, "00000")]
    [InlineData(123, 3, "123")]
    [InlineData(123, 2, "123")] // Overflow: no trunca
    public void NumIzq_Long_DeberiaRellenarConCerosALaIzquierda(long valor, int longitud, string esperado)
    {
        Assert.Equal(esperado, FixedWidthHelper.NumIzq(valor, longitud));
    }

    // --- NumIzq (decimal) ---

    [Theory]
    [InlineData(272116.50, 11, "00000272116")]
    [InlineData(0.99, 5, "00000")]       // Trunca decimales
    [InlineData(1000000, 7, "1000000")]
    public void NumIzq_Decimal_DeberiaTruncarDecimalesYRellenar(decimal valor, int longitud, string esperado)
    {
        Assert.Equal(esperado, FixedWidthHelper.NumIzq(valor, longitud));
    }

    // --- TextoDer ---

    [Theory]
    [InlineData("MORENO", 10, "MORENO    ")]
    [InlineData("ABCDEFGHIJ", 5, "ABCDE")]   // Trunca si es mas largo
    [InlineData(null, 5, "     ")]            // Null -> espacios
    [InlineData("", 3, "   ")]               // Vacio -> espacios
    public void TextoDer_DeberiaRellenarConEspaciosALaDerecha(string? valor, int longitud, string esperado)
    {
        Assert.Equal(esperado, FixedWidthHelper.TextoDer(valor, longitud));
    }

    // --- Leer ---

    [Fact]
    public void Leer_DeberiaExtraerSubstring()
    {
        var linea = "ABCDEFGHIJ";
        Assert.Equal("CDE", FixedWidthHelper.Leer(linea, 2, 3));
        Assert.Equal("A", FixedWidthHelper.Leer(linea, 0, 1));
    }

    [Fact]
    public void Leer_DeberiaLanzarExcepcion_CuandoLineaEsCorta()
    {
        var linea = "ABC";
        Assert.Throws<FormatException>(() => FixedWidthHelper.Leer(linea, 2, 5));
    }

    // --- LeerNumero ---

    [Fact]
    public void LeerNumero_DeberiaConvertirALong()
    {
        var linea = "  007051537  ";
        Assert.Equal(7051537, FixedWidthHelper.LeerNumero(linea, 2, 9));
    }

    [Fact]
    public void LeerNumero_DeberiaRetornarCero_CuandoNoEsNumero()
    {
        var linea = "  ABCDEF  ";
        Assert.Equal(0, FixedWidthHelper.LeerNumero(linea, 2, 6));
    }

    // --- LeerTexto ---

    [Fact]
    public void LeerTexto_DeberiaRemoverEspaciosTrailing()
    {
        var linea = "  MORENO   PEREZ     ";
        Assert.Equal("MORENO", FixedWidthHelper.LeerTexto(linea, 2, 9));
    }

    // --- Espacios y Ceros ---

    [Theory]
    [InlineData(5, "     ")]
    [InlineData(0, "")]
    public void Espacios_DeberiaGenerarBloqueDeEspacios(int longitud, string esperado)
    {
        Assert.Equal(esperado, FixedWidthHelper.Espacios(longitud));
    }

    [Theory]
    [InlineData(5, "00000")]
    [InlineData(0, "")]
    public void Ceros_DeberiaGenerarBloqueDeCeros(int longitud, string esperado)
    {
        Assert.Equal(esperado, FixedWidthHelper.Ceros(longitud));
    }
}
