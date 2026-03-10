<template>
  <div class="importar-nuevas-cuentas">
    <h1>Importar Nuevas Cuentas</h1>
    <p>Archivo Excel (.xlsx) con datos de nuevas cuentas bancarias.</p>

    <div v-if="!preview">
      <FileUpload accept=".xlsx,.xls" @fileSelected="onFileSelected" />
      <div v-if="loading" class="loading">Parseando Excel...</div>
      <div v-if="error" class="error">{{ error }}</div>
    </div>

    <div v-else>
      <PreviewImportacion
        :lineas="preview.lineas"
        :columnas="columnas"
        @confirmar="confirmar"
        @cancelar="cancelar"
      />

      <div v-if="resultado" class="resultado">
        <h3>Resultado</h3>
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
const resultado = ref(null)

const columnas = [
  { key: 'rutBeneficiario', label: 'RUT Benef.' },
  { key: 'dvBeneficiario', label: 'DV' },
  { key: 'nombreBeneficiario', label: 'Nombre' },
  { key: 'codBanco', label: 'Banco', editable: true, type: 'number' },
  { key: 'tipoCuenta', label: 'Tipo Cta', editable: true, type: 'number' },
  { key: 'numeroCuenta', label: 'N Cuenta', editable: true }
]

async function onFileSelected(file) {
  if (!file) return
  loading.value = true
  error.value = null
  try {
    const { data } = await archivosApi.previewNuevasCuentas(file)
    preview.value = data
  } catch (e) {
    error.value = e.response?.data?.error || e.message
  } finally {
    loading.value = false
  }
}

async function confirmar() {
  loading.value = true
  try {
    const { data } = await archivosApi.confirmarNuevasCuentas({
      lineas: preview.value.lineas.filter(l => l.incluir)
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
.loading { padding: 1rem; text-align: center; }
.error { padding: 0.8rem; background: #fce4ec; color: #c62828; border-radius: 4px; margin: 1rem 0; }
.resultado { margin-top: 1rem; padding: 1rem; background: #e8f5e9; border-radius: 8px; }
</style>
