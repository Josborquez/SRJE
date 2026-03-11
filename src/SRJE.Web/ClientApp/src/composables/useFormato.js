/**
 * Formatea un numero de cuenta bancaria para que nunca muestre notacion cientifica.
 * Ej: 2.706020573E+10 -> "27060205730"
 *     3.446134866E+10 -> "34461348660"
 *     "41762633599"   -> "41762633599"
 */
export function formatCuenta(val) {
  if (val == null || val === '') return '-'
  const s = String(val)
  // Si ya es un string limpio de solo digitos, retornar tal cual
  if (/^\d+$/.test(s)) return s
  // Normalizar separador decimal: coma -> punto (locale ES usa coma)
  const normalized = s.replace(/,/g, '.')
  // Convertir notacion cientifica o decimal a entero string
  try {
    const num = Number(normalized)
    if (isNaN(num)) return s
    return num.toFixed(0)
  } catch {
    return s
  }
}
