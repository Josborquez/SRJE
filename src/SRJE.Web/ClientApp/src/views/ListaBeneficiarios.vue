<template>
  <div class="lista-beneficiarios">
    <div class="page-header">
      <h1><Users :size="24" /> Beneficiarios</h1>
    </div>

    <div v-if="store.totalInscritos > 0" class="stats-bar">
      <div class="stat-card">
        <Users :size="18" class="stat-icon" />
        <div class="stat-content">
          <span class="stat-value">{{ store.totalInscritos.toLocaleString('es-CL') }}</span>
          <span class="stat-label">Total Inscritos</span>
        </div>
      </div>
      <div class="stat-card stat-activos">
        <CircleCheck :size="18" class="stat-icon" />
        <div class="stat-content">
          <span class="stat-value">{{ store.totalActivos.toLocaleString('es-CL') }}</span>
          <span class="stat-label">Activos</span>
        </div>
      </div>
      <div class="stat-card stat-inactivos">
        <CircleX :size="18" class="stat-icon" />
        <div class="stat-content">
          <span class="stat-value">{{ store.totalInactivos.toLocaleString('es-CL') }}</span>
          <span class="stat-label">Inactivos</span>
        </div>
      </div>
    </div>

    <div class="toolbar">
      <div class="search-wrapper">
        <Search :size="18" class="search-icon" />
        <input
          v-model="busqueda"
          @input="debounceBuscar"
          placeholder="Buscar por nombre o RUT..."
          class="search-input"
        />
      </div>
      <div class="toolbar-actions">
        <div class="export-group">
          <button @click="exportar('excel')" class="btn btn-secondary" :disabled="exportando">
            <FileSpreadsheet :size="16" /> Excel
          </button>
          <button @click="exportar('csv')" class="btn btn-secondary" :disabled="exportando">
            <FileText :size="16" /> CSV
          </button>
        </div>
        <router-link to="/beneficiarios/nuevo" class="btn btn-primary">
          <UserPlus :size="16" /> Nuevo Beneficiario
        </router-link>
      </div>
    </div>

    <AlertMessage v-if="alertMsg" :message="alertMsg" :type="alertType" @close="alertMsg = ''" />

    <div v-if="store.loading" class="loading">Cargando beneficiarios...</div>
    <div v-if="store.error" class="error"><CircleAlert :size="16" /> {{ store.error }}</div>

    <div v-if="!store.loading && !store.items.length && !store.error" class="empty-state">
      <Inbox :size="48" class="empty-icon" />
      <p>No se encontraron beneficiarios.</p>
      <small>Intenta con otra busqueda o crea un nuevo beneficiario.</small>
    </div>

    <div v-if="store.items.length" class="table-responsive">
    <table class="data-table">
      <thead>
        <tr>
          <th>RUT</th>
          <th>Nombre</th>
          <th>Funcionario(s)</th>
          <th>Banco</th>
          <th>Cuenta</th>
          <th>Retenciones</th>
          <th>Monto Mensual</th>
          <th>Estado</th>
          <th>Acciones</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="b in store.items" :key="b.id">
          <td>{{ b.rutFormateado }}</td>
          <td>{{ b.nombreBeneficiario }}</td>
          <td>
            <template v-if="b.funcionarios?.length">
              <div v-for="f in b.funcionarios" :key="f.rutFuncionario" style="line-height: 1.4;">
                <router-link :to="`/funcionarios/${f.rutFuncionario}`" class="func-link">
                  {{ f.nombreCompleto || f.rutFormateado }}
                </router-link>
              </div>
            </template>
            <span v-else>-</span>
          </td>
          <td>{{ b.nombreBanco || b.codBanco || '-' }}</td>
          <td>{{ formatCuenta(b.ctaEstado || b.ctaOtBanco) }}</td>
          <td class="text-center">{{ b.cantidadRetenciones || 0 }}</td>
          <td class="text-right">
            <template v-if="b.desgloseCuentas?.length > 1">
              <div v-for="(dc, idx) in b.desgloseCuentas" :key="idx" class="desglose-linea">
                <span class="desglose-banco">{{ dc.nombreBanco || 'Sin banco' }}</span>
                <span class="desglose-cuenta" v-if="dc.numeroCuenta"> {{ formatCuenta(dc.numeroCuenta) }}</span>:
                ${{ dc.monto.toLocaleString('es-CL') }} ({{ dc.cantidad }})
              </div>
              <div class="desglose-total"><strong>Total: ${{ b.montoTotalRetenciones.toLocaleString('es-CL') }}</strong></div>
            </template>
            <template v-else>{{ b.montoTotalRetenciones ? '$' + b.montoTotalRetenciones.toLocaleString('es-CL') : '-' }}</template>
          </td>
          <td>
            <span :class="'estado estado-' + b.estado.toLowerCase()">
              <CircleCheck v-if="b.estado === 'A'" :size="13" />
              <CircleX v-else :size="13" />
              {{ b.estado === 'A' ? 'Activo' : 'Inactivo' }}
            </span>
          </td>
          <td class="actions inline">
            <router-link :to="`/beneficiarios/${b.rutBeneficiario}`" class="btn-sm">
              <Eye :size="14" /> Ver
            </router-link>
            <router-link :to="`/beneficiarios/${b.rutBeneficiario}/editar`" class="btn-sm">
              <Pencil :size="14" /> Editar
            </router-link>
            <button v-if="b.estado === 'A'" class="btn-sm btn-sm-danger" @click="confirmarInactivar(b)">
              <Ban :size="14" /> Inactivar
            </button>
          </td>
        </tr>
      </tbody>
    </table>
    </div>

    <!-- Paginacion -->
    <div v-if="store.totalPages >= 1" class="pagination-bar">
      <div class="pagination-info">
        Mostrando {{ rangoInicio }}-{{ rangoFin }} de {{ store.totalCount }} registros
      </div>
      <div class="pagination-controls">
        <select v-model="tamanioPagina" @change="cambiarTamano" class="page-size-select">
          <option :value="10">10 por pag.</option>
          <option :value="20">20 por pag.</option>
          <option :value="50">50 por pag.</option>
          <option :value="100">100 por pag.</option>
        </select>
        <button @click="cambiarPagina(1)" :disabled="store.page <= 1" class="page-btn" title="Primera">&laquo;</button>
        <button @click="cambiarPagina(store.page - 1)" :disabled="store.page <= 1" class="page-btn" title="Anterior">&lsaquo;</button>
        <button
          v-for="p in paginasVisibles"
          :key="p"
          @click="cambiarPagina(p)"
          :class="['page-btn', { active: p === store.page }]"
        >{{ p }}</button>
        <button @click="cambiarPagina(store.page + 1)" :disabled="store.page >= store.totalPages" class="page-btn" title="Siguiente">&rsaquo;</button>
        <button @click="cambiarPagina(store.totalPages)" :disabled="store.page >= store.totalPages" class="page-btn" title="Ultima">&raquo;</button>
      </div>
    </div>

    <ConfirmModal
      v-model="showConfirmInactivar"
      title="Inactivar Beneficiario"
      :message="`¿Esta seguro de inactivar a ${beneficiarioAInactivar?.nombreBeneficiario || ''}? Esta accion cambiara su estado a Inactivo.`"
      confirmText="Si, inactivar"
      variant="danger"
      @confirm="ejecutarInactivar"
    />
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useBeneficiariosStore } from '../stores/beneficiarios.js'
import { beneficiariosApi } from '../api/index.js'
import AlertMessage from '../components/AlertMessage.vue'
import ConfirmModal from '../components/ConfirmModal.vue'
import { formatCuenta } from '../composables/useFormato.js'
import {
  Users, Search, UserPlus, Eye, Pencil, Ban,
  CircleCheck, CircleX, CircleAlert, Inbox,
  FileSpreadsheet, FileText
} from 'lucide-vue-next'

const store = useBeneficiariosStore()
const busqueda = ref('')
const tamanioPagina = ref(20)
const alertMsg = ref('')
const alertType = ref('info')
const showConfirmInactivar = ref(false)
const beneficiarioAInactivar = ref(null)
const exportando = ref(false)
let debounceTimer = null

onMounted(() => store.listar())
onUnmounted(() => clearTimeout(debounceTimer))

function debounceBuscar() {
  clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => {
    store.page = 1
    store.listar({ q: busqueda.value })
  }, 300)
}

function cambiarPagina(p) {
  store.page = p
  store.listar({ q: busqueda.value })
}

function cambiarTamano() {
  store.pageSize = tamanioPagina.value
  store.page = 1
  store.listar({ q: busqueda.value })
}

const rangoInicio = computed(() => store.totalCount === 0 ? 0 : (store.page - 1) * store.pageSize + 1)
const rangoFin = computed(() => Math.min(store.page * store.pageSize, store.totalCount))

const paginasVisibles = computed(() => {
  const total = store.totalPages
  const current = store.page
  if (total <= 7) return Array.from({ length: total }, (_, i) => i + 1)
  const pages = new Set([1, total])
  for (let i = Math.max(2, current - 2); i <= Math.min(total - 1, current + 2); i++) {
    pages.add(i)
  }
  return Array.from(pages).sort((a, b) => a - b)
})

async function exportar(formato) {
  exportando.value = true
  try {
    const response = formato === 'excel'
      ? await beneficiariosApi.exportarExcel()
      : await beneficiariosApi.exportarCsv()
    const url = window.URL.createObjectURL(new Blob([response.data]))
    const link = document.createElement('a')
    link.href = url
    const filename = response.headers['content-disposition']
      ?.match(/filename="?(.+)"?/)?.[1]
      || `Beneficiarios.${formato === 'excel' ? 'xlsx' : 'csv'}`
    link.setAttribute('download', filename)
    document.body.appendChild(link)
    link.click()
    link.remove()
    window.URL.revokeObjectURL(url)
    alertType.value = 'success'
    alertMsg.value = `Archivo ${formato.toUpperCase()} exportado exitosamente.`
  } catch (e) {
    alertType.value = 'error'
    alertMsg.value = `Error al exportar: ${e.message}`
  } finally {
    exportando.value = false
  }
}

function confirmarInactivar(b) {
  beneficiarioAInactivar.value = b
  showConfirmInactivar.value = true
}

async function ejecutarInactivar() {
  const b = beneficiarioAInactivar.value
  if (!b) return
  const ok = await store.inactivar(b.rutBeneficiario)
  if (ok) {
    alertType.value = 'success'
    alertMsg.value = `Beneficiario ${b.nombreBeneficiario} fue inactivado exitosamente.`
    store.listar({ q: busqueda.value })
  } else {
    alertType.value = 'error'
    alertMsg.value = store.error || 'Error al inactivar el beneficiario.'
  }
  beneficiarioAInactivar.value = null
}
</script>

<style scoped>
.desglose-linea {
  font-size: 0.82rem;
  line-height: 1.5;
  white-space: nowrap;
}
.desglose-banco {
  color: var(--text-secondary, #64748b);
}
.desglose-cuenta {
  font-family: monospace;
  font-size: 0.78rem;
}
.desglose-total {
  margin-top: 2px;
  padding-top: 2px;
  border-top: 1px solid var(--border-color, #e2e8f0);
  font-size: 0.82rem;
}
</style>
