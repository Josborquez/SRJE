<template>
  <div class="generar-temge">
    <h1>Generar Archivo TEMGE</h1>
    <p>Genera el archivo de pago en formato de ancho fijo compatible con el banco.</p>
    <p>Se incluiran todos los beneficiarios activos con cuenta bancaria y retenciones vigentes.</p>

    <AlertMessage v-if="alertMsg" :message="alertMsg" :type="alertType" @close="alertMsg = ''" />

    <div class="actions">
      <button @click="showConfirm = true" class="btn btn-primary" :disabled="loading">
        {{ loading ? 'Generando...' : 'Generar Archivo TEMGE' }}
      </button>
    </div>

    <div v-if="error" class="error">{{ error }}</div>

    <div v-if="generado" class="resultado">
      <h3>Archivo generado exitosamente</h3>
      <p>El archivo se descargo automaticamente.</p>
    </div>

    <ConfirmModal
      v-model="showConfirm"
      title="Generar Archivo TEMGE"
      message="Se generara el archivo TEMGE con todos los beneficiarios activos que tengan cuenta bancaria y retenciones vigentes. Desea continuar?"
      confirmText="Si, generar"
      @confirm="generar"
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { archivosApi } from '../api/index.js'
import AlertMessage from '../components/AlertMessage.vue'
import ConfirmModal from '../components/ConfirmModal.vue'

const loading = ref(false)
const error = ref(null)
const generado = ref(false)
const alertMsg = ref('')
const alertType = ref('info')
const showConfirm = ref(false)

async function generar() {
  loading.value = true
  error.value = null
  generado.value = false
  alertMsg.value = ''
  try {
    const response = await archivosApi.generarTemge()
    // Descargar blob
    const url = window.URL.createObjectURL(new Blob([response.data]))
    const link = document.createElement('a')
    link.href = url
    const filename = response.headers['content-disposition']
      ?.match(/filename="?(.+)"?/)?.[1]
      || `TEMGE_${new Date().toISOString().slice(0,10)}.txt`
    link.setAttribute('download', filename)
    document.body.appendChild(link)
    link.click()
    link.remove()
    window.URL.revokeObjectURL(url)
    generado.value = true
    alertType.value = 'success'
    alertMsg.value = 'Archivo TEMGE generado y descargado exitosamente.'
  } catch (e) {
    error.value = e.response?.data?.error || e.message
    alertType.value = 'error'
    alertMsg.value = 'Error al generar el archivo TEMGE.'
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
/* Estilos globales aplicados desde assets/styles.css */
/* Solo overrides especificos de este componente */
.actions { margin: 1.5rem 0; }
</style>
