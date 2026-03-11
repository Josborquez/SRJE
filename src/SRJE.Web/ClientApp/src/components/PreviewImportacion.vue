<template>
  <div class="preview-importacion">
    <div class="preview-stats">
      <span class="stat ok">OK: {{ stats.ok }}</span>
      <span class="stat nuevo">Nuevos: {{ stats.nuevos }}</span>
      <span class="stat advertencia">Advertencias: {{ stats.advertencias }}</span>
      <span class="stat error">Errores: {{ stats.errores }}</span>
      <span class="stat total">Total: {{ lineas.length }}</span>
    </div>

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
            <template v-if="col.editable">
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

    <!-- Paginacion del preview -->
    <div v-if="totalPages > 1" class="preview-pagination">
      <span class="pagination-info">
        Filas {{ rangoInicio }}-{{ rangoFin }} de {{ lineas.length }}
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
        Confirmar Importacion ({{ lineasIncluidas }} registros)
      </button>
      <button @click="$emit('cancelar')" class="btn btn-secondary">Cancelar</button>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { formatearRut } from '../composables/useRut.js'
import { formatCuenta } from '../composables/useFormato.js'

const props = defineProps({
  lineas: { type: Array, default: () => [] },
  columnas: { type: Array, default: () => [] }
})

defineEmits(['confirmar', 'cancelar'])

const selectAll = ref(true)
const pagina = ref(1)
const porPagina = ref(100)

const stats = computed(() => ({
  ok: props.lineas.filter(l => l.estadoLinea === 'OK').length,
  nuevos: props.lineas.filter(l => l.estadoLinea === 'NUEVO').length,
  advertencias: props.lineas.filter(l => l.estadoLinea === 'ADVERTENCIA').length,
  errores: props.lineas.filter(l => l.estadoLinea === 'ERROR').length
}))

const totalPages = computed(() => Math.ceil(props.lineas.length / porPagina.value))
const rangoInicio = computed(() => (pagina.value - 1) * porPagina.value + 1)
const rangoFin = computed(() => Math.min(pagina.value * porPagina.value, props.lineas.length))

const lineasPaginadas = computed(() => {
  const inicio = (pagina.value - 1) * porPagina.value
  return props.lineas.slice(inicio, inicio + porPagina.value)
})

const lineasIncluidas = computed(() => props.lineas.filter(l => l.incluir).length)
const hayLineasIncluidas = computed(() => lineasIncluidas.value > 0)

function toggleAll() {
  props.lineas.forEach(l => { l.incluir = selectAll.value })
}

function rowClass(linea) {
  if (!linea.incluir) return 'row-excluido'
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
.preview-stats { display: flex; gap: 1rem; margin-bottom: 1rem; flex-wrap: wrap; }
.stat { padding: 0.3rem 0.8rem; border-radius: 4px; font-size: 0.85rem; font-weight: 500; }
.stat.ok { background: #e2efda; color: #2e7d32; }
.stat.nuevo { background: #dce6f1; color: #1565c0; }
.stat.advertencia { background: #fff2cc; color: #e65100; }
.stat.error { background: #fce4d6; color: #c62828; }
.stat.total { background: #eee; }

.preview-table { width: 100%; border-collapse: collapse; font-size: 0.85rem; }
.preview-table th, .preview-table td { padding: 0.4rem 0.5rem; border: 1px solid #ddd; text-align: left; }
.preview-table th { background: #f0f0f0; position: sticky; top: 0; }

.row-ok { background: #fff; }
.row-nuevo { background: #dce6f1; }
.row-advertencia { background: #fff2cc; }
.row-error { background: #fce4d6; }
.row-excluido { background: #f2f2f2; text-decoration: line-through; color: #999; }

.badge { padding: 0.15rem 0.4rem; border-radius: 3px; font-size: 0.75rem; font-weight: bold; }
.badge.ok { background: #4caf50; color: #fff; }
.badge.nuevo { background: #2196f3; color: #fff; }
.badge.advertencia { background: #ff9800; color: #fff; }
.badge.error { background: #f44336; color: #fff; }

.inline-edit { width: 100%; padding: 0.2rem; border: 1px solid #ccc; border-radius: 3px; font-size: 0.85rem; }
.msg-col { font-size: 0.8rem; color: #666; max-width: 250px; }

/* Paginacion preview */
.preview-pagination {
  display: flex; align-items: center; justify-content: space-between;
  padding: 0.5rem 0; margin-top: 0.5rem; flex-wrap: wrap; gap: 0.5rem;
}
.pagination-info { font-size: 0.83rem; color: #666; }
.pagination-controls { display: flex; align-items: center; gap: 0.25rem; }
.page-btn {
  min-width: 28px; height: 28px; border: 1px solid #ddd; border-radius: 4px;
  cursor: pointer; background: #fff; font-size: 0.8rem;
  display: inline-flex; align-items: center; justify-content: center;
}
.page-btn:hover:not(:disabled) { background: #f0f0f0; }
.page-btn:disabled { opacity: 0.4; cursor: not-allowed; }
.page-current { font-size: 0.83rem; color: #333; padding: 0 0.3rem; }
.page-size-select { padding: 0.2rem; border: 1px solid #ccc; border-radius: 4px; font-size: 0.8rem; margin-left: 0.3rem; }

.preview-actions { margin-top: 1rem; display: flex; gap: 0.5rem; }
.btn { padding: 0.6rem 1.2rem; border: none; border-radius: 4px; cursor: pointer; font-size: 0.9rem; }
.btn-primary { background: #1976d2; color: #fff; }
.btn-primary:hover { background: #1565c0; }
.btn-primary:disabled { background: #ccc; cursor: not-allowed; }
.btn-secondary { background: #eee; color: #333; }
</style>
