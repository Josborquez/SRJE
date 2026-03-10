namespace SRJE.Web.Helpers;

/// <summary>
/// Helper unico para todas las operaciones de ancho fijo (DRY).
/// Usado por: RemuneracionesParser, TemgeParser y TemgeBuilder.
/// </summary>
public static class FixedWidthHelper
{
    /// <summary>
    /// Rellena izquierda con ceros (campos numericos).
    /// Ejemplo: NumIzq(7051537, 9) → "007051537"
    /// </summary>
    public static string NumIzq(long valor, int longitud)
    {
        return valor.ToString().PadLeft(longitud, '0');
    }

    /// <summary>
    /// Rellena izquierda con ceros para montos decimales truncados a entero.
    /// Ejemplo: NumIzq(272116.50m, 11) → "00000272116"
    /// </summary>
    public static string NumIzq(decimal valor, int longitud)
    {
        return ((long)valor).ToString().PadLeft(longitud, '0');
    }

    /// <summary>
    /// Rellena derecha con espacios (campos alfanumericos).
    /// Ejemplo: TextoDer("MORENO", 39) → "MORENO                                 "
    /// </summary>
    public static string TextoDer(string? valor, int longitud)
    {
        var texto = (valor ?? "").PadRight(longitud);
        return texto.Length > longitud ? texto[..longitud] : texto;
    }

    /// <summary>
    /// Lee campo de ancho fijo desde una linea (0-based).
    /// </summary>
    public static string Leer(string linea, int inicio, int longitud)
    {
        if (linea.Length < inicio + longitud)
            throw new FormatException(
                $"Linea de {linea.Length} chars es mas corta que lo esperado en pos {inicio}+{longitud}");

        return linea.Substring(inicio, longitud);
    }

    /// <summary>
    /// Lee campo de ancho fijo y lo convierte a long.
    /// </summary>
    public static long LeerNumero(string linea, int inicio, int longitud)
    {
        var campo = Leer(linea, inicio, longitud).Trim();
        return long.TryParse(campo, out var valor) ? valor : 0;
    }

    /// <summary>
    /// Lee campo de ancho fijo y lo retorna sin espacios trailing.
    /// </summary>
    public static string LeerTexto(string linea, int inicio, int longitud)
    {
        return Leer(linea, inicio, longitud).TrimEnd();
    }

    /// <summary>
    /// Genera un bloque de espacios.
    /// </summary>
    public static string Espacios(int longitud)
    {
        return new string(' ', longitud);
    }

    /// <summary>
    /// Genera un bloque de ceros.
    /// </summary>
    public static string Ceros(int longitud)
    {
        return new string('0', longitud);
    }
}
