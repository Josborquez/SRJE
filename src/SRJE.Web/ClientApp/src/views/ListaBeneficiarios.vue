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

    <div v-if="store.loading" class="loading">Cargando...</div>
    <div v-if="store.error" class="error">{{ store.error }}</div>

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
          <td>{{ b.nombreBanco || b.codBanco }}</td>
          <td>{{ b.ctaEstado || b.ctaOtBanco || '-' }}</td>
          <td><span :class="'estado estado-' + b.estado.toLowerCase()">{{ b.estado }}</span></td>
          <td class="actions">
            <router-link :to="`/beneficiarios/${b.rutBeneficiario}`" class="btn-sm">Ver</router-link>
            <router-link :to="`/beneficiarios/${b.rutBeneficiario}/editar`" class="btn-sm">Editar</router-link>
          </td>
        </tr>
      </tbody>
    </table>

    <div v-if="store.totalPages > 1" class="pagination">
      <button @click="cambiarPagina(store.page - 1)" :disabled="store.page <= 1">Anterior</button>
      <span>Pagina {{ store.page }} de {{ store.totalPages }}</span>
      <button @click="cambiarPagina(store.page + 1)" :disabled="store.page >= store.totalPages">Siguiente</button>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useBeneficiariosStore } from '../stores/beneficiarios.js'

const store = useBeneficiariosStore()
const busqueda = ref('')
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
.btn-sm { padding: 0.25rem 0.5rem; font-size: 0.8rem; color: #1976d2; text-decoration: none; }
.btn-sm:hover { text-decoration: underline; }
.actions { white-space: nowrap; }
.pagination { display: flex; align-items: center; gap: 1rem; margin-top: 1rem; justify-content: center; }
.pagination button { padding: 0.4rem 0.8rem; border: 1px solid #ccc; border-radius: 4px; cursor: pointer; background: #fff; }
.pagination button:disabled { opacity: 0.5; cursor: not-allowed; }
.loading { padding: 2rem; text-align: center; color: #666; }
.error { padding: 1rem; background: #fce4ec; color: #c62828; border-radius: 4px; margin: 1rem 0; }
</style>
