<template>
  <div>
    <div class="page-header">
      <h1>Mantenedor de Bancos</h1>
      <p>Administrar catalogo de bancos del sistema</p>
    </div>

    <div class="toolbar">
      <div class="search-wrapper">
        <Search :size="18" class="search-icon" />
        <input
          v-model="busqueda"
          type="text"
          class="search-input"
          placeholder="Buscar por nombre o codigo..."
        />
      </div>
      <button class="btn btn-primary" @click="abrirFormulario()">
        <Plus :size="16" /> Nuevo Banco
      </button>
    </div>

    <div v-if="cargando" class="loading">Cargando bancos...</div>
    <div v-else-if="error" class="error">{{ error }}</div>

    <table v-else class="data-table">
      <thead>
        <tr>
          <th>Codigo</th>
          <th>Nombre</th>
          <th>Usa Cta. Otro Banco</th>
          <th>Estado</th>
          <th>Acciones</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="banco in bancosFiltrados" :key="banco.codBanco">
          <td><strong>{{ banco.codBanco }}</strong></td>
          <td>{{ banco.nombreBanco }}</td>
          <td>
            <span :class="banco.usaCtaOtBanco === 'S' ? 'estado estado-a' : 'estado estado-i'">
              {{ banco.usaCtaOtBanco === 'S' ? 'Si' : 'No' }}
            </span>
          </td>
          <td>
            <span :class="banco.activo === 'S' ? 'estado estado-a' : 'estado estado-i'">
              {{ banco.activo === 'S' ? 'Activo' : 'Inactivo' }}
            </span>
          </td>
          <td class="actions inline">
            <button class="btn-sm" @click="abrirFormulario(banco)">
              <Pencil :size="14" /> Editar
            </button>
            <button
              :class="['btn-sm', banco.activo === 'S' ? 'btn-sm-danger' : '']"
              @click="toggleEstado(banco)"
            >
              <Power :size="14" />
              {{ banco.activo === 'S' ? 'Inactivar' : 'Activar' }}
            </button>
          </td>
        </tr>
        <tr v-if="bancosFiltrados.length === 0">
          <td colspan="5" style="text-align:center; color: var(--text-muted); padding: 2rem;">
            No se encontraron bancos
          </td>
        </tr>
      </tbody>
    </table>

    <!-- Modal formulario -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="mostrarForm" class="modal-overlay" @click.self="cerrarFormulario">
          <div class="modal-dialog modal-form">
            <div class="modal-header">
              <Building2 :size="20" class="modal-icon primary" />
              <h3>{{ editando ? 'Editar Banco' : 'Nuevo Banco' }}</h3>
            </div>
            <form @submit.prevent="guardar">
              <div class="modal-body">
                <div class="form-grid">
                  <div class="field">
                    <label>Codigo *</label>
                    <input
                      v-model.number="form.codBanco"
                      type="number"
                      required
                      :disabled="editando"
                      min="1"
                    />
                  </div>
                  <div class="field">
                    <label>Nombre *</label>
                    <input v-model="form.nombreBanco" type="text" required maxlength="100" />
                  </div>
                  <div class="field">
                    <label>Usa Cta. Otro Banco</label>
                    <select v-model="form.usaCtaOtBanco">
                      <option value="S">Si</option>
                      <option value="N">No</option>
                    </select>
                  </div>
                  <div v-if="editando" class="field">
                    <label>Estado</label>
                    <select v-model="form.activo">
                      <option value="S">Activo</option>
                      <option value="N">Inactivo</option>
                    </select>
                  </div>
                </div>
                <div v-if="formError" class="error" style="margin-top:1rem;">{{ formError }}</div>
              </div>
              <div class="modal-footer">
                <button type="button" class="btn btn-secondary" @click="cerrarFormulario">Cancelar</button>
                <button type="submit" class="btn btn-primary" :disabled="guardando">
                  {{ guardando ? 'Guardando...' : 'Guardar' }}
                </button>
              </div>
            </form>
          </div>
        </div>
      </Transition>
    </Teleport>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { mantenedoresApi } from '../api/index.js'
import { Search, Plus, Pencil, Power, Building2 } from 'lucide-vue-next'

const bancos = ref([])
const cargando = ref(true)
const error = ref('')
const busqueda = ref('')

const mostrarForm = ref(false)
const editando = ref(false)
const guardando = ref(false)
const formError = ref('')
const form = ref({ codBanco: null, nombreBanco: '', usaCtaOtBanco: 'S', activo: 'S' })

const bancosFiltrados = computed(() => {
  if (!busqueda.value) return bancos.value
  const q = busqueda.value.toUpperCase()
  return bancos.value.filter(b =>
    b.nombreBanco.toUpperCase().includes(q) ||
    b.codBanco.toString().includes(q)
  )
})

async function cargar() {
  cargando.value = true
  error.value = ''
  try {
    const { data } = await mantenedoresApi.listarBancos()
    bancos.value = data
  } catch (e) {
    error.value = 'Error al cargar bancos'
  } finally {
    cargando.value = false
  }
}

function abrirFormulario(banco = null) {
  formError.value = ''
  if (banco) {
    editando.value = true
    form.value = { ...banco }
  } else {
    editando.value = false
    form.value = { codBanco: null, nombreBanco: '', usaCtaOtBanco: 'S', activo: 'S' }
  }
  mostrarForm.value = true
}

function cerrarFormulario() {
  mostrarForm.value = false
}

async function guardar() {
  guardando.value = true
  formError.value = ''
  try {
    if (editando.value) {
      await mantenedoresApi.actualizarBanco(form.value.codBanco, {
        nombreBanco: form.value.nombreBanco,
        usaCtaOtBanco: form.value.usaCtaOtBanco,
        activo: form.value.activo
      })
    } else {
      await mantenedoresApi.crearBanco({
        codBanco: form.value.codBanco,
        nombreBanco: form.value.nombreBanco,
        usaCtaOtBanco: form.value.usaCtaOtBanco
      })
    }
    cerrarFormulario()
    await cargar()
  } catch (e) {
    formError.value = e.response?.data?.error || 'Error al guardar'
  } finally {
    guardando.value = false
  }
}

async function toggleEstado(banco) {
  try {
    await mantenedoresApi.toggleBanco(banco.codBanco)
    await cargar()
  } catch (e) {
    error.value = 'Error al cambiar estado'
  }
}

onMounted(cargar)
</script>

<style scoped>
.modal-overlay {
  position: fixed; inset: 0; background: rgba(15, 23, 42, 0.5);
  display: flex; align-items: center; justify-content: center; z-index: 1000;
  backdrop-filter: blur(2px);
}
.modal-dialog.modal-form {
  background: #fff; border-radius: 14px;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.2);
  max-width: 540px; width: 92%; overflow: hidden;
}
.modal-header {
  padding: 1.25rem 1.5rem 0.75rem;
  display: flex; align-items: center; gap: 0.6rem;
  border-bottom: 1px solid #f1f5f9;
}
.modal-header h3 { margin: 0; font-size: 1.05rem; color: #1e293b; }
.modal-icon.primary { color: #2563eb; }
.modal-body { padding: 1.25rem 1.5rem; }
.modal-footer {
  padding: 0.85rem 1.5rem; display: flex; justify-content: flex-end; gap: 0.6rem;
  border-top: 1px solid #f1f5f9; background: #f8fafc;
}
.modal-fade-enter-active { transition: opacity 0.2s ease; }
.modal-fade-leave-active { transition: opacity 0.15s ease; }
.modal-fade-enter-from, .modal-fade-leave-to { opacity: 0; }
</style>
