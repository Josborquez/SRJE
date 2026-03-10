using SRJE.Web.Helpers;
using SRJE.Web.Models.ViewModels;

namespace SRJE.Web.Parsers;

/// <summary>
/// Parser para archivo TEMGE bancario (ancho fijo).
/// Cabecera: 129 chars (tipo '1'), Detalle: 130 chars (tipo '2'), Cierre: 129 chars (tipo '3').
/// Layout validado con archivo real.
/// </summary>
public static class TemgeParser
{
    public static TemgeArchivoDto Parsear(Stream stream)
    {
        var resultado = new TemgeArchivoDto();
        using var reader = new StreamReader(stream, System.Text.Encoding.Latin1);

        int numLinea = 0;
        string? linea;
        while ((linea = reader.ReadLine()) != null)
        {
            numLinea++;
            if (string.IsNullOrWhiteSpace(linea))
                continue;

            var tipoRegistro = linea[0];

            switch (tipoRegistro)
            {
                case '1':
                    ParsearCabecera(linea, resultado);
                    break;

                case '2':
                    var detalle = ParsearDetalle(linea, numLinea);
                    resultado.Lineas.Add(detalle);
                    break;

                case '3':
                    ParsearCierre(linea, resultado);
                    break;
            }
        }

        return resultado;
    }

    private static void ParsearCabecera(string linea, TemgeArchivoDto dto)
    {
        // Pos 2-21 (0-based: 1..20): COD_EMPRESA (20 chars)
        dto.CodEmpresa = FixedWidthHelper.LeerTexto(linea, 1, 20);
        // Pos 34-41 (0-based: 33..40): FECHA1
        dto.FechaProceso = FixedWidthHelper.LeerTexto(linea, 33, 8);
        // Pos 50-55 (0-based: 49..54): HORA
        dto.HoraProceso = FixedWidthHelper.LeerTexto(linea, 49, 6);
    }

    private static PreviewLineaDto ParsearDetalle(string linea, int numLinea)
    {
        var dto = new PreviewLineaDto { NumeroLinea = numLinea };

        try
        {
            // Pos 2-10 (0-based: 1..9): RUT_BENEFICIARIO
            dto.RutBeneficiario = FixedWidthHelper.LeerNumero(linea, 1, 9);
            // Pos 11 (0-based: 10): DV_BENEFICIARIO
            dto.DvBeneficiario = FixedWidthHelper.LeerTexto(linea, 10, 1);
            // Pos 12-50 (0-based: 11..49): NOMBRE
            dto.NombreBeneficiario = FixedWidthHelper.LeerTexto(linea, 11, 39);
            // Pos 52 (0-based: 51): IND_BANCO
            var indBanco = FixedWidthHelper.Leer(linea, 51, 1);
            // Pos 53-67 (0-based: 52..66): CTA_OT_BANCO
            var ctaOtBanco = FixedWidthHelper.LeerTexto(linea, 52, 15);
            // Pos 79-80 (0-based: 78..79): TIPO_CUENTA
            dto.TipoCuenta = FixedWidthHelper.LeerNumero(linea, 78, 2);
            // Pos 81-83 (0-based: 80..82): COD_BANCO
            dto.CodBanco = FixedWidthHelper.LeerNumero(linea, 80, 3);
            // Pos 84-94 (0-based: 83..93): CTA_ESTADO
            var ctaEstado = FixedWidthHelper.LeerTexto(linea, 83, 11);
            // Pos 95-105 (0-based: 94..104): MONTO
            dto.Monto = FixedWidthHelper.LeerNumero(linea, 94, 11);

            // Determinar cuenta segun banco
            if (indBanco == "=" && ctaOtBanco != "000000000000000")
                dto.NumeroCuenta = ctaOtBanco.TrimStart('0');
            else
                dto.NumeroCuenta = ctaEstado;

            // Validar RUT
            if (!RutHelper.Validar(dto.RutBeneficiario, dto.DvBeneficiario))
            {
                dto.EstadoLinea = "ADVERTENCIA";
                dto.Mensaje = "RUT no pasa validacion modulo 11";
            }
        }
        catch (Exception ex)
        {
            dto.EstadoLinea = "ERROR";
            dto.Mensaje = $"Error parseando detalle: {ex.Message}";
        }

        return dto;
    }

    private static void ParsearCierre(string linea, TemgeArchivoDto dto)
    {
        // Pos 2-14 (0-based: 1..13): MONTO_TOTAL
        dto.MontoTotalCierre = FixedWidthHelper.LeerNumero(linea, 1, 13);
        // Pos 17-21 (0-based: 16..20): TOTAL_REGISTROS
        dto.TotalRegistrosCierre = (int)FixedWidthHelper.LeerNumero(linea, 16, 5);
    }
}

public class TemgeArchivoDto
{
    public string? CodEmpresa { get; set; }
    public string? FechaProceso { get; set; }
    public string? HoraProceso { get; set; }
    public long MontoTotalCierre { get; set; }
    public int TotalRegistrosCierre { get; set; }
    public List<PreviewLineaDto> Lineas { get; set; } = new();
    public bool IntegridadOk => MontoTotalCierre == Lineas.Sum(l => l.Monto ?? 0)
                                && TotalRegistrosCierre == Lineas.Count;
}
