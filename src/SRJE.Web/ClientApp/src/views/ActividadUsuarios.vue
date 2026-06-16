<template>
  <div>
    <div class="page-header">
      <h1>Actividad de Usuarios</h1>
      <p>Historial de accesos y archivos trabajados</p>
    </div>

    <div class="tabs">
      <button :class="['tab', { active: tab === 'accesos' }]" @click="cambiarTab('accesos')">
        <LogIn :size="16" /> Accesos
      </button>
      <button :class="['tab', { active: tab === 'cargas' }]" @click="cambiarTab('cargas')">
        <FileUp :size="16" /> Cargas
      </button>
    </div>

    <div class="filtros">
      <div class="field">
        <label>Usuario</label>
        <select v-model="filtros.usuario" @change="aplicarFiltros">
          <option value="">Todos</option>
          <option v-for="u in usuarios" :key="u.usuario" :value="u.usuario">{{ u.usuario }}</option>
        </select>
      </div>
      <template v-if="tab === 'accesos'">
        <div class="field">
          <label>Evento</label>
          <select v-model="filtros.evento" @change="aplicarFiltros">
            <option value="">Todos</option>
            <option value="login_ok">Login exitoso</option>
            <option value="login_fail">Login fallido</option>
            <option value="logout">Logout</option>
          </select>
        </div>
        <div class="field">
          <label>Desde</label>
          <input v-model="filtros.desde" type="date" @change="aplicarFiltros" />
        </div>
        <div class="field">
          <label>Hasta</label>
          <input v-model="filtros.hasta" type="date" @change="aplicarFiltros" />
        </div>
      </template>
    </div>

    <div v-if="cargando" class="loading">Cargando...</div>
    <div v-else-if="error" class="error">{{ error }}</div>

    <template v-else>
      <div class="table-responsive">
        <table v-if="tab === 'accesos'" class="data-table">
          <thead>
            <tr>
              <th>Usuario</th>
              <th>Evento</th>
              <th>IP</th>
              <th>Fecha</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="a in items" :key="a.id">
              <td><strong>{{ a.usuario }}</strong></td>
              <td>
                <span :class="'estado ' + claseEvento(a.evento)">{{ textoEvento(a.evento) }}</span>
              </td>
              <td>{{ a.ip || '-' }}</td>
              <td>{{ formatFechaHora(a.fecha) }}</td>
            </tr>
            <tr v-if="items.length === 0">
              <td colspan="4" style="text-align:center; color: var(--text-muted); padding: 2rem;">
                Sin registros de acceso
              </td>
            </tr>
          </tbody>
        </table>

        <table v-else class="data-table">
          <thead>
            <tr>
              <th>Tipo</th>
              <th>Archivo</th>
              <th>Fecha</th>
              <th>Estado</th>
              <th>Lineas</th>
              <th>Insertados</th>
              <th>Actualizados</th>
              <th>Errores</th>
              <th>Usuario</th>
              <th>IP</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="c in items" :key="c.id">
              <td>{{ c.tipoCarga }}</td>
              <td>{{ c.nombreArchivo || '-' }}</td>
              <td>{{ formatFechaHora(c.fechaInicio) }}</td>
              <td>{{ c.estado }}</td>
              <td class="text-center">{{ c.totalLineas ?? '-' }}</td>
              <td class="text-center">{{ c.registrosInsertados ?? '-' }}</td>
              <td class="text-center">{{ c.registrosActualizados ?? '-' }}</td>
              <td class="text-center">{{ c.registrosError ?? '-' }}</td>
              <td><strong>{{ c.usuario || '-' }}</strong></td>
              <td>{{ c.ipUsuario || '-' }}</td>
            </tr>
            <tr v-if="items.length === 0">
              <td colspan="10" style="text-align:center; color: var(--text-muted); padding: 2rem;">
                Sin cargas registradas
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div v-if="totalPages >= 1" class="pagination-bar">
        <div class="pagination-info">
          Mostrando {{ rangoInicio }}-{{ rangoFin }} de {{ totalCount }} registros
        </div>
        <div class="pagination-controls">
          <select v-model="pageSize" @change="cambiarTamano" class="page-size-select">
            <option :value="10">10 por pag.</option>
            <option :value="20">20 por pag.</option>
            <option :value="50">50 por pag.</option>
            <option :value="100">100 por pag.</option>
          </select>
          <button @click="cambiarPagina(1)" :disabled="page <= 1" class="page-btn" title="Primera">&laquo;</button>
          <button @click="cambiarPagina(page - 1)" :disabled="page <= 1" class="page-btn" title="Anterior">&lsaquo;</button>
          <button
            v-for="p in paginasVisibles"
            :key="p"
            @click="cambiarPagina(p)"
            :class="['page-btn', { active: p === page }]"
          >{{ p }}</button>
          <button @click="cambiarPagina(page + 1)" :disabled="page >= totalPages" class="page-btn" title="Siguiente">&rsaquo;</button>
          <button @click="cambiarPagina(totalPages)" :disabled="page >= totalPages" class="page-btn" title="Ultima">&raquo;</button>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { usuariosApi } from '../api/index.js'
import { LogIn, FileUp } from 'lucide-vue-next'

const tab = ref('accesos')
const usuarios = ref([])
const items = ref([])
const cargando = ref(true)
const error = ref('')
const page = ref(1)
const pageSize = ref(20)
const totalCount = ref(0)
const totalPages = ref(0)
const filtros = ref({ usuario: '', evento: '', desde: '', hasta: '' })

function textoEvento(evento) {
  return { login_ok: 'Login exitoso', login_fail: 'Login fallido', logout: 'Logout' }[evento] || evento
}

function claseEvento(evento) {
  return { login_ok: 'estado-a', login_fail: 'estado-i', logout: 'estado-logout' }[evento] || ''
}

function formatFechaHora(fecha) {
  return fecha ? new Date(fecha).toLocaleString('es-CL') : '-'
}

async function cargar() {
  cargando.value = true
  error.value = ''
  try {
    const params = {
      usuario: filtros.value.usuario || undefined,
      page: page.value,
      pageSize: pageSize.value
    }
    let data
    if (tab.value === 'accesos') {
      params.evento = filtros.value.evento || undefined
      params.desde = filtros.value.desde || undefined
      params.hasta = filtros.value.hasta || undefined
      ;({ data } = await usuariosApi.accesos(params))
    } else {
      ;({ data } = await usuariosApi.cargas(params))
    }
    items.value = data.items
    totalCount.value = data.totalCount
    totalPages.value = data.totalPages
  } catch (e) {
    error.value = 'Error al cargar la actividad'
  } finally {
    cargando.value = false
  }
}

function cambiarTab(nueva) {
  tab.value = nueva
  page.value = 1
  cargar()
}

function aplicarFiltros() {
  page.value = 1
  cargar()
}

function cambiarPagina(p) {
  page.value = p
  cargar()
}

function cambiarTamano() {
  page.value = 1
  cargar()
}

const rangoInicio = computed(() => totalCount.value === 0 ? 0 : (page.value - 1) * pageSize.value + 1)
const rangoFin = computed(() => Math.min(page.value * pageSize.value, totalCount.value))

const paginasVisibles = computed(() => {
  const total = totalPages.value
  const current = page.value
  if (total <= 7) return Array.from({ length: total }, (_, i) => i + 1)
  const pages = new Set([1, total])
  for (let i = Math.max(2, current - 2); i <= Math.min(total - 1, current + 2); i++) {
    pages.add(i)
  }
  return Array.from(pages).sort((a, b) => a - b)
})

onMounted(async () => {
  cargar()
  try {
    const { data } = await usuariosApi.listar()
    usuarios.value = data
  } catch { /* el filtro de usuario queda vacio */ }
})
</script>

<style scoped>
.tabs {
  display: flex;
  gap: 0.5rem;
  margin-bottom: 1rem;
  border-bottom: 1px solid var(--border-color, #e2e8f0);
}
.tab {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  padding: 0.6rem 1.1rem;
  background: none;
  border: none;
  border-bottom: 2px solid transparent;
  color: var(--text-secondary, #64748b);
  cursor: pointer;
  font-size: 0.9rem;
}
.tab.active {
  color: #2563eb;
  border-bottom-color: #2563eb;
  font-weight: 600;
}
.filtros {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem;
  margin-bottom: 1rem;
  align-items: flex-end;
}
.filtros .field {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}
.filtros label {
  font-size: 0.8rem;
  color: var(--text-secondary, #64748b);
}
.filtros select,
.filtros input {
  padding: 0.45rem 0.6rem;
  border: 1px solid var(--border-color, #cbd5e1);
  border-radius: 8px;
  font-size: 0.88rem;
  background: #fff;
}
.estado-logout {
  background: #e2e8f0;
  color: #475569;
}
</style>
