<template>
  <div class="importar-remuneraciones">
    <div class="page-header">
      <h1><FileSpreadsheet :size="24" /> Importar Remuneraciones</h1>
      <p>Cargue el archivo de texto de ancho fijo (126 caracteres) del sistema de remuneraciones.</p>
    </div>

    <AlertMessage v-if="alertMsg" :message="alertMsg" :type="alertType" @close="alertMsg = ''" />

    <div v-if="!preview">
      <FileUpload accept=".txt" @fileSelected="onFileSelected" />
      <div v-if="loading" class="loading">Procesando archivo de remuneraciones...</div>
      <div v-if="error" class="error"><CircleAlert :size="16" /> {{ error }}</div>
    </div>

    <div v-else>
      <div class="periodo-input">
        <label><Calendar :size="16" /> Periodo Proceso (AAAAMM):</label>
        <input v-model="periodo" placeholder="Ej: 202602" maxlength="6" />
      </div>

      <PreviewImportacion
        :lineas="preview.lineas"
        :columnas="columnas"
        @confirmar="mostrarConfirmacion"
        @cancelar="cancelar"
      />

      <div v-if="resultado" class="resultado">
        <h3><CheckCircle :size="18" /> Resultado de Importacion</h3>
        <p>{{ resultado.mensaje }}</p>
        <p>Insertados: <strong>{{ resultado.insertados }}</strong> |
          Actualizados: <strong>{{ resultado.actualizados }}</strong> |
          Excluidos: <strong>{{ resultado.excluidos }}</strong> |
          Errores: <strong>{{ resultado.errores }}</strong></p>
      </div>
    </div>

    <ConfirmModal
      v-model="showConfirm"
      title="Confirmar Importacion de Remuneraciones"
      :message="`Se importaran ${lineasSeleccionadas} registros para el periodo ${periodo}. ¿Desea continuar?`"
      confirmText="Si, importar"
      @confirm="confirmar"
    />
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import FileUpload from '../components/FileUpload.vue'
import PreviewImportacion from '../components/PreviewImportacion.vue'
import AlertMessage from '../components/AlertMessage.vue'
import ConfirmModal from '../components/ConfirmModal.vue'
import { archivosApi, catalogosApi } from '../api/index.js'
import { FileSpreadsheet, CircleAlert, Calendar, CheckCircle } from 'lucide-vue-next'

const preview = ref(null)
const opcionesCodRetencion = ref([])
const opcionesTipoPago = ref([
  { value: 'DEPOSITO A CUENTA', label: 'DEPOSITO A CUENTA' },
  { value: 'VALE VISTA', label: 'VALE VISTA' },
  { value: 'CHEQUE', label: 'CHEQUE' },
  { value: 'TRANSFERENCIA', label: 'TRANSFERENCIA' }
])
const opcionesBancos = ref([])
const opcionesTiposCuenta = ref([])
const loading = ref(false)
const error = ref(null)
const periodo = ref('')
const resultado = ref(null)
const alertMsg = ref('')
const alertType = ref('info')
const showConfirm = ref(false)

const cuentasPorBeneficiario = ref({})

const columnas = computed(() => [
  { key: 'rutBeneficiario', label: 'RUT Benef.', format: 'rut' },
  { key: 'dvBeneficiario', label: 'DV' },
  { key: 'nombreBeneficiario', label: 'Nombre' },
  { key: 'rutFuncionario', label: 'RUT Func.' },
  { key: 'codRetencion', label: 'Cod. Retencion', editable: true, options: opcionesCodRetencion.value },
  { key: 'tipoPago', label: 'Tipo Pago', editable: true, options: opcionesTipoPago.value },
  { key: 'monto', label: 'Monto', format: 'monto' },
  {
    key: 'cuentaSeleccionada',
    label: 'Cuenta',
    editableWhen: 'esMulticuenta',
    optionsFrom: (linea) => {
      const cuentas = cuentasPorBeneficiario.value[linea.rutBeneficiario]
      if (!cuentas?.length) return []
      return cuentas.map(c => ({
        value: c.id,
        label: `${c.nombreBanco || c.codBanco} - ${c.numeroCuenta}${c.alias ? ' (' + c.alias + ')' : ''}`
      }))
    },
    numeric: true,
    onChange: (linea, val) => {
      const cuentas = cuentasPorBeneficiario.value[linea.rutBeneficiario]
      const cuenta = cuentas?.find(c => c.id === val)
      if (cuenta) {
        linea.codBanco = cuenta.codBanco
        linea.tipoCuenta = cuenta.tipoCuenta
        linea.numeroCuenta = cuenta.numeroCuenta
      }
    }
  },
  { key: 'codBanco', label: 'Banco', editableWhen: 'esMulticuenta', numeric: true, options: opcionesBancos.value },
  { key: 'tipoCuenta', label: 'Tipo Cta', editableWhen: 'esMulticuenta', numeric: true, options: opcionesTiposCuenta.value },
  { key: 'numeroCuenta', label: 'N° Cuenta', editableWhen: 'esMulticuenta' }
])

const lineasSeleccionadas = computed(() =>
  preview.value?.lineas?.filter(l => l.incluir).length || 0
)

onMounted(async () => {
  try {
    const [retRes, bancosRes, tiposCtaRes] = await Promise.all([
      catalogosApi.tiposRetencion(),
      catalogosApi.bancos(),
      catalogosApi.tiposCuenta()
    ])
    opcionesCodRetencion.value = retRes.data.map(t => ({
      value: t.codRetencion,
      label: `${t.codRetencion} - ${t.descripcion}`
    }))
    opcionesBancos.value = bancosRes.data.map(b => ({
      value: b.codBanco,
      label: `${b.codBanco} - ${b.nombreBanco}`
    }))
    opcionesTiposCuenta.value = tiposCtaRes.data.map(t => ({
      value: t.codTipoCuenta,
      label: `${t.codTipoCuenta} - ${t.descripcion}`
    }))
  } catch (e) {
    console.error('Error cargando catalogos:', e)
  }
})

async function onFileSelected(file) {
  if (!file) return
  loading.value = true
  error.value = null
  alertMsg.value = ''
  try {
    const { data } = await archivosApi.previewRemuneraciones(file)
    preview.value = data
    cuentasPorBeneficiario.value = data.cuentasPorBeneficiario || {}
    if (data.lineasMulticuenta > 0) {
      alertType.value = 'warning'
      alertMsg.value = `Se cargaron ${data.lineas?.length || 0} registros. ${data.lineasMulticuenta} lineas multicuenta requieren asignacion manual de cuenta bancaria.`
    } else {
      alertType.value = 'info'
      alertMsg.value = `Se cargaron ${data.lineas?.length || 0} registros del archivo. Ingrese el periodo y confirme la importacion.`
    }
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
.periodo-input {
  margin: 1.25rem 0;
  display: flex;
  align-items: center;
  gap: 0.6rem;
  background: var(--bg-card);
  padding: 0.9rem 1.2rem;
  border-radius: var(--border-radius);
  border: 1px solid var(--border-color);
}
.periodo-input label {
  font-weight: 500;
  font-size: 0.9rem;
  display: flex;
  align-items: center;
  gap: 0.35rem;
  color: var(--text-secondary);
  white-space: nowrap;
}
.periodo-input input {
  padding: 0.5rem 0.75rem;
  border: 1px solid var(--border-color);
  border-radius: var(--border-radius);
  width: 130px;
  font-size: 0.9rem;
}
.periodo-input input:focus {
  outline: none;
  border-color: var(--color-primary);
  box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
}
</style>
