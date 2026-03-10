namespace SRJE.Web.Helpers;

public static class RutHelper
{
    private static readonly int[] Factores = { 2, 3, 4, 5, 6, 7 };

    /// <summary>
    /// Calcula el digito verificador de un RUT chileno (modulo 11).
    /// </summary>
    public static string CalcularDv(long rut)
    {
        var suma = 0;
        var numero = rut.ToString();

        for (int i = numero.Length - 1, j = 0; i >= 0; i--, j++)
        {
            suma += (numero[i] - '0') * Factores[j % Factores.Length];
        }

        var resto = 11 - (suma % 11);

        return resto switch
        {
            11 => "0",
            10 => "K",
            _ => resto.ToString()
        };
    }

    /// <summary>
    /// Valida que el DV corresponda al RUT usando modulo 11.
    /// </summary>
    public static bool Validar(long rut, string dv)
    {
        if (rut <= 0 || string.IsNullOrEmpty(dv))
            return false;

        return CalcularDv(rut).Equals(dv.ToUpper().Trim());
    }

    /// <summary>
    /// Formatea un RUT: 7.051.537-7
    /// </summary>
    public static string Formatear(long rut, string dv)
    {
        return $"{rut:N0}-{dv}".Replace(",", ".");
    }

    /// <summary>
    /// Parsea un RUT formateado (7.051.537-7 o 7051537-7) a sus componentes.
    /// </summary>
    public static (long rut, string dv)? Parsear(string rutFormateado)
    {
        if (string.IsNullOrWhiteSpace(rutFormateado))
            return null;

        var limpio = rutFormateado.Replace(".", "").Replace("-", "").Trim().ToUpper();

        if (limpio.Length < 2)
            return null;

        var dvStr = limpio[^1..];
        var rutStr = limpio[..^1];

        if (!long.TryParse(rutStr, out var rut))
            return null;

        return (rut, dvStr);
    }
}
