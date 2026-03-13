using SRJE.Web.Helpers;
using SRJE.Web.Models.ViewModels;

namespace SRJE.Web.Parsers;

/// <summary>
/// Parser para archivo ENVIO_REMUNERACIONES (ancho fijo 126 chars, encoding Latin-1).
/// Layout validado con archivo real.
/// </summary>
public static class RemuneracionesParser
{
    public const int LargoLinea = 126;

    public static List<PreviewLineaDto> Parsear(Stream stream)
    {
        var resultado = new List<PreviewLineaDto>();
        using var reader = new StreamReader(stream, System.Text.Encoding.Latin1);

        int numLinea = 0;
        string? linea;
        while ((linea = reader.ReadLine()) != null)
        {
            numLinea++;
            if (string.IsNullOrWhiteSpace(linea))
                continue;

            var dto = new PreviewLineaDto { NumeroLinea = numLinea };

            try
            {
                // Pos 1-9: RUT Funcionario/Titular (0-based: 0..8)
                dto.RutFuncionario = FixedWidthHelper.LeerNumero(linea, 0, 9);
                // Pos 10: DV Funcionario (0-based: 9)
                dto.DvFuncionario = FixedWidthHelper.LeerTexto(linea, 9, 1);
                // Pos 11-19: RUT Beneficiario (0-based: 10..18)
                dto.RutBeneficiario = FixedWidthHelper.LeerNumero(linea, 10, 9);
                // Pos 20: DV Beneficiario (0-based: 19)
                dto.DvBeneficiario = FixedWidthHelper.LeerTexto(linea, 19, 1);
                // Pos 21-40: Apellido Paterno (no se almacena directo, se usa para nombre completo)
                var apPaterno = FixedWidthHelper.LeerTexto(linea, 20, 20);
                // Pos 41-60: Apellido Materno
                var apMaterno = FixedWidthHelper.LeerTexto(linea, 40, 20);
                // Pos 61-90: Nombres
                var nombres = FixedWidthHelper.LeerTexto(linea, 60, 30);
                dto.NombreBeneficiario = $"{apPaterno} {apMaterno} {nombres}".Trim();
                // Truncar a 39 chars para TEMGE
                if (dto.NombreBeneficiario.Length > 39)
                    dto.NombreBeneficiario = dto.NombreBeneficiario[..39];
                // Pos 91-98: Monto retencion judicial (8 digitos, sin decimales)
                dto.Monto = FixedWidthHelper.LeerNumero(linea, 90, 8);
                // Pos 99-109: Codigo Retencion
                dto.CodRetencion = FixedWidthHelper.LeerTexto(linea, 98, 11);
                // Pos 110-126: Tipo Pago
                dto.TipoPago = FixedWidthHelper.LeerTexto(linea, 109, 17);

                // Validar RUT beneficiario
                if (!RutHelper.Validar(dto.RutBeneficiario, dto.DvBeneficiario))
                {
                    dto.EstadoLinea = "ADVERTENCIA";
                    dto.Mensaje = "RUT beneficiario no pasa validacion modulo 11";
                }

                // Validar RUT funcionario
                if (dto.RutFuncionario.HasValue &&
                    !RutHelper.Validar(dto.RutFuncionario.Value, dto.DvFuncionario ?? ""))
                {
                    dto.EstadoLinea = "ADVERTENCIA";
                    dto.Mensaje = (dto.Mensaje ?? "") + " | RUT funcionario no pasa validacion";
                }
            }
            catch (Exception ex)
            {
                dto.EstadoLinea = "ERROR";
                dto.Mensaje = $"Error parseando linea: {ex.Message}";
            }

            resultado.Add(dto);
        }

        return resultado;
    }
}
