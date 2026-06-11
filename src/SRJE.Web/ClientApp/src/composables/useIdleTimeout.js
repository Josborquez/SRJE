import { ref, onUnmounted } from 'vue'

const EVENTOS = ['mousemove', 'mousedown', 'keydown', 'scroll', 'touchstart']

/**
 * Detecta inactividad del usuario: aviso a los `avisoMin` minutos,
 * timeout (logout) a los `logoutMin` minutos sin actividad.
 */
export function useIdleTimeout({ avisoMin = 25, logoutMin = 30, onTimeout } = {}) {
  const avisoVisible = ref(false)
  let avisoTimer = null
  let logoutTimer = null
  let activo = false
  let ultimoReset = 0

  function limpiarTimers() {
    clearTimeout(avisoTimer)
    clearTimeout(logoutTimer)
  }

  function programar() {
    limpiarTimers()
    avisoTimer = setTimeout(() => { avisoVisible.value = true }, avisoMin * 60000)
    logoutTimer = setTimeout(() => {
      stop()
      onTimeout?.()
    }, logoutMin * 60000)
  }

  function onActividad() {
    // Con el aviso abierto, solo el boton "Continuar" renueva la sesion
    if (avisoVisible.value) return
    // Throttle: reprogramar a lo mas una vez por segundo
    const ahora = Date.now()
    if (ahora - ultimoReset < 1000) return
    ultimoReset = ahora
    programar()
  }

  function start() {
    if (activo) return
    activo = true
    EVENTOS.forEach(e => window.addEventListener(e, onActividad, { passive: true }))
    programar()
  }

  function stop() {
    if (!activo) return
    activo = false
    avisoVisible.value = false
    limpiarTimers()
    EVENTOS.forEach(e => window.removeEventListener(e, onActividad))
  }

  function continuar() {
    avisoVisible.value = false
    programar()
  }

  onUnmounted(stop)

  return { avisoVisible, start, stop, continuar }
}
