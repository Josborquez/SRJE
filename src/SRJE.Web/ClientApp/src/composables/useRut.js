/**
 * Validacion de RUT chileno (modulo 11).
 * Port JS del RutHelper.cs del backend (DRY: misma logica).
 */

const FACTORES = [2, 3, 4, 5, 6, 7]

export function calcularDv(rut) {
  const numero = String(rut)
  let suma = 0
  for (let i = numero.length - 1, j = 0; i >= 0; i--, j++) {
    suma += parseInt(numero[i]) * FACTORES[j % FACTORES.length]
  }
  const resto = 11 - (suma % 11)
  if (resto === 11) return '0'
  if (resto === 10) return 'K'
  return String(resto)
}

export function validarRut(rut, dv) {
  if (!rut || rut <= 0 || !dv) return false
  return calcularDv(rut) === dv.toUpperCase().trim()
}

export function formatearRut(rut, dv) {
  const formatted = Number(rut).toLocaleString('es-CL')
  return `${formatted}-${dv}`
}

export function parsearRut(rutFormateado) {
  if (!rutFormateado) return null
  const limpio = rutFormateado.replace(/\./g, '').replace(/-/g, '').trim().toUpperCase()
  if (limpio.length < 2) return null
  const dv = limpio.slice(-1)
  const rut = parseInt(limpio.slice(0, -1))
  if (isNaN(rut)) return null
  return { rut, dv }
}

export function useRut() {
  return { calcularDv, validarRut, formatearRut, parsearRut }
}
