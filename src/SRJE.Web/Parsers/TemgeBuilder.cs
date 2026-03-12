using System.Text;
using SRJE.Web.Helpers;
using SRJE.Web.Models.Entities;

namespace SRJE.Web.Parsers;

/// <summary>
/// Generador de archivo TEMGE en formato de ancho fijo compatible con el banco.
/// Layout validado con archivo real y codigo VB6 legacy.
/// Cabecera: 129 chars, Detalle: 130 chars, Cierre: 130 chars.
/// </summary>
public class TemgeBuilder
{
    private const string CodEmpresa = "06110104519640100572"; // 20 chars
    private const long CodBancoEstado = 12;

    public byte[] Generar(List<RegistroTemge> registros, DateTime fechaProceso)
    {
        var sb = new StringBuilder();
        var hora = fechaProceso.ToString("HHmmss");
        var fecha = fechaProceso.ToString("yyyyMMdd");

        // --- CABECERA (Tipo 1) - 129 chars ---
        sb.Append('1');                                      // Pos 1:    Tipo registro
        sb.Append(CodEmpresa);                               // Pos 2-21: Cod empresa (20 chars)
        sb.Append(FixedWidthHelper.Espacios(11));            // Pos 22-32: Espacios
        sb.Append(fecha);                                    // Pos 33-40: Fecha 1
        sb.Append(fecha);                                    // Pos 41-48: Fecha 2 (duplicado)
        sb.Append(hora);                                     // Pos 49-54: Hora
        sb.Append(FixedWidthHelper.Espacios(75));            // Pos 55-129: Espacios
        sb.Append("\r\n");

        decimal montoTotal = 0;
        int cantidadRegistros = 0;

        // --- DETALLE (Tipo 2) - 130 chars por registro ---
        foreach (var reg in registros)
        {
            sb.Append('2');                                                    // Pos 1
            sb.Append(FixedWidthHelper.NumIzq(reg.RutBeneficiario, 9));       // Pos 2-10
            sb.Append(FixedWidthHelper.TextoDer(reg.DvBeneficiario, 1));      // Pos 11
            sb.Append(FixedWidthHelper.TextoDer(reg.NombreBeneficiario, 39)); // Pos 12-50
            sb.Append('*');                                                    // Pos 51

            if (reg.CodBanco == CodBancoEstado)
            {
                sb.Append(' ');                                                // Pos 52: espacio
                sb.Append(FixedWidthHelper.Ceros(15));                         // Pos 53-67: ceros
            }
            else
            {
                sb.Append('=');                                                // Pos 52: igual
                sb.Append(FixedWidthHelper.NumIzq(
                    long.TryParse(reg.NumeroCuenta, out var nc) ? nc : 0, 15));// Pos 53-67
            }

            sb.Append(FixedWidthHelper.Ceros(11));                            // Pos 68-78: ceros fijos
            sb.Append(FixedWidthHelper.NumIzq(reg.TipoCuenta, 2));            // Pos 79-80
            sb.Append(FixedWidthHelper.NumIzq(reg.CodBanco, 3));              // Pos 81-83

            if (reg.CodBanco == CodBancoEstado)
            {
                // TEMGE format allows 11 chars; DB stores up to 15 — TextoDer truncates if longer
                sb.Append(FixedWidthHelper.TextoDer(reg.CtaEstado, 11));      // Pos 84-94
            }
            else
            {
                sb.Append(FixedWidthHelper.Ceros(11));                         // Pos 84-94
            }

            sb.Append(FixedWidthHelper.NumIzq(reg.Monto, 11));               // Pos 95-105
            sb.Append("00");                                                   // Pos 106-107: decimales
            sb.Append(FixedWidthHelper.Ceros(10));                            // Pos 108-117
            sb.Append(FixedWidthHelper.Ceros(11));                            // Pos 118-128
            sb.Append(FixedWidthHelper.Espacios(2));                          // Pos 129-130
            sb.Append("\r\n");

            montoTotal += reg.Monto;
            cantidadRegistros++;
        }

        // --- CIERRE (Tipo 3) - 130 chars ---
        sb.Append('3');                                                       // Pos 1
        sb.Append(FixedWidthHelper.NumIzq(montoTotal, 13));                  // Pos 2-14
        sb.Append("00");                                                      // Pos 15-16: decimales
        sb.Append(FixedWidthHelper.NumIzq(cantidadRegistros, 5));            // Pos 17-21
        sb.Append(FixedWidthHelper.Espacios(109));                           // Pos 22-130
        sb.Append("\r\n");

        return Encoding.Latin1.GetBytes(sb.ToString());
    }
}

public class RegistroTemge
{
    public long RutBeneficiario { get; set; }
    public string DvBeneficiario { get; set; } = string.Empty;
    public string NombreBeneficiario { get; set; } = string.Empty;
    public long CodBanco { get; set; }
    public long TipoCuenta { get; set; }
    public string? NumeroCuenta { get; set; }
    public string? CtaEstado { get; set; }
    public decimal Monto { get; set; }
}
