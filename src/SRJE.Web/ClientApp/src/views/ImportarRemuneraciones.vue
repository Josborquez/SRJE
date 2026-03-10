<template>
  <div class="importar-remuneraciones">
    <h1>Importar ENVIO REMUNERACIONES</h1>
    <p>Archivo de texto de ancho fijo (126 chars) del sistema de remuneraciones.</p>

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
        @confirmar="confirmar"
        @cancelar="cancelar"
      />

      <div v-if="resultado" class="resultado">
        <h3>Resultado de Importacion</h3>
        <p>{{ resultado.mensaje }}</p>
        <p>Insertados: {{ resultado.insertados }} | Actualizados: {{ resultado.actualizados }}
          | Excluidos: {{ resultado.excluidos }} | Errores: {{ resultado.errores }}</p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import FileUpload from '../components/FileUpload.vue'
import PreviewImportacion from '../components/PreviewImportacion.vue'
import { archivosApi } from '../api/index.js'

const preview = ref(null)
const loading = ref(false)
const error = ref(null)
const periodo = ref('')
const resultado = ref(null)

const columnas = [
  { key: 'rutBeneficiario', label: 'RUT Benef.', format: 'rut' },
  { key: 'dvBeneficiario', label: 'DV' },
  { key: 'nombreBeneficiario', label: 'Nombre' },
  { key: 'rutFuncionario', label: 'RUT Func.' },
  { key: 'codRetencion', label: 'Cod. Retencion', editable: true },
  { key: 'tipoPago', label: 'Tipo Pago', editable: true }
]

async function onFileSelected(file) {
  if (!file) return
  loading.value = true
  error.value = null
  try {
    const { data } = await archivosApi.previewRemuneraciones(file)
    preview.value = data
  } catch (e) {
    error.value = e.response?.data?.error || e.message
  } finally {
    loading.value = false
  }
}

async function confirmar() {
  if (!periodo.value || periodo.value.length !== 6) {
    error.value = 'Ingrese periodo AAAAMM valido'
    return
  }
  loading.value = true
  try {
    const { data } = await archivosApi.confirmarRemuneraciones({
      lineas: preview.value.lineas.filter(l => l.incluir),
      periodoProceso: periodo.value
    })
    resultado.value = data
  } catch (e) {
    error.value = e.response?.data?.error || e.message
  } finally {
    loading.value = false
  }
}

function cancelar() {
  preview.value = null
  resultado.value = null
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
