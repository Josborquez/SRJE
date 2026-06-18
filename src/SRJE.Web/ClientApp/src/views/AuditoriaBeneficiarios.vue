<template>
  <div class="auditoria-beneficiarios">
    <div class="page-header">
      <h1><FileSearch :size="24" /> Auditoria de Beneficiarios</h1>
      <p>Compare los beneficiarios del archivo de remuneraciones contra los registrados en el sistema.</p>
    </div>

    <AlertMessage v-if="alertMsg" :message="alertMsg" :type="alertType" @close="alertMsg = ''" />

    <!-- Upload -->
    <div v-if="!resultado">
      <FileUpload accept=".txt" @fileSelected="onFileSelected" />
      <div v-if="loading" class="loading">Comparando beneficiarios...</div>
      <div v-if="error" class="error"><CircleAlert :size="16" /> {{ error }}</div>
    </div>

    <!-- Resultados -->
    <div v-if="resultado">
      <!-- Resumen -->
      <div class="audit-summary">
        <div class="stat-card">
          <FileText :size="18" class="stat-icon" />
          <div class="stat-content">
            <span class="stat-value">{{ resultado.totalArchivo.toLocaleString('es-CL') }}</span>
            <span class="stat-label">En Archivo</span>
          </div>
        </div>
        <div class="stat-card">
          <Database :size="18" class="stat-icon" />
          <div class="stat-content">
            <span class="stat-value">{{ resultado.totalSistema.toLocaleString('es-CL') }}</span>
            <span class="stat-label">En Sistema</span>
          </div>
        </div>
        <div class="stat-card stat-warning">
          <UserPlus :size="18" class="stat-icon" />
          <div class="stat-content">
            <span class="stat-value">{{ resultado.soloEnArchivo.length }}</span>
            <span class="stat-label">Solo en Archivo</span>
          </div>
        </div>
        <div class="stat-card stat-danger">
          <UserMinus :size="18" class="stat-icon" />
          <div class="stat-content">
            <span class="stat-value">{{ resultado.soloEnSistema.length }}</span>
            <span class="stat-label">Solo en Sistema</span>
          </div>
        </div>
        <div class="stat-card stat-info">
          <ArrowLeftRight :size="18" class="stat-icon" />
          <div class="stat-content">
            <span class="stat-value">{{ resultado.conDiferencias.length }}</span>
            <span class="stat-label">Con Diferencias</span>
          </div>
        </div>
        <div class="stat-card stat-success">
          <CircleCheck :size="18" class="stat-icon" />
          <div class="stat-content">
            <span class="stat-value">{{ resultado.totalCoincidentes.toLocaleString('es-CL') }}</span>
            <span class="stat-label">Coincidentes</span>
          </div>
        </div>
      </div>

      <!-- Acciones -->
      <div class="audit-actions">
        <button class="btn btn-primary" @click="exportarExcel" :disabled="exportando">
          <FileSpreadsheet :size="16" /> Descargar Excel
        </button>
        <button class="btn btn-secondary" @click="exportarCsv" :disabled="exportando">
          <FileText :size="16" /> Descargar CSV
        </button>
        <button class="btn btn-secondary" @click="cancelar">
          <RotateCcw :size="16" /> Nueva Comparacion
        </button>
      </div>

      <!-- Tabs -->
      <div class="audit-tabs">
        <button
          :class="['tab-btn', { active: tab === 'archivo' }]"
          @click="tab = 'archivo'"
        >
          Solo en Archivo ({{ resultado.soloEnArchivo.length }})
        </button>
        <button
          :class="['tab-btn', { active: tab === 'sistema' }]"
          @click="tab = 'sistema'"
        >
          Solo en Sistema ({{ resultado.soloEnSistema.length }})
        </button>
        <button
          :class="['tab-btn', { active: tab === 'diferencias' }]"
          @click="tab = 'diferencias'"
        >
          Con Diferencias ({{ resultado.conDiferencias.length }})
        </button>
      </div>

      <!-- Tab: Solo en Archivo -->
      <div v-if="tab === 'archivo'">
        <div v-if="!resultado.soloEnArchivo.length" class="empty-state">
          <CircleCheck :size="48" class="empty-icon" />
          <p>Todos los beneficiarios del archivo existen en el sistema.</p>
        </div>
        <table v-else class="data-table">
          <thead>
            <tr>
              <th>#</th>
              <th>RUT Beneficiario</th>
              <th>Nombre</th>
              <th>RUT Funcionario</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(item, i) in resultado.soloEnArchivo" :key="item.rutBeneficiario">
              <td>{{ i + 1 }}</td>
              <td>{{ item.rutFormateado }}</td>
              <td>{{ item.nombreBeneficiario }}</td>
              <td>{{ item.rutFuncionarioFormateado || '-' }}</td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Tab: Solo en Sistema -->
      <div v-if="tab === 'sistema'">
        <div v-if="!resultado.soloEnSistema.length" class="empty-state">
          <CircleCheck :size="48" class="empty-icon" />
          <p>Todos los beneficiarios del sistema estan en el archivo.</p>
        </div>
        <table v-else class="data-table">
          <thead>
            <tr>
              <th>#</th>
              <th>RUT Beneficiario</th>
              <th>Nombre</th>
              <th>Estado</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(item, i) in resultado.soloEnSistema" :key="item.rutBeneficiario">
              <td>{{ i + 1 }}</td>
              <td>{{ item.rutFormateado }}</td>
              <td>{{ item.nombreBeneficiario }}</td>
              <td>{{ item.estado === 'A' ? 'Activo' : 'Inactivo' }}</td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Tab: Con Diferencias -->
      <div v-if="tab === 'diferencias'">
        <div v-if="!resultado.conDiferencias.length" class="empty-state">
          <CircleCheck :size="48" class="empty-icon" />
          <p>No se encontraron diferencias en los datos coincidentes.</p>
        </div>
        <table v-else class="data-table">
          <thead>
            <tr>
              <th>#</th>
              <th>RUT</th>
              <th>Campo</th>
              <th>Valor Archivo</th>
              <th>Valor Sistema</th>
              <th>Accion</th>
            </tr>
          </thead>
          <tbody>
            <template v-for="(dif, i) in resultado.conDiferencias" :key="dif.rutBeneficiario">
              <tr v-for="(campo, j) in dif.diferencias" :key="`${dif.rutBeneficiario}-${j}`">
                <td>{{ j === 0 ? i + 1 : '' }}</td>
                <td>{{ j === 0 ? dif.rutFormateado : '' }}</td>
                <td>{{ campo.campo }}</td>
                <td class="val-archivo">{{ campo.valorArchivo }}</td>
                <td class="val-sistema">{{ campo.valorSistema }}</td>
                <td class="acciones-dif">
                  <template v-if="campo.campo === 'Nombre'">
                    <button
                      class="btn-mini btn-mini-archivo"
                      :disabled="procesando === dif.rutBeneficiario"
                      title="Actualizar el sistema con el valor del archivo"
                      @click="usarArchivo(dif, campo)"
                    >
                      <Check :size="13" /> Usar Archivo
                    </button>
                    <button
                      class="btn-mini btn-mini-sistema"
                      :disabled="procesando === dif.rutBeneficiario"
                      title="Mantener el valor del sistema y descartar la diferencia"
                      @click="mantenerSistema(dif)"
                    >
                      Mantener Sistema
                    </button>
                  </template>
                </td>
              </tr>
            </template>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import FileUpload from '../components/FileUpload.vue'
import AlertMessage from '../components/AlertMessage.vue'
import { archivosApi, beneficiariosApi } from '../api/index.js'
import {
  FileSearch, FileText, FileSpreadsheet, Database,
  UserPlus, UserMinus, ArrowLeftRight, CircleCheck,
  CircleAlert, RotateCcw, Check
} from 'lucide-vue-next'

const resultado = ref(null)
const loading = ref(false)
const exportando = ref(false)
const error = ref(null)
const alertMsg = ref('')
const alertType = ref('info')
const tab = ref('archivo')
const procesando = ref(null)
let archivoOriginal = null

async function usarArchivo(dif, campo) {
  procesando.value = dif.rutBeneficiario
  try {
    await beneficiariosApi.corregirNombre(dif.rutBeneficiario, { nombreBeneficiario: campo.valorArchivo })
    quitarDiferencia(dif)
    resultado.value.totalCoincidentes++
    alertType.value = 'success'
    alertMsg.value = `Sistema actualizado: ${dif.rutFormateado} -> "${campo.valorArchivo}".`
  } catch (e) {
    alertType.value = 'error'
    alertMsg.value = e.response?.data?.error || 'No se pudo actualizar el nombre en el sistema.'
  } finally {
    procesando.value = null
  }
}

function mantenerSistema(dif) {
  quitarDiferencia(dif)
  alertType.value = 'info'
  alertMsg.value = `Se mantuvo el valor del sistema para ${dif.rutFormateado}.`
}

function quitarDiferencia(dif) {
  resultado.value.conDiferencias = resultado.value.conDiferencias
    .filter(d => d.rutBeneficiario !== dif.rutBeneficiario)
}

async function onFileSelected(file) {
  if (!file) return
  archivoOriginal = file
  loading.value = true
  error.value = null
  alertMsg.value = ''
  resultado.value = null
  try {
    const { data } = await archivosApi.compararAuditoria(file)
    resultado.value = data
    alertType.value = 'info'
    alertMsg.value = `Comparacion completada: ${data.totalArchivo} beneficiarios en archivo, ${data.totalSistema} en sistema.`
  } catch (e) {
    error.value = e.response?.data?.error || e.message
  } finally {
    loading.value = false
  }
}

async function exportarExcel() {
  if (!archivoOriginal) return
  exportando.value = true
  try {
    const { data } = await archivosApi.exportarAuditoriaExcel(archivoOriginal)
    descargarBlob(data, `Auditoria_Beneficiarios_${fechaHoy()}.xlsx`)
  } catch (e) {
    alertType.value = 'error'
    alertMsg.value = 'Error al generar el archivo Excel.'
  } finally {
    exportando.value = false
  }
}

async function exportarCsv() {
  if (!archivoOriginal) return
  exportando.value = true
  try {
    const { data } = await archivosApi.exportarAuditoriaCsv(archivoOriginal)
    descargarBlob(data, `Auditoria_Beneficiarios_${fechaHoy()}.csv`)
  } catch (e) {
    alertType.value = 'error'
    alertMsg.value = 'Error al generar el archivo CSV.'
  } finally {
    exportando.value = false
  }
}

function descargarBlob(blob, nombre) {
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = nombre
  a.click()
  URL.revokeObjectURL(url)
}

function fechaHoy() {
  return new Date().toISOString().slice(0, 10).replace(/-/g, '')
}

function cancelar() {
  resultado.value = null
  error.value = null
  alertMsg.value = ''
  archivoOriginal = null
  tab.value = 'archivo'
}
</script>

<style scoped>
.audit-summary {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
  margin-bottom: 1.25rem;
}

.audit-summary .stat-card .stat-icon { color: var(--color-primary); }
.audit-summary .stat-warning .stat-icon { color: var(--color-warning); }
.audit-summary .stat-danger .stat-icon { color: var(--color-danger); }
.audit-summary .stat-info .stat-icon { color: var(--color-info); }
.audit-summary .stat-success .stat-icon { color: var(--color-success); }

.audit-actions {
  display: flex;
  gap: 0.75rem;
  margin-bottom: 1.25rem;
}

.audit-tabs {
  display: flex;
  gap: 0.25rem;
  margin-bottom: 1rem;
  border-bottom: 2px solid var(--border-color);
}

.tab-btn {
  padding: 0.6rem 1.2rem;
  background: none;
  border: none;
  border-bottom: 2px solid transparent;
  margin-bottom: -2px;
  font-size: 0.88rem;
  font-weight: 500;
  color: var(--text-secondary);
  cursor: pointer;
  transition: all 0.2s ease;
}

.tab-btn:hover { color: var(--text-primary); }

.tab-btn.active {
  color: var(--color-primary);
  border-bottom-color: var(--color-primary);
}

.val-archivo { color: var(--color-warning); font-weight: 500; }
.val-sistema { color: var(--color-info); font-weight: 500; }

.acciones-dif {
  display: flex;
  gap: 0.4rem;
  flex-wrap: wrap;
  white-space: nowrap;
}

.btn-mini {
  display: inline-flex;
  align-items: center;
  gap: 0.25rem;
  padding: 0.3rem 0.6rem;
  font-size: 0.78rem;
  font-weight: 500;
  border: 1px solid transparent;
  border-radius: 4px;
  cursor: pointer;
  transition: all 0.15s ease;
}

.btn-mini:disabled { opacity: 0.5; cursor: not-allowed; }

.btn-mini-archivo {
  background: var(--color-primary);
  color: #fff;
}
.btn-mini-archivo:hover:not(:disabled) { filter: brightness(0.93); }

.btn-mini-sistema {
  background: none;
  border-color: var(--border-color);
  color: var(--text-secondary);
}
.btn-mini-sistema:hover:not(:disabled) { color: var(--text-primary); }
</style>
