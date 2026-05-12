using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SRJE.Web.Helpers;
using SRJE.Web.Infrastructure.Data;
using SRJE.Web.Models.ViewModels;
using SRJE.Web.Parsers;

namespace SRJE.Web.Services;

public interface IAuditoriaBeneficiariosService
{
    Task<AuditoriaComparacionDto> CompararAsync(Stream stream);
    Task<byte[]> ExportarExcelAsync(Stream stream);
    Task<byte[]> ExportarCsvAsync(Stream stream);
}

public class AuditoriaBeneficiariosService : IAuditoriaBeneficiariosService
{
    private readonly SrjeDbContext _db;

    public AuditoriaBeneficiariosService(SrjeDbContext db)
    {
        _db = db;
    }

    public async Task<AuditoriaComparacionDto> CompararAsync(Stream stream)
    {
        // Parsear archivo TXT de remuneraciones
        var lineas = RemuneracionesParser.Parsear(stream);

        // Deduplicar por RUT beneficiario (puede haber multiples retenciones por beneficiario)
        var archivoDict = new Dictionary<long, PreviewLineaDto>();
        foreach (var linea in lineas)
        {
            if (linea.EstadoLinea == "ERROR") continue;
            archivoDict.TryAdd(linea.RutBeneficiario, linea);
        }

        // Cargar todos los beneficiarios activos de la BD
        var beneficiariosDb = await _db.Beneficiarios.AsNoTracking()
            .Where(b => b.Estado == "A")
            .ToListAsync();
        var dbDict = beneficiariosDb.ToDictionary(b => b.RutBeneficiario);

        var resultado = new AuditoriaComparacionDto
        {
            TotalArchivo = archivoDict.Count,
            TotalSistema = dbDict.Count
        };

        // Solo en archivo (no existe en BD)
        foreach (var (rut, linea) in archivoDict)
        {
            if (!dbDict.ContainsKey(rut))
            {
                resultado.SoloEnArchivo.Add(new AuditoriaItemDto
                {
                    RutBeneficiario = linea.RutBeneficiario,
                    DvBeneficiario = linea.DvBeneficiario,
                    RutFormateado = RutHelper.Formatear(linea.RutBeneficiario, linea.DvBeneficiario),
                    NombreBeneficiario = linea.NombreBeneficiario,
                    RutFuncionarioFormateado = linea.RutFuncionario.HasValue && linea.DvFuncionario != null
                        ? RutHelper.Formatear(linea.RutFuncionario.Value, linea.DvFuncionario)
                        : null
                });
            }
        }

        // Solo en sistema (no existe en archivo)
        foreach (var (rut, benef) in dbDict)
        {
            if (!archivoDict.ContainsKey(rut))
            {
                resultado.SoloEnSistema.Add(new AuditoriaItemDto
                {
                    RutBeneficiario = benef.RutBeneficiario,
                    DvBeneficiario = benef.DvBeneficiario,
                    RutFormateado = RutHelper.Formatear(benef.RutBeneficiario, benef.DvBeneficiario),
                    NombreBeneficiario = benef.NombreBeneficiario,
                    Estado = benef.Estado
                });
            }
        }

        // En ambos: buscar diferencias
        int coincidentes = 0;
        foreach (var (rut, linea) in archivoDict)
        {
            if (!dbDict.TryGetValue(rut, out var benef)) continue;

            var diffs = new List<CampoDiferencia>();

            var nombreArchivo = linea.NombreBeneficiario.Trim().ToUpper();
            var nombreSistema = benef.NombreBeneficiario.Trim().ToUpper();
            if (nombreArchivo != nombreSistema)
            {
                diffs.Add(new CampoDiferencia
                {
                    Campo = "Nombre",
                    ValorArchivo = linea.NombreBeneficiario,
                    ValorSistema = benef.NombreBeneficiario
                });
            }

            if (diffs.Count > 0)
            {
                resultado.ConDiferencias.Add(new AuditoriaDiferenciaDto
                {
                    RutBeneficiario = linea.RutBeneficiario,
                    DvBeneficiario = linea.DvBeneficiario,
                    RutFormateado = RutHelper.Formatear(linea.RutBeneficiario, linea.DvBeneficiario),
                    NombreArchivo = linea.NombreBeneficiario,
                    NombreSistema = benef.NombreBeneficiario,
                    Diferencias = diffs
                });
            }
            else
            {
                coincidentes++;
            }
        }

        resultado.TotalCoincidentes = coincidentes;

        // Ordenar por RUT
        resultado.SoloEnArchivo = resultado.SoloEnArchivo.OrderBy(x => x.RutBeneficiario).ToList();
        resultado.SoloEnSistema = resultado.SoloEnSistema.OrderBy(x => x.RutBeneficiario).ToList();
        resultado.ConDiferencias = resultado.ConDiferencias.OrderBy(x => x.RutBeneficiario).ToList();

        return resultado;
    }

    public async Task<byte[]> ExportarExcelAsync(Stream stream)
    {
        var datos = await CompararAsync(stream);

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using var package = new ExcelPackage();

        // Hoja 1: Solo en Archivo
        var ws1 = package.Workbook.Worksheets.Add("Solo en Archivo");
        WriteItemHeaders(ws1);
        for (int i = 0; i < datos.SoloEnArchivo.Count; i++)
            WriteItemRow(ws1, i + 2, datos.SoloEnArchivo[i]);
        ws1.Cells.AutoFitColumns();

        // Hoja 2: Solo en Sistema
        var ws2 = package.Workbook.Worksheets.Add("Solo en Sistema");
        WriteItemHeaders(ws2);
        for (int i = 0; i < datos.SoloEnSistema.Count; i++)
            WriteItemRow(ws2, i + 2, datos.SoloEnSistema[i]);
        ws2.Cells.AutoFitColumns();

        // Hoja 3: Con Diferencias
        var ws3 = package.Workbook.Worksheets.Add("Con Diferencias");
        var diffHeaders = new[] { "RUT", "Campo", "Valor Archivo", "Valor Sistema" };
        for (int c = 0; c < diffHeaders.Length; c++)
        {
            ws3.Cells[1, c + 1].Value = diffHeaders[c];
            ws3.Cells[1, c + 1].Style.Font.Bold = true;
            ws3.Cells[1, c + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws3.Cells[1, c + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(219, 234, 254));
        }
        int row3 = 2;
        foreach (var dif in datos.ConDiferencias)
        {
            foreach (var campo in dif.Diferencias)
            {
                ws3.Cells[row3, 1].Value = dif.RutFormateado;
                ws3.Cells[row3, 2].Value = campo.Campo;
                ws3.Cells[row3, 3].Value = campo.ValorArchivo;
                ws3.Cells[row3, 4].Value = campo.ValorSistema;
                row3++;
            }
        }
        ws3.Cells.AutoFitColumns();

        // Hoja 4: Resumen
        var ws4 = package.Workbook.Worksheets.Add("Resumen");
        ws4.Cells[1, 1].Value = "Concepto";
        ws4.Cells[1, 2].Value = "Cantidad";
        ws4.Cells[1, 1].Style.Font.Bold = true;
        ws4.Cells[1, 2].Style.Font.Bold = true;
        ws4.Cells[2, 1].Value = "Total en Archivo (RUTs unicos)";
        ws4.Cells[2, 2].Value = datos.TotalArchivo;
        ws4.Cells[3, 1].Value = "Total en Sistema (Activos)";
        ws4.Cells[3, 2].Value = datos.TotalSistema;
        ws4.Cells[4, 1].Value = "Solo en Archivo";
        ws4.Cells[4, 2].Value = datos.SoloEnArchivo.Count;
        ws4.Cells[5, 1].Value = "Solo en Sistema";
        ws4.Cells[5, 2].Value = datos.SoloEnSistema.Count;
        ws4.Cells[6, 1].Value = "Con Diferencias";
        ws4.Cells[6, 2].Value = datos.ConDiferencias.Count;
        ws4.Cells[7, 1].Value = "Coincidentes (sin diferencias)";
        ws4.Cells[7, 2].Value = datos.TotalCoincidentes;
        ws4.Cells.AutoFitColumns();

        return package.GetAsByteArray();
    }

    public async Task<byte[]> ExportarCsvAsync(Stream stream)
    {
        var datos = await CompararAsync(stream);
        var sb = new StringBuilder();

        // BOM UTF-8 para que Excel abra con acentos correctos
        sb.AppendLine("sep=;");

        // Seccion: Resumen
        sb.AppendLine("=== RESUMEN ===");
        sb.AppendLine($"Total en Archivo (RUTs unicos);{datos.TotalArchivo}");
        sb.AppendLine($"Total en Sistema (Activos);{datos.TotalSistema}");
        sb.AppendLine($"Solo en Archivo;{datos.SoloEnArchivo.Count}");
        sb.AppendLine($"Solo en Sistema;{datos.SoloEnSistema.Count}");
        sb.AppendLine($"Con Diferencias;{datos.ConDiferencias.Count}");
        sb.AppendLine($"Coincidentes;{datos.TotalCoincidentes}");
        sb.AppendLine();

        // Seccion: Solo en Archivo
        sb.AppendLine("=== SOLO EN ARCHIVO ===");
        sb.AppendLine("RUT;Nombre;RUT Funcionario");
        foreach (var item in datos.SoloEnArchivo)
            sb.AppendLine($"{item.RutFormateado};{item.NombreBeneficiario};{item.RutFuncionarioFormateado ?? "-"}");
        sb.AppendLine();

        // Seccion: Solo en Sistema
        sb.AppendLine("=== SOLO EN SISTEMA ===");
        sb.AppendLine("RUT;Nombre;RUT Funcionario");
        foreach (var item in datos.SoloEnSistema)
            sb.AppendLine($"{item.RutFormateado};{item.NombreBeneficiario};{item.RutFuncionarioFormateado ?? "-"}");
        sb.AppendLine();

        // Seccion: Con Diferencias
        sb.AppendLine("=== CON DIFERENCIAS ===");
        sb.AppendLine("RUT;Campo;Valor Archivo;Valor Sistema");
        foreach (var dif in datos.ConDiferencias)
        {
            foreach (var campo in dif.Diferencias)
                sb.AppendLine($"{dif.RutFormateado};{campo.Campo};{campo.ValorArchivo};{campo.ValorSistema}");
        }

        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    private static void WriteItemHeaders(ExcelWorksheet ws)
    {
        var headers = new[] { "RUT", "Nombre", "RUT Funcionario" };
        for (int c = 0; c < headers.Length; c++)
        {
            ws.Cells[1, c + 1].Value = headers[c];
            ws.Cells[1, c + 1].Style.Font.Bold = true;
            ws.Cells[1, c + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[1, c + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(219, 234, 254));
        }
    }

    private static void WriteItemRow(ExcelWorksheet ws, int row, AuditoriaItemDto item)
    {
        ws.Cells[row, 1].Value = item.RutFormateado;
        ws.Cells[row, 2].Value = item.NombreBeneficiario;
        ws.Cells[row, 3].Value = item.RutFuncionarioFormateado ?? "-";
    }
}
