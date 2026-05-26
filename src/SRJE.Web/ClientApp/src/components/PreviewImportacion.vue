<template>
  <div class="preview-importacion">
    <div class="preview-stats">
      <span class="stat ok" :class="{ active: filtroActivo === 'ok' }" @click="toggleFiltro('ok')"><CheckCircle :size="14" /> OK: {{ stats.ok }}</span>
      <span class="stat nuevo" :class="{ active: filtroActivo === 'nuevo' }" @click="toggleFiltro('nuevo')"><PlusCircle :size="14" /> Nuevos: {{ stats.nuevos }}</span>
      <span class="stat advertencia" :class="{ active: filtroActivo === 'advertencia' }" @click="toggleFiltro('advertencia')"><AlertTriangle :size="14" /> Advertencias: {{ stats.advertencias }}</span>
      <span class="stat error" :class="{ active: filtroActivo === 'error' }" @click="toggleFiltro('error')"><XCircle :size="14" /> Errores: {{ stats.errores }}</span>
      <span v-if="stats.multicuenta > 0" class="stat multicuenta" :class="{ active: filtroActivo === 'multicuenta' }" @click="toggleFiltro('multicuenta')"><AlertTriangle :size="14" /> Multicuenta: {{ stats.multicuenta }}</span>
      <span class="stat total" :class="{ active: !filtroActivo }" @click="toggleFiltro(null)"><List :size="14" /> Total: {{ lineas.length }}</span>
    </div>

    <div class="preview-table-wrapper">
      <table class="preview-table">
        <thead>
          <tr>
            <th><input type="checkbox" v-model="selectAll" @change="toggleAll" /></th>
            <th>#</th>
            <th v-for="col in columnas" :key="col.key">{{ col.label }}</th>
            <th>Estado</th>
            <th>Mensaje</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="linea in lineasPaginadas"
            :key="linea.numeroLinea"
            :class="rowClass(linea)"
          >
            <td><input type="checkbox" v-model="linea.incluir" /></td>
            <td>{{ linea.numeroLinea }}</td>
            <td v-for="col in columnas" :key="col.key">
              <template v-if="isEditable(linea, col) && getOptions(linea, col).length">
                <select
                  :value="linea[col.key]"
                  @change="onSelectChange(linea, col, $event)"
                  class="inline-select"
                >
                  <option
                    v-if="linea[col.key] && !getOptions(linea, col).some(o => o.value === linea[col.key])"
                    :value="linea[col.key]"
                  >{{ linea[col.key] }}</option>
                  <option
                    v-for="opt in getOptions(linea, col)"
                    :key="opt.value"
                    :value="opt.value"
                  >{{ opt.label }}</option>
                </select>
              </template>
              <template v-else-if="isEditable(linea, col)">
                <input
                  v-model="linea[col.key]"
                  class="inline-edit"
                  :type="col.type || 'text'"
                />
              </template>
              <template v-else>
                {{ formatValue(linea[col.key], col) }}
              </template>
            </td>
            <td><span :class="'badge ' + linea.estadoLinea.toLowerCase()">{{ linea.estadoLinea }}</span></td>
            <td class="msg-col">{{ linea.mensaje }}</td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Paginacion del preview -->
    <div v-if="totalPages > 1" class="preview-pagination">
      <span class="pagination-info">
        Filas {{ rangoInicio }}-{{ rangoFin }} de {{ lineasFiltradas.length }}
        <template v-if="filtroActivo"> (filtrado de {{ lineas.length }})</template>
      </span>
      <div class="pagination-controls">
        <button @click="pagina = 1" :disabled="pagina <= 1" class="page-btn">&laquo;</button>
        <button @click="pagina--" :disabled="pagina <= 1" class="page-btn">&lsaquo;</button>
        <span class="page-current">{{ pagina }} / {{ totalPages }}</span>
        <button @click="pagina++" :disabled="pagina >= totalPages" class="page-btn">&rsaquo;</button>
        <button @click="pagina = totalPages" :disabled="pagina >= totalPages" class="page-btn">&raquo;</button>
        <select v-model="porPagina" class="page-size-select">
          <option :value="50">50</option>
          <option :value="100">100</option>
          <option :value="200">200</option>
          <option :value="99999">Todas</option>
        </select>
      </div>
    </div>

    <div class="preview-actions" v-if="lineas.length > 0">
      <button @click="$emit('confirmar')" class="btn btn-primary" :disabled="!hayLineasIncluidas">
        <Upload :size="16" /> Confirmar Importacion ({{ lineasIncluidas }} registros)
      </button>
      <button @click="$emit('cancelar')" class="btn btn-secondary">
        <X :size="16" /> Cancelar
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { formatearRut } from '../composables/useRut.js'
import { formatCuenta } from '../composables/useFormato.js'
import {
  CheckCircle, PlusCircle, AlertTriangle, XCircle,
  List, Upload, X
} from 'lucide-vue-next'

const props = defineProps({
  lineas: { type: Array, default: () => [] },
  columnas: { type: Array, default: () => [] }
})

defineEmits(['confirmar', 'cancelar'])

const selectAll = ref(true)
const pagina = ref(1)
const porPagina = ref(100)
const filtroActivo = ref(null)

const stats = computed(() => ({
  ok: props.lineas.filter(l => l.estadoLinea === 'OK').length,
  nuevos: props.lineas.filter(l => l.estadoLinea === 'NUEVO').length,
  advertencias: props.lineas.filter(l => l.estadoLinea === 'ADVERTENCIA').length,
  errores: props.lineas.filter(l => l.estadoLinea === 'ERROR').length,
  multicuenta: props.lineas.filter(l => l.esMulticuenta).length
}))

const lineasFiltradas = computed(() => {
  if (!filtroActivo.value) return props.lineas
  if (filtroActivo.value === 'multicuenta') return props.lineas.filter(l => l.esMulticuenta)
  return props.lineas.filter(l => l.estadoLinea.toLowerCase() === filtroActivo.value)
})

function toggleFiltro(tipo) {
  filtroActivo.value = filtroActivo.value === tipo ? null : tipo
  pagina.value = 1
}

const totalPages = computed(() => Math.ceil(lineasFiltradas.value.length / porPagina.value))
const rangoInicio = computed(() => lineasFiltradas.value.length ? (pagina.value - 1) * porPagina.value + 1 : 0)
const rangoFin = computed(() => Math.min(pagina.value * porPagina.value, lineasFiltradas.value.length))

const lineasPaginadas = computed(() => {
  const inicio = (pagina.value - 1) * porPagina.value
  return lineasFiltradas.value.slice(inicio, inicio + porPagina.value)
})

const lineasIncluidas = computed(() => props.lineas.filter(l => l.incluir).length)
const hayLineasIncluidas = computed(() => lineasIncluidas.value > 0)

function toggleAll() {
  props.lineas.forEach(l => { l.incluir = selectAll.value })
}

function isEditable(linea, col) {
  if (col.editable) return true
  if (col.editableWhen) return !!linea[col.editableWhen]
  return false
}

function getOptions(linea, col) {
  if (col.optionsFrom) return col.optionsFrom(linea) || []
  return col.options || []
}

function onSelectChange(linea, col, event) {
  const val = event.target.value
  linea[col.key] = col.numeric ? Number(val) : val
  if (col.onChange) col.onChange(linea, col.numeric ? Number(val) : val)
}

function rowClass(linea) {
  if (!linea.incluir) return 'row-excluido'
  if (linea.esMulticuenta) return 'row-advertencia row-multicuenta'
  return 'row-' + linea.estadoLinea.toLowerCase()
}

function formatValue(val, col) {
  if (col.format === 'rut' && val) return formatearRut(val, '')
  if (col.format === 'monto' && val) return '$' + Number(val).toLocaleString('es-CL')
  if (col.key === 'numeroCuenta' || col.key === 'ctaEstado' || col.key === 'ctaOtBanco') return formatCuenta(val)
  return val ?? ''
}
</script>

<style scoped>
.preview-stats {
  display: flex;
  gap: 0.75rem;
  margin-bottom: 1rem;
  flex-wrap: wrap;
}
.stat {
  padding: 0.35rem 0.85rem;
  border-radius: 20px;
  font-size: 0.82rem;
  font-weight: 500;
  display: inline-flex;
  align-items: center;
  gap: 0.3rem;
  cursor: pointer;
  transition: all 0.2s;
  user-select: none;
}
.stat:hover { opacity: 0.85; transform: scale(1.03); }
.stat.active { box-shadow: 0 0 0 2px currentColor; font-weight: 700; }
.stat.ok { background: #f0fdf4; color: #166534; }
.stat.nuevo { background: #f0f9ff; color: #075985; }
.stat.advertencia { background: #fffbeb; color: #92400e; }
.stat.error { background: #fef2f2; color: #991b1b; }
.stat.total { background: #f1f5f9; color: #334155; }
.stat.multicuenta { background: #fef3c7; color: #92400e; }

.preview-table-wrapper {
  overflow-x: auto;
  border-radius: 10px;
  border: 1px solid #e2e8f0;
}

.preview-table { width: 100%; border-collapse: collapse; font-size: 0.85rem; }
.preview-table th, .preview-table td { padding: 0.5rem 0.6rem; border-bottom: 1px solid #f1f5f9; text-align: left; }
.preview-table th { background: #f8fafc; position: sticky; top: 0; font-weight: 600; color: #64748b; font-size: 0.78rem; text-transform: uppercase; letter-spacing: 0.03em; }
.preview-table tbody tr:last-child td { border-bottom: none; }

.row-ok { background: #fff; }
.row-nuevo { background: #f0f9ff; }
.row-advertencia { background: #fffbeb; }
.row-error { background: #fef2f2; }
.row-excluido { background: #f8fafc; text-decoration: line-through; color: #94a3b8; }
.row-multicuenta { border-left: 3px solid #d97706; }

.badge { padding: 0.2rem 0.5rem; border-radius: 12px; font-size: 0.72rem; font-weight: 600; letter-spacing: 0.02em; }
.badge.ok { background: #16a34a; color: #fff; }
.badge.nuevo { background: #2563eb; color: #fff; }
.badge.advertencia { background: #d97706; color: #fff; }
.badge.error { background: #dc2626; color: #fff; }

.inline-edit {
  width: 100%;
  padding: 0.25rem 0.4rem;
  border: 1px solid #e2e8f0;
  border-radius: 4px;
  font-size: 0.85rem;
  transition: border-color 0.2s;
}
.inline-edit:focus { outline: none; border-color: #2563eb; box-shadow: 0 0 0 2px rgba(37,99,235,0.1); }
.inline-select {
  width: 100%;
  padding: 0.25rem 0.4rem;
  border: 1px solid #e2e8f0;
  border-radius: 4px;
  font-size: 0.85rem;
  background: #fff;
  cursor: pointer;
  transition: border-color 0.2s;
}
.inline-select:focus { outline: none; border-color: #2563eb; box-shadow: 0 0 0 2px rgba(37,99,235,0.1); }
.msg-col { font-size: 0.8rem; color: #64748b; max-width: 250px; }

/* Paginacion preview */
.preview-pagination {
  display: flex; align-items: center; justify-content: space-between;
  padding: 0.6rem 0; margin-top: 0.5rem; flex-wrap: wrap; gap: 0.5rem;
}
.pagination-info { font-size: 0.83rem; color: #64748b; }
.pagination-controls { display: flex; align-items: center; gap: 0.25rem; }
.page-btn {
  min-width: 28px; height: 28px; border: 1px solid #e2e8f0; border-radius: 6px;
  cursor: pointer; background: #fff; font-size: 0.8rem;
  display: inline-flex; align-items: center; justify-content: center;
  transition: all 0.2s;
}
.page-btn:hover:not(:disabled) { background: #f1f5f9; }
.page-btn:disabled { opacity: 0.35; cursor: not-allowed; }
.page-current { font-size: 0.83rem; color: #334155; padding: 0 0.3rem; }
.page-size-select { padding: 0.2rem; border: 1px solid #e2e8f0; border-radius: 6px; font-size: 0.8rem; margin-left: 0.3rem; }

.preview-actions {
  margin-top: 1.25rem;
  display: flex;
  gap: 0.6rem;
}
.btn {
  padding: 0.55rem 1.15rem; border: none; border-radius: 8px;
  cursor: pointer; font-size: 0.88rem; font-weight: 500;
  display: inline-flex; align-items: center; gap: 0.4rem;
  transition: all 0.2s;
}
.btn-primary { background: #2563eb; color: #fff; }
.btn-primary:hover { background: #1d4ed8; }
.btn-primary:disabled { background: #94a3b8; cursor: not-allowed; }
.btn-secondary { background: #f1f5f9; color: #334155; border: 1px solid #e2e8f0; }
.btn-secondary:hover { background: #e2e8f0; }
</style>
