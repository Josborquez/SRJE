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
/* Estilos globales aplicados desde assets/styles.css */
/* Solo overrides especificos de este componente */
.actions { white-space: nowrap; }
</style>
