"""
Genera PDF de analisis comparativo entre TEMGE_SRJE2.txt y PAGO RETJUD_MAR 2026(1).txt
"""
import os
from reportlab.lib.pagesizes import letter, landscape
from reportlab.lib import colors
from reportlab.lib.units import inch, mm
from reportlab.platypus import SimpleDocTemplate, Table, TableStyle, Paragraph, Spacer, PageBreak
from reportlab.lib.styles import getSampleStyleSheet, ParagraphStyle

BASE = os.path.dirname(os.path.abspath(__file__))
PAGO_FILE = os.path.join(BASE, "PAGO RETJUD_MAR 2026(1).txt")
SRJE2_FILE = os.path.join(BASE, "TEMGE_SRJE2.txt")
RENUM_FILE = os.path.join(BASE, "1. ENVIO REMUNERACIONES.txt")
OUTPUT_PDF = os.path.join(BASE, "Analisis_TEMGE_vs_PAGO_Marzo2026.pdf")


def parse_temge_detail(line):
    """Parse a TEMGE detail line (type 2)"""
    if not line.startswith("2"):
        return None
    rut = line[1:10].lstrip("0") or "0"
    dv = line[10]
    nombre = line[11:50].rstrip()
    banco = line[80:83]
    tipo_cta = line[78:80]
    cta = line[83:94].strip()
    monto = int(line[94:105])
    return {
        "rut": rut, "dv": dv, "nombre": nombre,
        "banco": banco, "tipo_cta": tipo_cta, "cta": cta, "monto": monto,
        "rut_padded": line[1:10]
    }


def parse_renum_line(line):
    """Parse a remuneraciones line"""
    if len(line) < 100:
        return None
    try:
        rut_func = int(line[0:9].strip())
        rut_benef = int(line[10:19].strip())
        monto = int(line[90:98].strip())
        cod = line[98:109].strip()
        tipo_pago = line[109:126].strip()
        return {
            "rut_func": rut_func, "rut_benef": rut_benef,
            "monto": monto, "cod": cod, "tipo_pago": tipo_pago
        }
    except:
        return None


def load_file_lines(filepath, encoding="latin-1"):
    with open(filepath, encoding=encoding, errors="replace") as f:
        return f.readlines()


def fmt_money(n):
    return f"${n:,.0f}".replace(",", ".")


def fmt_rut(rut_str, dv):
    r = rut_str.lstrip("0") or "0"
    # Format with dots
    r_rev = r[::-1]
    parts = [r_rev[i:i+3] for i in range(0, len(r_rev), 3)]
    formatted = ".".join(parts)[::-1]
    return f"{formatted}-{dv}"


def main():
    # --- Load PAGO file ---
    pago_lines = load_file_lines(PAGO_FILE)
    pago_records = [parse_temge_detail(l.rstrip()) for l in pago_lines if l.startswith("2")]
    pago_records = [r for r in pago_records if r]

    # --- Load SRJE2 file ---
    srje2_lines = load_file_lines(SRJE2_FILE)
    srje2_records = [parse_temge_detail(l.rstrip()) for l in srje2_lines if l.startswith("2")]
    srje2_records = [r for r in srje2_records if r]

    # --- Load remuneraciones ---
    renum_lines = load_file_lines(RENUM_FILE)
    renum_records = [parse_renum_line(l.rstrip()) for l in renum_lines]
    renum_records = [r for r in renum_records if r]

    # --- Find duplicates in PAGO ---
    from collections import Counter, defaultdict
    pago_rut_count = Counter(r["rut"] for r in pago_records)
    dup_ruts = {rut for rut, cnt in pago_rut_count.items() if cnt > 1}

    # Group PAGO records by RUT
    pago_by_rut = defaultdict(list)
    for r in pago_records:
        pago_by_rut[r["rut"]].append(r)

    # Group SRJE2 records by RUT
    srje2_by_rut = defaultdict(list)
    for r in srje2_records:
        srje2_by_rut[r["rut"]].append(r)

    # Group remuneraciones by beneficiario RUT
    renum_by_benef = defaultdict(list)
    for r in renum_records:
        renum_by_benef[str(r["rut_benef"])].append(r)

    # --- Classify duplicates ---
    dup_mismo_func = []  # Same funcionario, multiple retentions
    dup_dif_func = []    # Different funcionario
    dup_periodo_ant = [] # Only 1 in remuneraciones (other from prior period)

    for rut in sorted(dup_ruts, key=lambda x: int(x.rstrip("Kk"))):
        pago_entries = pago_by_rut[rut]
        renum_entries = renum_by_benef.get(rut, [])

        if len(renum_entries) == 0:
            # Not in current remuneraciones at all - both from prior data
            dup_periodo_ant.append(rut)
        elif len(renum_entries) == 1:
            dup_periodo_ant.append(rut)
        else:
            funcs = set(r["rut_func"] for r in renum_entries)
            if len(funcs) == 1:
                dup_mismo_func.append(rut)
            else:
                dup_dif_func.append(rut)

    # --- Generate PDF ---
    doc = SimpleDocTemplate(OUTPUT_PDF, pagesize=landscape(letter),
                           topMargin=0.5*inch, bottomMargin=0.5*inch,
                           leftMargin=0.5*inch, rightMargin=0.5*inch)

    styles = getSampleStyleSheet()
    title_style = ParagraphStyle("CustomTitle", parent=styles["Title"], fontSize=16, spaceAfter=6)
    subtitle_style = ParagraphStyle("Subtitle", parent=styles["Heading2"], fontSize=12, spaceAfter=4)
    normal = ParagraphStyle("CustomNormal", parent=styles["Normal"], fontSize=8)
    small = ParagraphStyle("Small", parent=styles["Normal"], fontSize=7)
    header_style = ParagraphStyle("Header", parent=styles["Normal"], fontSize=7,
                                  textColor=colors.white, alignment=1)

    elements = []

    # --- PAGE 1: Summary ---
    elements.append(Paragraph("Analisis Comparativo: TEMGE_SRJE2 vs PAGO RETJUD", title_style))
    elements.append(Paragraph("Marzo 2026 - Sistema SRJE", styles["Heading3"]))
    elements.append(Spacer(1, 12))

    # Summary table
    summary_data = [
        ["Concepto", "TEMGE_SRJE2 (Sistema Nuevo)", "PAGO RETJUD (Esperado)", "Diferencia"],
        ["Total Registros", f"{len(srje2_records):,}".replace(",","."), f"{len(pago_records):,}".replace(",","."),
         f"{len(pago_records) - len(srje2_records):,}".replace(",",".")],
        ["Monto Total", fmt_money(sum(r["monto"] for r in srje2_records)),
         fmt_money(sum(r["monto"] for r in pago_records)),
         fmt_money(sum(r["monto"] for r in pago_records) - sum(r["monto"] for r in srje2_records))],
        ["RUTs Unicos", f"{len(set(r['rut'] for r in srje2_records)):,}".replace(",","."),
         f"{len(set(r['rut'] for r in pago_records)):,}".replace(",","."), "0"],
        ["RUTs Duplicados", str(len([r for r in Counter(r2["rut"] for r2 in srje2_records).values() if r > 1])),
         str(len(dup_ruts)),
         str(len(dup_ruts) - len([r for r in Counter(r2["rut"] for r2 in srje2_records).values() if r > 1]))],
    ]
    t = Table(summary_data, colWidths=[160, 180, 180, 120])
    t.setStyle(TableStyle([
        ("BACKGROUND", (0, 0), (-1, 0), colors.HexColor("#2c3e50")),
        ("TEXTCOLOR", (0, 0), (-1, 0), colors.white),
        ("FONTSIZE", (0, 0), (-1, -1), 9),
        ("FONTNAME", (0, 0), (-1, 0), "Helvetica-Bold"),
        ("ALIGN", (1, 1), (-1, -1), "RIGHT"),
        ("GRID", (0, 0), (-1, -1), 0.5, colors.grey),
        ("ROWBACKGROUNDS", (0, 1), (-1, -1), [colors.white, colors.HexColor("#ecf0f1")]),
    ]))
    elements.append(t)
    elements.append(Spacer(1, 16))

    # Classification summary
    elements.append(Paragraph("Clasificacion de las 52 Retenciones Faltantes", subtitle_style))
    class_data = [
        ["Categoria", "Cantidad", "Descripcion"],
        ["Mismo Funcionario", str(len(dup_mismo_func)),
         "2 retenciones del mismo funcionario con distinto monto. El sistema sobreescribia la 1ra."],
        ["Diferente Funcionario", str(len(dup_dif_func)),
         "2 retenciones de funcionarios distintos. Ya generadas correctamente en SRJE2."],
        ["Periodo Anterior", str(len(dup_periodo_ant)),
         "Solo 1 retencion en remuneraciones marzo. La 2da viene de un periodo anterior."],
    ]
    t2 = Table(class_data, colWidths=[130, 70, 440])
    t2.setStyle(TableStyle([
        ("BACKGROUND", (0, 0), (-1, 0), colors.HexColor("#2c3e50")),
        ("TEXTCOLOR", (0, 0), (-1, 0), colors.white),
        ("FONTSIZE", (0, 0), (-1, -1), 9),
        ("FONTNAME", (0, 0), (-1, 0), "Helvetica-Bold"),
        ("ALIGN", (1, 1), (1, -1), "CENTER"),
        ("GRID", (0, 0), (-1, -1), 0.5, colors.grey),
        ("ROWBACKGROUNDS", (0, 1), (-1, -1), [colors.white, colors.HexColor("#ecf0f1")]),
        ("VALIGN", (0, 0), (-1, -1), "TOP"),
    ]))
    elements.append(t2)
    elements.append(Spacer(1, 16))

    # Issue about bank accounts
    elements.append(Paragraph("Cuentas Bancarias Diferentes (2 casos)", subtitle_style))
    elements.append(Paragraph(
        "Los siguientes 2 beneficiarios tienen retenciones que deben pagarse en cuentas bancarias distintas. "
        "En TEMGE_SRJE2 ambas lineas usan la misma cuenta (la ultima asignada al beneficiario). "
        "La correccion requiere asignar la cuenta bancaria a nivel de retencion individual.",
        ParagraphStyle("Note", parent=styles["Normal"], fontSize=8, spaceAfter=6)
    ))

    for rut in dup_dif_func:
        pago_entries = pago_by_rut[rut]
        srje2_entries = srje2_by_rut.get(rut, [])
        rut_fmt = fmt_rut(pago_entries[0]["rut"], pago_entries[0]["dv"])

        elements.append(Paragraph(f"<b>{rut_fmt} - {pago_entries[0]['nombre']}</b>", small))
        bank_data = [["Fuente", "Banco", "Tipo Cta", "Cuenta", "Monto"]]
        for e in pago_entries:
            bank_data.append(["PAGO (esperado)", e["banco"], e["tipo_cta"], e["cta"], fmt_money(e["monto"])])
        for e in srje2_entries:
            bank_data.append(["SRJE2 (actual)", e["banco"], e["tipo_cta"], e["cta"], fmt_money(e["monto"])])
        tb = Table(bank_data, colWidths=[100, 50, 55, 100, 90])
        tb.setStyle(TableStyle([
            ("BACKGROUND", (0, 0), (-1, 0), colors.HexColor("#34495e")),
            ("TEXTCOLOR", (0, 0), (-1, 0), colors.white),
            ("FONTSIZE", (0, 0), (-1, -1), 7),
            ("FONTNAME", (0, 0), (-1, 0), "Helvetica-Bold"),
            ("ALIGN", (4, 1), (4, -1), "RIGHT"),
            ("GRID", (0, 0), (-1, -1), 0.5, colors.grey),
        ]))
        elements.append(tb)
        elements.append(Spacer(1, 8))

    # --- PAGE 2+: Detail tables ---
    elements.append(PageBreak())

    # CATEGORY 1: Mismo funcionario (CRITICAL - these were lost)
    elements.append(Paragraph("Detalle: Retenciones Mismo Funcionario (Sobreescritas)", title_style))
    elements.append(Paragraph(
        f"{len(dup_mismo_func)} beneficiarios con 2 retenciones del mismo funcionario. "
        "La segunda retencion sobreescribia la primera en el sistema, perdiendo un monto.",
        ParagraphStyle("Note", parent=styles["Normal"], fontSize=8, spaceAfter=8, textColor=colors.HexColor("#c0392b"))
    ))

    detail_data = [["N", "RUT Beneficiario", "Nombre", "RUT Func.", "Monto 1\n(PAGO)", "Monto 2\n(PAGO)",
                    "Monto SRJE2", "Monto\nPerdido"]]

    total_perdido = 0
    for i, rut in enumerate(dup_mismo_func, 1):
        pago_entries = pago_by_rut[rut]
        srje2_entries = srje2_by_rut.get(rut, [])
        renum_entries = renum_by_benef.get(rut, [])
        rut_fmt = fmt_rut(pago_entries[0]["rut"], pago_entries[0]["dv"])
        func_rut = str(renum_entries[0]["rut_func"]) if renum_entries else "-"

        montos_pago = sorted([e["monto"] for e in pago_entries])
        monto_srje2 = sum(e["monto"] for e in srje2_entries)
        monto_perdido = sum(montos_pago) - monto_srje2
        total_perdido += monto_perdido

        detail_data.append([
            str(i), rut_fmt, pago_entries[0]["nombre"][:30], func_rut,
            fmt_money(montos_pago[0]), fmt_money(montos_pago[1]),
            fmt_money(monto_srje2), fmt_money(monto_perdido)
        ])

    detail_data.append(["", "", "", "TOTAL", "", "", "", fmt_money(total_perdido)])

    td = Table(detail_data, colWidths=[22, 80, 175, 65, 72, 72, 72, 72])
    td.setStyle(TableStyle([
        ("BACKGROUND", (0, 0), (-1, 0), colors.HexColor("#c0392b")),
        ("TEXTCOLOR", (0, 0), (-1, 0), colors.white),
        ("FONTSIZE", (0, 0), (-1, -1), 7),
        ("FONTNAME", (0, 0), (-1, 0), "Helvetica-Bold"),
        ("FONTNAME", (0, -1), (-1, -1), "Helvetica-Bold"),
        ("ALIGN", (0, 0), (0, -1), "CENTER"),
        ("ALIGN", (4, 1), (-1, -1), "RIGHT"),
        ("GRID", (0, 0), (-1, -1), 0.5, colors.grey),
        ("ROWBACKGROUNDS", (0, 1), (-1, -2), [colors.white, colors.HexColor("#fdf2f2")]),
        ("BACKGROUND", (0, -1), (-1, -1), colors.HexColor("#f8d7da")),
    ]))
    elements.append(td)

    # --- PAGE 3+: Periodo anterior ---
    elements.append(PageBreak())
    elements.append(Paragraph("Detalle: Retenciones de Periodo Anterior", title_style))
    elements.append(Paragraph(
        f"{len(dup_periodo_ant)} beneficiarios con 1 retencion en remuneraciones de marzo y 1 retencion adicional "
        "proveniente de un periodo anterior que sigue activa en el sistema legacy.",
        ParagraphStyle("Note", parent=styles["Normal"], fontSize=8, spaceAfter=8, textColor=colors.HexColor("#2980b9"))
    ))

    prev_data = [["N", "RUT Beneficiario", "Nombre", "Monto 1\n(PAGO)", "Monto 2\n(PAGO)",
                  "En Renum.\nMarzo", "Monto SRJE2"]]

    for i, rut in enumerate(dup_periodo_ant, 1):
        pago_entries = pago_by_rut[rut]
        srje2_entries = srje2_by_rut.get(rut, [])
        renum_entries = renum_by_benef.get(rut, [])
        rut_fmt = fmt_rut(pago_entries[0]["rut"], pago_entries[0]["dv"])
        montos_pago = sorted([e["monto"] for e in pago_entries])
        monto_srje2 = sum(e["monto"] for e in srje2_entries)
        en_renum = len(renum_entries)

        prev_data.append([
            str(i), rut_fmt, pago_entries[0]["nombre"][:32],
            fmt_money(montos_pago[0]), fmt_money(montos_pago[1]),
            f"{en_renum} linea(s)", fmt_money(monto_srje2)
        ])

    tp = Table(prev_data, colWidths=[22, 80, 195, 80, 80, 65, 80])
    tp.setStyle(TableStyle([
        ("BACKGROUND", (0, 0), (-1, 0), colors.HexColor("#2980b9")),
        ("TEXTCOLOR", (0, 0), (-1, 0), colors.white),
        ("FONTSIZE", (0, 0), (-1, -1), 7),
        ("FONTNAME", (0, 0), (-1, 0), "Helvetica-Bold"),
        ("ALIGN", (0, 0), (0, -1), "CENTER"),
        ("ALIGN", (3, 1), (-1, -1), "RIGHT"),
        ("ALIGN", (5, 1), (5, -1), "CENTER"),
        ("GRID", (0, 0), (-1, -1), 0.5, colors.grey),
        ("ROWBACKGROUNDS", (0, 1), (-1, -1), [colors.white, colors.HexColor("#eaf2f8")]),
    ]))
    elements.append(tp)

    # --- PAGE 4: All 54 duplicates complete reference ---
    elements.append(PageBreak())
    elements.append(Paragraph("Referencia Completa: Todos los RUTs Duplicados en PAGO RETJUD", title_style))
    elements.append(Paragraph(
        "Listado de los 54 beneficiarios que aparecen con 2 lineas en el archivo PAGO RETJUD_MAR 2026(1).txt. "
        "Se muestra el detalle de cada linea con su monto y cuenta bancaria.",
        ParagraphStyle("Note", parent=styles["Normal"], fontSize=8, spaceAfter=8)
    ))

    all_data = [["N", "RUT", "Nombre", "Banco", "Cuenta Linea 1", "Monto 1",
                 "Cuenta Linea 2", "Monto 2", "Total", "Categoria"]]

    cat_labels = {
        "MISMO_FUNC": "Mismo Func.",
        "DIF_FUNC": "Dif. Func.",
        "PERIODO_ANT": "Per. Anterior"
    }

    for i, rut in enumerate(sorted(dup_ruts, key=lambda x: int(x.rstrip("Kk"))), 1):
        entries = pago_by_rut[rut]
        rut_fmt = fmt_rut(entries[0]["rut"], entries[0]["dv"])

        if rut in dup_mismo_func:
            cat = "Mismo Func."
        elif rut in dup_dif_func:
            cat = "Dif. Func."
        else:
            cat = "Per. Anterior"

        total = sum(e["monto"] for e in entries)
        e1, e2 = entries[0], entries[1]

        all_data.append([
            str(i), rut_fmt, entries[0]["nombre"][:25],
            e1["banco"],
            e1["cta"], fmt_money(e1["monto"]),
            e2["cta"], fmt_money(e2["monto"]),
            fmt_money(total), cat
        ])

    ta = Table(all_data, colWidths=[20, 72, 145, 32, 72, 62, 72, 62, 62, 65])
    ta.setStyle(TableStyle([
        ("BACKGROUND", (0, 0), (-1, 0), colors.HexColor("#2c3e50")),
        ("TEXTCOLOR", (0, 0), (-1, 0), colors.white),
        ("FONTSIZE", (0, 0), (-1, -1), 6.5),
        ("FONTNAME", (0, 0), (-1, 0), "Helvetica-Bold"),
        ("ALIGN", (0, 0), (0, -1), "CENTER"),
        ("ALIGN", (5, 1), (5, -1), "RIGHT"),
        ("ALIGN", (7, 1), (8, -1), "RIGHT"),
        ("GRID", (0, 0), (-1, -1), 0.5, colors.grey),
        ("ROWBACKGROUNDS", (0, 1), (-1, -1), [colors.white, colors.HexColor("#ecf0f1")]),
    ]))
    elements.append(ta)

    # Build PDF
    doc.build(elements)
    print(f"PDF generado: {OUTPUT_PDF}")
    print(f"  Total paginas estimado: 4+")
    print(f"  Registros PAGO: {len(pago_records)}")
    print(f"  Registros SRJE2: {len(srje2_records)}")
    print(f"  Duplicados analizados: {len(dup_ruts)}")


if __name__ == "__main__":
    main()
