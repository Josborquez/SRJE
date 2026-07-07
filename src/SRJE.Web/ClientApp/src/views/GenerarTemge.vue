<template>
  <div class="generar-temge">
    <div class="page-header">
      <h1><FileDown :size="24" /> Generar Archivo TEMGE</h1>
      <p>Genera el archivo de pago en formato de ancho fijo compatible con el banco.</p>
    </div>

    <div class="generar-card">
      <div class="generar-card-icon">
        <FileDown :size="40" />
      </div>
      <div class="generar-card-content">
        <h3>Archivo de Pago TEMGE</h3>
        <p>Se incluiran los beneficiarios activos con cuenta bancaria y retenciones vigentes
          del periodo seleccionado. El archivo se descargara automaticamente en formato de texto.</p>
      </div>
    </div>

    <div class="periodo-input">
      <label><Calendar :size="16" /> Periodo Proceso (AAAAMM):</label>
      <input v-model="periodo" placeholder="Ej: 202603" maxlength="6" />
      <span v-if="periodo && !periodoValido" class="periodo-error">
        Formato invalido. Use AAAAMM (ej: 202603)
      </span>
    </div>

    <AlertMessage v-if="alertMsg" :message="alertMsg" :type="alertType" @close="alertMsg = ''" />

    <div class="actions">
      <button @click="showConfirm = true" class="btn btn-primary" :disabled="loading || !periodoValido">
        <Download :size="16" />
        {{ loading ? 'Generando archivo...' : 'Generar Archivo TEMGE' }}
      </button>
    </div>

    <div v-if="error" class="error"><CircleAlert :size="16" /> {{ error }}</div>

    <div v-if="generado" class="resultado">
      <h3><CheckCircle :size="18" /> Archivo generado exitosamente</h3>
      <p>El archivo se ha descargado automaticamente a su computador.</p>
      <p>Registros incluidos: <strong>{{ cantidadRegistros }}</strong> |
        Monto total: <strong>${{ montoTotal.toLocaleString('es-CL') }}</strong></p>
    </div>

    <div v-if="excluidos.length" class="excluidos">
      <h3><TriangleAlert :size="18" /> {{ excluidos.length }} retenciones excluidas del archivo
        (${{ montoExcluido.toLocaleString('es-CL') }})</h3>
      <table>
        <thead>
          <tr>
            <th>RUT Beneficiario</th>
            <th>Nombre</th>
            <th>Monto</th>
            <th>Motivo</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="(e, i) in excluidos" :key="i">
            <td>{{ e.rutBeneficiario }}-{{ e.dvBeneficiario }}</td>
            <td>{{ e.nombreBeneficiario || '-' }}</td>
            <td class="monto">${{ e.monto.toLocaleString('es-CL') }}</td>
            <td>{{ e.motivo }}</td>
          </tr>
        </tbody>
      </table>
    </div>

    <ConfirmModal
      v-model="showConfirm"
      title="Generar Archivo TEMGE"
      :message="`Se generara el archivo TEMGE para el periodo ${periodo} con los beneficiarios activos que tengan cuenta bancaria y retenciones vigentes. ¿Desea continuar?`"
      confirmText="Si, generar"
      @confirm="generar"
    />
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { archivosApi } from '../api/index.js'
import AlertMessage from '../components/AlertMessage.vue'
import ConfirmModal from '../components/ConfirmModal.vue'
import { FileDown, Download, CircleAlert, CheckCircle, Calendar, TriangleAlert } from 'lucide-vue-next'

const loading = ref(false)
const error = ref(null)
const generado = ref(false)
const alertMsg = ref('')
const alertType = ref('info')
const showConfirm = ref(false)
const periodo = ref('')
const excluidos = ref([])
const cantidadRegistros = ref(0)
const montoTotal = ref(0)

const montoExcluido = computed(() =>
  excluidos.value.reduce((s, e) => s + (e.monto || 0), 0)
)

const periodoValido = computed(() => {
  if (!periodo.value) return false
  return /^\d{6}$/.test(periodo.value)
})

async function generar() {
  loading.value = true
  error.value = null
  generado.value = false
  alertMsg.value = ''
  excluidos.value = []
  try {
    const { data } = await archivosApi.generarTemge(periodo.value)
    if (!data?.archivo) {
      throw new Error('Respuesta inesperada del servidor. Verifique que el backend este actualizado y reintente.')
    }
    const bytes = Uint8Array.from(atob(data.archivo), c => c.charCodeAt(0))
    const url = window.URL.createObjectURL(new Blob([bytes], { type: 'text/plain' }))
    const link = document.createElement('a')
    link.href = url
    link.setAttribute('download', data.nombreArchivo || `TEMGE_${periodo.value}.txt`)
    document.body.appendChild(link)
    link.click()
    link.remove()
    window.URL.revokeObjectURL(url)
    generado.value = true
    excluidos.value = data.excluidos || []
    cantidadRegistros.value = data.cantidadRegistros || 0
    montoTotal.value = data.montoTotal || 0
    if (excluidos.value.length) {
      alertType.value = 'warning'
      alertMsg.value = `Archivo TEMGE generado, pero ${excluidos.value.length} retenciones quedaron fuera por $${montoExcluido.value.toLocaleString('es-CL')}. Revise el detalle mas abajo.`
    } else {
      alertType.value = 'success'
      alertMsg.value = `Archivo TEMGE generado y descargado exitosamente para el periodo ${periodo.value}.`
    }
  } catch (e) {
    error.value = e.response?.data?.error || e.message
    alertType.value = 'error'
    alertMsg.value = 'Error al generar el archivo TEMGE. Intente nuevamente.'
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.generar-card {
  display: flex;
  align-items: center;
  gap: 1.5rem;
  background: var(--bg-card);
  padding: 1.5rem 2rem;
  border-radius: var(--border-radius-lg);
  border: 1px solid var(--border-color);
  margin-bottom: 1.5rem;
}
.generar-card-icon {
  width: 72px;
  height: 72px;
  background: var(--color-primary-light);
  color: var(--color-primary);
  border-radius: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}
.generar-card-content h3 {
  font-size: 1.05rem;
  margin-bottom: 0.3rem;
}
.generar-card-content p {
  color: var(--text-secondary);
  font-size: 0.9rem;
  line-height: 1.5;
}
.periodo-input {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  margin-bottom: 1.5rem;
  flex-wrap: wrap;
}
.periodo-input label {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  font-weight: 500;
  white-space: nowrap;
}
.periodo-input input {
  padding: 0.5rem 0.75rem;
  border: 1px solid var(--border-color);
  border-radius: var(--border-radius);
  font-size: 0.95rem;
  width: 140px;
  font-family: var(--font-mono, monospace);
}
.periodo-error {
  color: var(--color-danger, #e53e3e);
  font-size: 0.85rem;
}
.excluidos {
  margin-top: 1.5rem;
  background: var(--bg-card);
  border: 1px solid var(--color-warning, #f59e0b);
  border-radius: var(--border-radius-lg);
  padding: 1.25rem 1.5rem;
}
.excluidos h3 {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 1rem;
  margin-bottom: 0.75rem;
  color: var(--color-warning-dark, #b45309);
}
.excluidos table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.9rem;
}
.excluidos th, .excluidos td {
  text-align: left;
  padding: 0.45rem 0.75rem;
  border-bottom: 1px solid var(--border-color);
}
.excluidos td.monto {
  text-align: right;
  font-family: var(--font-mono, monospace);
}
</style>
