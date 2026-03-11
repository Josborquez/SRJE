<template>
  <div class="importar-temge">
    <div class="page-header">
      <h1><FileInput :size="24" /> Importar Archivo TEMGE</h1>
      <p>Cargue el archivo bancario de ancho fijo para conciliacion de pagos.</p>
    </div>

    <AlertMessage v-if="alertMsg" :message="alertMsg" :type="alertType" @close="alertMsg = ''" />

    <div v-if="!preview">
      <FileUpload accept=".txt" @fileSelected="onFileSelected" />
      <div v-if="loading" class="loading">Procesando archivo TEMGE...</div>
      <div v-if="error" class="error"><CircleAlert :size="16" /> {{ error }}</div>
    </div>

    <div v-else>
      <div class="temge-info">
        <div class="temge-info-grid">
          <div class="temge-info-item">
            <Building :size="16" />
            <div><span class="info-label">Empresa</span><span class="info-value">{{ preview.codEmpresa }}</span></div>
          </div>
          <div class="temge-info-item">
            <Calendar :size="16" />
            <div><span class="info-label">Fecha</span><span class="info-value">{{ preview.fechaProceso }}</span></div>
          </div>
          <div class="temge-info-item">
            <Hash :size="16" />
            <div><span class="info-label">Total registros</span><span class="info-value">{{ preview.totalRegistrosCierre }}</span></div>
          </div>
          <div class="temge-info-item">
            <DollarSign :size="16" />
            <div><span class="info-label">Monto total</span><span class="info-value">${{ preview.montoTotalCierre?.toLocaleString('es-CL') }}</span></div>
          </div>
          <div class="temge-info-item">
            <ShieldCheck v-if="preview.integridadOk" :size="16" class="icon-ok" />
            <ShieldAlert v-else :size="16" class="icon-fail" />
            <div>
              <span class="info-label">Integridad</span>
              <span :class="preview.integridadOk ? 'info-value ok' : 'info-value fail'">
                {{ preview.integridadOk ? 'Verificada correctamente' : 'ERROR - Totales no coinciden' }}
              </span>
            </div>
          </div>
        </div>
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
import AlertMessage from '../components/AlertMessage.vue'
import { archivosApi } from '../api/index.js'
import {
  FileInput, CircleAlert, Building, Calendar,
  Hash, DollarSign, ShieldCheck, ShieldAlert
} from 'lucide-vue-next'

const preview = ref(null)
const loading = ref(false)
const error = ref(null)
const alertMsg = ref('')
const alertType = ref('info')

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
  alertMsg.value = ''
  try {
    const { data } = await archivosApi.previewTemge(file)
    preview.value = data
    if (!data.integridadOk) {
      alertType.value = 'warning'
      alertMsg.value = 'Los totales del archivo no coinciden con el registro de cierre. Verifique el archivo antes de continuar.'
    } else {
      alertType.value = 'success'
      alertMsg.value = `Archivo TEMGE cargado correctamente. ${data.lineas?.length || 0} registros encontrados.`
    }
  } catch (e) {
    error.value = e.response?.data?.error || e.message
    alertType.value = 'error'
    alertMsg.value = 'Error al procesar el archivo TEMGE.'
  } finally {
    loading.value = false
  }
}

function cancelar() {
  preview.value = null
  alertMsg.value = ''
}
</script>

<style scoped>
.temge-info {
  background: var(--bg-card);
  padding: 1.25rem;
  border-radius: var(--border-radius-lg);
  margin: 1.25rem 0;
  border: 1px solid var(--border-color);
}
.temge-info-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: 1rem;
}
.temge-info-item {
  display: flex;
  align-items: flex-start;
  gap: 0.5rem;
  color: var(--text-secondary);
}
.temge-info-item div { display: flex; flex-direction: column; }
.info-label { font-size: 0.78rem; color: var(--text-muted); text-transform: uppercase; letter-spacing: 0.03em; }
.info-value { font-size: 0.95rem; font-weight: 500; color: var(--text-primary); }
.info-value.ok { color: var(--color-success); }
.info-value.fail { color: var(--color-danger); }
.icon-ok { color: var(--color-success); }
.icon-fail { color: var(--color-danger); }
</style>
