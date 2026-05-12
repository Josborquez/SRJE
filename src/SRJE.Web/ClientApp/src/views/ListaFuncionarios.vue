<template>
  <div class="lista-funcionarios">
    <div class="page-header">
      <h1><Users :size="24" /> Funcionarios</h1>
    </div>

    <div v-if="store.stats" class="stats-bar">
      <div class="stat-card">
        <Users :size="18" class="stat-icon" />
        <div class="stat-content">
          <span class="stat-value">{{ store.stats.totalFuncionarios?.toLocaleString('es-CL') || 0 }}</span>
          <span class="stat-label">Total Funcionarios</span>
        </div>
      </div>
      <div class="stat-card stat-activos">
        <CircleCheck :size="18" class="stat-icon" />
        <div class="stat-content">
          <span class="stat-value">{{ store.stats.totalActivos?.toLocaleString('es-CL') || 0 }}</span>
          <span class="stat-label">Activos</span>
        </div>
      </div>
      <div class="stat-card">
        <DollarSign :size="18" class="stat-icon" />
        <div class="stat-content">
          <span class="stat-value">${{ store.stats.montoMensualTotal?.toLocaleString('es-CL') || 0 }}</span>
          <span class="stat-label">Monto Mensual Total</span>
        </div>
      </div>
      <div class="stat-card">
        <Calendar :size="18" class="stat-icon" />
        <div class="stat-content">
          <span class="stat-value">{{ store.stats.periodoActual || '-' }}</span>
          <span class="stat-label">Periodo Actual</span>
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
        <select v-model="filtroEstado" @change="filtrar" class="page-size-select">
          <option value="">Todos</option>
          <option value="S">Activos</option>
          <option value="N">Inactivos</option>
        </select>
      </div>
    </div>

    <AlertMessage v-if="alertMsg" :message="alertMsg" :type="alertType" @close="alertMsg = ''" />

    <div v-if="store.loading" class="loading">Cargando funcionarios...</div>
    <div v-if="store.error" class="error"><CircleAlert :size="16" /> {{ store.error }}</div>

    <div v-if="!store.loading && !store.items.length && !store.error" class="empty-state">
      <Inbox :size="48" class="empty-icon" />
      <p>No se encontraron funcionarios.</p>
      <small>Intenta con otra busqueda o ajusta los filtros.</small>
    </div>

    <div v-if="store.items.length" class="table-responsive">
    <table class="data-table">
      <thead>
        <tr>
          <th>RUT</th>
          <th>Apellido Paterno</th>
          <th>Apellido Materno</th>
          <th>Nombres</th>
          <th>Beneficiarios</th>
          <th>Monto Total</th>
          <th>Estado</th>
          <th>Acciones</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="f in store.items" :key="f.rutFuncionario">
          <td>{{ f.rutFormateado }}</td>
          <td>{{ f.apellidoPaterno || '-' }}</td>
          <td>{{ f.apellidoMaterno || '-' }}</td>
          <td>{{ f.nombres || '-' }}</td>
          <td class="text-center">{{ f.cantidadBeneficiarios || 0 }}</td>
          <td class="text-right">{{ f.montoTotal ? '$' + f.montoTotal.toLocaleString('es-CL') : '-' }}</td>
          <td>
            <span :class="'estado estado-' + (f.activo === 'S' ? 'a' : 'i')">
              <CircleCheck v-if="f.activo === 'S'" :size="13" />
              <CircleX v-else :size="13" />
              {{ f.activo === 'S' ? 'Activo' : 'Inactivo' }}
            </span>
          </td>
          <td class="actions inline">
            <router-link :to="`/funcionarios/${f.rutFuncionario}`" class="btn-sm">
              <Eye :size="14" /> Ver
            </router-link>
            <router-link :to="`/funcionarios/${f.rutFuncionario}/editar`" class="btn-sm">
              <Pencil :size="14" /> Editar
            </router-link>
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
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useFuncionariosStore } from '../stores/funcionarios.js'
import AlertMessage from '../components/AlertMessage.vue'
import {
  Users, Search, Eye, Pencil,
  CircleCheck, CircleX, CircleAlert, Inbox,
  DollarSign, Calendar
} from 'lucide-vue-next'

const store = useFuncionariosStore()
const busqueda = ref('')
const filtroEstado = ref('')
const tamanioPagina = ref(20)
const alertMsg = ref('')
const alertType = ref('info')
let debounceTimer = null

onMounted(() => {
  store.cargarStats()
  store.listar()
})
onUnmounted(() => clearTimeout(debounceTimer))

function buildParams() {
  const params = {}
  if (busqueda.value) params.q = busqueda.value
  if (filtroEstado.value) params.activo = filtroEstado.value
  return params
}

function debounceBuscar() {
  clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => {
    store.page = 1
    store.listar(buildParams())
  }, 300)
}

function filtrar() {
  store.page = 1
  store.listar(buildParams())
}

function cambiarPagina(p) {
  store.page = p
  store.listar(buildParams())
}

function cambiarTamano() {
  store.pageSize = tamanioPagina.value
  store.page = 1
  store.listar(buildParams())
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
</script>
