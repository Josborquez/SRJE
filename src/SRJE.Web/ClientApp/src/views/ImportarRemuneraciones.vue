<template>
  <div class="importar-remuneraciones">
    <h1>Importar ENVIO REMUNERACIONES</h1>
    <p>Archivo de texto de ancho fijo (126 chars) del sistema de remuneraciones.</p>

    <AlertMessage v-if="alertMsg" :message="alertMsg" :type="alertType" @close="alertMsg = ''" />

    <div v-if="!preview">
      <FileUpload accept=".txt" @fileSelected="onFileSelected" />
      <div v-if="loading" class="loading">Parseando archivo...</div>
      <div v-if="error" class="error">{{ error }}</div>
    </div>

    <div v-else>
      <div class="periodo-input">
        <label>Periodo Proceso (AAAAMM):</label>
        <input v-model="periodo" placeholder="Ej: 202602" maxlength="6" />
      </div>

      <PreviewImportacion
        :lineas="preview.lineas"
        :columnas="columnas"
        @confirmar="mostrarConfirmacion"
        @cancelar="cancelar"
      />

      <div v-if="resultado" class="resultado">
        <h3>Resultado de Importacion</h3>
        <p>{{ resultado.mensaje }}</p>
        <p>Insertados: {{ resultado.insertados }} | Actualizados: {{ resultado.actualizados }}
          | Excluidos: {{ resultado.excluidos }} | Errores: {{ resultado.errores }}</p>
      </div>
    </div>

    <ConfirmModal
      v-model="showConfirm"
      title="Confirmar Importacion de Remuneraciones"
      :message="`Se importaran ${lineasSeleccionadas} registros para el periodo ${periodo}. Desea continuar?`"
      confirmText="Si, importar"
      @confirm="confirmar"
    />
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import FileUpload from '../components/FileUpload.vue'
import PreviewImportacion from '../components/PreviewImportacion.vue'
import AlertMessage from '../components/AlertMessage.vue'
import ConfirmModal from '../components/ConfirmModal.vue'
import { archivosApi } from '../api/index.js'

const preview = ref(null)
const loading = ref(false)
const error = ref(null)
const periodo = ref('')
const resultado = ref(null)
const alertMsg = ref('')
const alertType = ref('info')
const showConfirm = ref(false)

const columnas = [
  { key: 'rutBeneficiario', label: 'RUT Benef.', format: 'rut' },
  { key: 'dvBeneficiario', label: 'DV' },
  { key: 'nombreBeneficiario', label: 'Nombre' },
  { key: 'rutFuncionario', label: 'RUT Func.' },
  { key: 'codRetencion', label: 'Cod. Retencion', editable: true },
  { key: 'tipoPago', label: 'Tipo Pago', editable: true }
]

const lineasSeleccionadas = computed(() =>
  preview.value?.lineas?.filter(l => l.incluir).length || 0
)

async function onFileSelected(file) {
  if (!file) return
  loading.value = true
  error.value = null
  alertMsg.value = ''
  try {
    const { data } = await archivosApi.previewRemuneraciones(file)
    preview.value = data
    alertType.value = 'info'
    alertMsg.value = `Se cargaron ${data.lineas?.length || 0} registros del archivo. Ingrese el periodo y confirme.`
  } catch (e) {
    error.value = e.response?.data?.error || e.message
  } finally {
    loading.value = false
  }
}

function mostrarConfirmacion() {
  if (!periodo.value || periodo.value.length !== 6) {
    alertType.value = 'warning'
    alertMsg.value = 'Debe ingresar un periodo valido en formato AAAAMM (ej: 202602).'
    return
  }
  showConfirm.value = true
}

async function confirmar() {
  loading.value = true
  alertMsg.value = ''
  try {
    const { data } = await archivosApi.confirmarRemuneraciones({
      lineas: preview.value.lineas.filter(l => l.incluir),
      periodoProceso: periodo.value
    })
    resultado.value = data
    alertType.value = 'success'
    alertMsg.value = `Importacion completada: ${data.insertados} insertados, ${data.actualizados} actualizados.`
  } catch (e) {
    error.value = e.response?.data?.error || e.message
    alertType.value = 'error'
    alertMsg.value = 'Error al confirmar la importacion de remuneraciones.'
  } finally {
    loading.value = false
  }
}

function cancelar() {
  preview.value = null
  resultado.value = null
  alertMsg.value = ''
}
</script>

<style scoped>
.periodo-input { margin: 1rem 0; display: flex; align-items: center; gap: 0.5rem; }
.periodo-input input { padding: 0.4rem; border: 1px solid #ccc; border-radius: 4px; width: 120px; }
.loading { padding: 1rem; text-align: center; color: #666; }
.error { padding: 0.8rem; background: #fce4ec; color: #c62828; border-radius: 4px; margin: 1rem 0; }
.resultado { margin-top: 1rem; padding: 1rem; background: #e8f5e9; border-radius: 8px; }
.resultado h3 { color: #2e7d32; margin-bottom: 0.5rem; }
</style>
