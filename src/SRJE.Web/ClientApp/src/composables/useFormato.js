/**
 * Formatea un numero de cuenta bancaria para que nunca muestre notacion cientifica.
 * Ej: 2.706020573E+10 -> "27060205730"
 *     3.446134866E+10 -> "34461348660"
 *     "41762633599"   -> "41762633599"
 */
export function formatCuenta(val) {
  if (val == null || val === '') return '-'
  const s = String(val)
  // Si ya es un string limpio sin notacion cientifica, retornar tal cual
  if (!/e\+/i.test(s) && !/^\d+[.,]\d+$/.test(s)) return s
  // Convertir notacion cientifica o decimal a entero string sin separadores
  try {
    const num = Number(val)
    if (isNaN(num)) return s
    return num.toLocaleString('en-US', { useGrouping: false, maximumFractionDigits: 0 })
  } catch {
    return s
  }
}
