<template>
  <div class="importar-temge">
    <h1>Importar Archivo TEMGE</h1>
    <p>Archivo bancario de ancho fijo para conciliacion de pagos.</p>

    <div v-if="!preview">
      <FileUpload accept=".txt" @fileSelected="onFileSelected" />
      <div v-if="loading" class="loading">Parseando archivo TEMGE...</div>
      <div v-if="error" class="error">{{ error }}</div>
    </div>

    <div v-else>
      <div class="temge-info">
        <p><strong>Empresa:</strong> {{ preview.codEmpresa }}</p>
        <p><strong>Fecha:</strong> {{ preview.fechaProceso }}</p>
        <p><strong>Total registros (cierre):</strong> {{ preview.totalRegistrosCierre }}</p>
        <p><strong>Monto total (cierre):</strong> ${{ preview.montoTotalCierre?.toLocaleString('es-CL') }}</p>
        <p><strong>Integridad:</strong>
          <span :class="preview.integridadOk ? 'ok' : 'fail'">
            {{ preview.integridadOk ? 'OK' : 'ERROR - Totales no coinciden' }}
          </span>
        </p>
      </div>

      <PreviewImportacion
        :lineas="preview.lineas"
        :columnas="columnas"
        @confirmar="() => {}"
        @cancelar="cancelar"
      />
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

const columnas = [
  { key: 'rutBeneficiario', label: 'RUT Benef.' },
  { key: 'dvBeneficiario', label: 'DV' },
  { key: 'nombreBeneficiario', label: 'Nombre' },
  { key: 'monto', label: 'Monto', format: 'monto' },
  { key: 'codBanco', label: 'Banco' },
  { key: 'numeroCuenta', label: 'Cuenta' }
]

async function onFileSelected(file) {
  if (!file) return
  loading.value = true
  error.value = null
  try {
    const { data } = await archivosApi.previewTemge(file)
    preview.value = data
  } catch (e) {
    error.value = e.response?.data?.error || e.message
  } finally {
    loading.value = false
  }
}

function cancelar() {
  preview.value = null
}
</script>

<style scoped>
.temge-info { background: #fff; padding: 1rem; border-radius: 8px; margin: 1rem 0; }
.temge-info p { margin: 0.3rem 0; }
.ok { color: #2e7d32; font-weight: bold; }
.fail { color: #c62828; font-weight: bold; }
.loading { padding: 1rem; text-align: center; }
.error { padding: 0.8rem; background: #fce4ec; color: #c62828; border-radius: 4px; margin: 1rem 0; }
</style>
