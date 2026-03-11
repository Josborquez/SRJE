<template>
  <div class="lista-beneficiarios">
    <h1>Beneficiarios</h1>

    <div class="toolbar">
      <input
        v-model="busqueda"
        @input="debounceBuscar"
        placeholder="Buscar por nombre o RUT..."
        class="search-input"
      />
      <router-link to="/beneficiarios/nuevo" class="btn btn-primary">Nuevo Beneficiario</router-link>
    </div>

    <AlertMessage v-if="alertMsg" :message="alertMsg" :type="alertType" @close="alertMsg = ''" />

    <div v-if="store.loading" class="loading">Cargando...</div>
    <div v-if="store.error" class="error">{{ store.error }}</div>

    <div v-if="!store.loading && !store.items.length && !store.error" class="empty-state">
      No se encontraron beneficiarios.
    </div>

    <table v-if="store.items.length" class="data-table">
      <thead>
        <tr>
          <th>RUT</th>
          <th>Nombre</th>
          <th>RUT Funcionario</th>
          <th>Nombre Funcionario</th>
          <th>Banco</th>
          <th>Cuenta</th>
          <th>Estado</th>
          <th>Acciones</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="b in store.items" :key="b.id">
          <td>{{ b.rutFormateado }}</td>
          <td>{{ b.nombreBeneficiario }}</td>
          <td>{{ b.rutFuncionarioFormateado || '-' }}</td>
          <td>{{ b.nombreFuncionario || '-' }}</td>
          <td>{{ b.nombreBanco || b.codBanco || '-' }}</td>
          <td>{{ formatCuenta(b.ctaEstado || b.ctaOtBanco) }}</td>
          <td><span :class="'estado estado-' + b.estado.toLowerCase()">{{ b.estado === 'A' ? 'Activo' : 'Inactivo' }}</span></td>
          <td class="actions">
            <router-link :to="`/beneficiarios/${b.rutBeneficiario}`" class="btn-sm">Ver</router-link>
            <router-link :to="`/beneficiarios/${b.rutBeneficiario}/editar`" class="btn-sm">Editar</router-link>
            <button v-if="b.estado === 'A'" class="btn-sm btn-sm-danger" @click="confirmarInactivar(b)">Inactivar</button>
          </td>
        </tr>
      </tbody>
    </table>

    <!-- Paginacion mejorada -->
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
      :message="`Esta seguro de inactivar a ${beneficiarioAInactivar?.nombreBeneficiario || ''}? Esta accion cambiara su estado a Inactivo.`"
      confirmText="Si, inactivar"
      variant="danger"
      @confirm="ejecutarInactivar"
    />
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useBeneficiariosStore } from '../stores/beneficiarios.js'
import AlertMessage from '../components/AlertMessage.vue'
import ConfirmModal from '../components/ConfirmModal.vue'
import { formatCuenta } from '../composables/useFormato.js'

const store = useBeneficiariosStore()
const busqueda = ref('')
const tamanioPagina = ref(20)
const alertMsg = ref('')
const alertType = ref('info')
const showConfirmInactivar = ref(false)
const beneficiarioAInactivar = ref(null)
let debounceTimer = null

onMounted(() => store.listar())

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
.toolbar { display: flex; gap: 1rem; margin: 1rem 0; align-items: center; }
.search-input { flex: 1; padding: 0.5rem; border: 1px solid #ccc; border-radius: 4px; font-size: 0.95rem; }
.btn { padding: 0.5rem 1rem; border: none; border-radius: 4px; cursor: pointer; text-decoration: none; }
.btn-primary { background: #1976d2; color: #fff; }
.data-table { width: 100%; border-collapse: collapse; background: #fff; border-radius: 8px; overflow: hidden; }
.data-table th, .data-table td { padding: 0.6rem 0.8rem; border-bottom: 1px solid #eee; text-align: left; }
.data-table th { background: #f5f5f5; font-weight: 600; }
.data-table tr:hover { background: #f9f9f9; }
.estado { padding: 0.15rem 0.5rem; border-radius: 3px; font-size: 0.8rem; }
.estado-a { background: #e8f5e9; color: #2e7d32; }
.estado-i { background: #ffebee; color: #c62828; }
.btn-sm { padding: 0.25rem 0.5rem; font-size: 0.8rem; color: #1976d2; text-decoration: none; background: none; border: none; cursor: pointer; }
.btn-sm:hover { text-decoration: underline; }
.btn-sm-danger { color: #c62828; }
.actions { white-space: nowrap; }
.empty-state { padding: 2rem; text-align: center; color: #999; background: #fff; border-radius: 8px; }

/* Paginacion */
.pagination-bar {
  display: flex; align-items: center; justify-content: space-between;
  margin-top: 1rem; padding: 0.75rem 0; flex-wrap: wrap; gap: 0.5rem;
}
.pagination-info { font-size: 0.85rem; color: #666; }
.pagination-controls { display: flex; align-items: center; gap: 0.25rem; }
.page-size-select {
  padding: 0.35rem 0.5rem; border: 1px solid #ccc; border-radius: 4px;
  font-size: 0.85rem; margin-right: 0.5rem; background: #fff;
}
.page-btn {
  min-width: 32px; height: 32px; padding: 0 0.4rem; border: 1px solid #ddd;
  border-radius: 4px; cursor: pointer; background: #fff; font-size: 0.85rem;
  display: inline-flex; align-items: center; justify-content: center;
}
.page-btn:hover:not(:disabled):not(.active) { background: #f0f0f0; }
.page-btn.active { background: #1976d2; color: #fff; border-color: #1976d2; }
.page-btn:disabled { opacity: 0.4; cursor: not-allowed; }

.loading { padding: 2rem; text-align: center; color: #666; }
.error { padding: 1rem; background: #fce4ec; color: #c62828; border-radius: 4px; margin: 1rem 0; }
</style>
