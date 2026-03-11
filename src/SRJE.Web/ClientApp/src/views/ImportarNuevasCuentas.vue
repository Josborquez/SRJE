<template>
  <div class="importar-nuevas-cuentas">
    <div class="page-header">
      <h1><CreditCard :size="24" /> Importar Nuevas Cuentas</h1>
      <p>Cargue un archivo Excel (.xlsx) con los datos de nuevas cuentas bancarias de beneficiarios.</p>
    </div>

    <AlertMessage v-if="alertMsg" :message="alertMsg" :type="alertType" @close="alertMsg = ''" />

    <div v-if="!preview">
      <FileUpload accept=".xlsx,.xls" @fileSelected="onFileSelected" />
      <div v-if="loading" class="loading">Procesando archivo Excel...</div>
      <div v-if="error" class="error"><CircleAlert :size="16" /> {{ error }}</div>
    </div>

    <div v-else>
      <PreviewImportacion
        :lineas="preview.lineas"
        :columnas="columnas"
        @confirmar="mostrarConfirmacion"
        @cancelar="cancelar"
      />

      <div v-if="resultado" class="resultado">
        <h3><CheckCircle :size="18" /> Resultado de Importacion</h3>
        <p>Insertados: <strong>{{ resultado.insertados }}</strong> |
          Actualizados: <strong>{{ resultado.actualizados }}</strong> |
          Excluidos: <strong>{{ resultado.excluidos }}</strong> |
          Errores: <strong>{{ resultado.errores }}</strong></p>
      </div>
    </div>

    <ConfirmModal
      v-model="showConfirm"
      title="Confirmar Importacion de Cuentas"
      :message="`Se importaran ${lineasSeleccionadas} registros de nuevas cuentas bancarias. ¿Desea continuar?`"
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
import { CreditCard, CircleAlert, CheckCircle } from 'lucide-vue-next'

const preview = ref(null)
const loading = ref(false)
const error = ref(null)
const resultado = ref(null)
const alertMsg = ref('')
const alertType = ref('info')
const showConfirm = ref(false)

const columnas = [
  { key: 'rutBeneficiario', label: 'RUT Benef.' },
  { key: 'dvBeneficiario', label: 'DV' },
  { key: 'nombreBeneficiario', label: 'Nombre' },
  { key: 'codBanco', label: 'Banco', editable: true, type: 'number' },
  { key: 'tipoCuenta', label: 'Tipo Cta', editable: true, type: 'number' },
  { key: 'numeroCuenta', label: 'N Cuenta', editable: true }
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
    const { data } = await archivosApi.previewNuevasCuentas(file)
    preview.value = data
    alertType.value = 'info'
    alertMsg.value = `Se cargaron ${data.lineas?.length || 0} registros del archivo. Revise los datos y confirme la importacion.`
  } catch (e) {
    error.value = e.response?.data?.error || e.message
  } finally {
    loading.value = false
  }
}

function mostrarConfirmacion() {
  showConfirm.value = true
}

async function confirmar() {
  loading.value = true
  alertMsg.value = ''
  try {
    const { data } = await archivosApi.confirmarNuevasCuentas({
      lineas: preview.value.lineas.filter(l => l.incluir)
    })
    resultado.value = data
    alertType.value = 'success'
    alertMsg.value = `Importacion completada: ${data.insertados} insertados, ${data.actualizados} actualizados.`
  } catch (e) {
    error.value = e.response?.data?.error || e.message
    alertType.value = 'error'
    alertMsg.value = 'Error al confirmar la importacion de cuentas.'
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
