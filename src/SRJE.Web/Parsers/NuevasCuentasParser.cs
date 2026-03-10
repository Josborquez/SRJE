using OfficeOpenXml;
using SRJE.Web.Helpers;
using SRJE.Web.Models.ViewModels;

namespace SRJE.Web.Parsers;

/// <summary>
/// Parser para archivo Excel NUEVAS_CUENTAS_A_BASE_DATO.xlsx.
/// Layout: A=RUT, B=DV, C=RUT_Titular, D=DV_Titular, E=Nombre,
///         F=Cta_OtBanco, G=Tipo_Cuenta, H=Cod_Banco, I=Cta_Estado, J=Observaciones
/// </summary>
public static class NuevasCuentasParser
{
    static NuevasCuentasParser()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public static List<PreviewLineaDto> Parsear(Stream stream)
    {
        var resultado = new List<PreviewLineaDto>();

        using var package = new ExcelPackage(stream);
        var ws = package.Workbook.Worksheets[0];

        // Empieza en fila 2 (fila 1 = cabecera)
        for (int row = 2; row <= ws.Dimension?.End.Row; row++)
        {
            var rutStr = ws.Cells[row, 1].Text?.Trim();
            if (string.IsNullOrEmpty(rutStr))
                continue;

            var dto = new PreviewLineaDto { NumeroLinea = row - 1 };

            try
            {
                dto.RutBeneficiario = long.Parse(rutStr);
                dto.DvBeneficiario = ws.Cells[row, 2].Text?.Trim().ToUpper() ?? "";
                dto.RutFuncionario = long.TryParse(ws.Cells[row, 3].Text?.Trim(), out var rutTit)
                    ? rutTit : null;
                dto.DvFuncionario = ws.Cells[row, 4].Text?.Trim().ToUpper();
                dto.NombreBeneficiario = ws.Cells[row, 5].Text?.Trim() ?? "";

                // Truncar nombre a 39 chars para TEMGE
                if (dto.NombreBeneficiario.Length > 39)
                    dto.NombreBeneficiario = dto.NombreBeneficiario[..39];

                var ctaOtBanco = ws.Cells[row, 6].Text?.Trim() ?? "0";
                var tipoCuenta = long.TryParse(ws.Cells[row, 7].Text?.Trim(), out var tc) ? tc : 0;
                var codBanco = long.TryParse(ws.Cells[row, 8].Text?.Trim(), out var cb) ? cb : 0;
                var ctaEstado = ws.Cells[row, 9].Text?.Trim() ?? "";

                dto.TipoCuenta = tipoCuenta;
                dto.CodBanco = codBanco;

                // BancoEstado (cod 12): usar CTA_ESTADO (hasta 15 chars)
                if (codBanco == 12)
                {
                    dto.NumeroCuenta = ctaEstado;
                }
                else
                {
                    dto.NumeroCuenta = ctaOtBanco;
                }

                // Validar RUT beneficiario
                if (!RutHelper.Validar(dto.RutBeneficiario, dto.DvBeneficiario))
                {
                    dto.EstadoLinea = "ERROR";
                    dto.Mensaje = "RUT beneficiario invalido (modulo 11)";
                    continue;
                }

                // Validar cuenta BancoEstado: hasta 15 digitos numericos
                if (codBanco == 12 && (string.IsNullOrEmpty(dto.NumeroCuenta) ||
                    dto.NumeroCuenta.Length > 15 || !dto.NumeroCuenta.All(char.IsDigit)))
                {
                    dto.EstadoLinea = "ADVERTENCIA";
                    dto.Mensaje = $"Cuenta BancoEstado tiene {ctaEstado.Length} digitos (maximo 15)";
                }
            }
            catch (Exception ex)
            {
                dto.EstadoLinea = "ERROR";
                dto.Mensaje = $"Error en fila {row}: {ex.Message}";
            }

            resultado.Add(dto);
        }

        return resultado;
    }
}
