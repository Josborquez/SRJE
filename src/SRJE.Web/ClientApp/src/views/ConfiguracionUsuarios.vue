<template>
  <div>
    <div class="page-header">
      <h1>Usuarios del Sistema</h1>
      <p>Administrar cuentas de acceso a SRJE</p>
    </div>

    <div class="toolbar">
      <div class="search-wrapper">
        <Search :size="18" class="search-icon" />
        <input
          v-model="busqueda"
          type="text"
          class="search-input"
          placeholder="Buscar por usuario o nombre..."
        />
      </div>
      <button class="btn btn-primary" @click="abrirFormulario()">
        <Plus :size="16" /> Nuevo Usuario
      </button>
    </div>

    <AlertMessage v-if="alertMsg" :message="alertMsg" :type="alertType" @close="alertMsg = ''" />

    <div v-if="cargando" class="loading">Cargando usuarios...</div>
    <div v-else-if="error" class="error">{{ error }}</div>

    <table v-else class="data-table">
      <thead>
        <tr>
          <th>Usuario</th>
          <th>Nombre</th>
          <th>Rol</th>
          <th>Estado</th>
          <th>Fecha Creacion</th>
          <th>Acciones</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="u in usuariosFiltrados" :key="u.usuario">
          <td><strong>{{ u.usuario }}</strong></td>
          <td>{{ u.nombreCompleto }}</td>
          <td>{{ u.rol }}</td>
          <td>
            <span :class="u.estado === 'A' ? 'estado estado-a' : 'estado estado-i'">
              {{ u.estado === 'A' ? 'Activo' : 'Inactivo' }}
            </span>
          </td>
          <td>{{ formatFecha(u.fechaCreacion) }}</td>
          <td class="actions inline">
            <button class="btn-sm" @click="abrirFormulario(u)">
              <Pencil :size="14" /> Editar
            </button>
            <button class="btn-sm" @click="abrirPassword(u)">
              <KeyRound :size="14" /> Contraseña
            </button>
            <button
              v-if="u.usuario !== authStore.usuario?.usuario"
              :class="['btn-sm', u.estado === 'A' ? 'btn-sm-danger' : '']"
              @click="confirmarToggle(u)"
            >
              <Power :size="14" />
              {{ u.estado === 'A' ? 'Inactivar' : 'Activar' }}
            </button>
          </td>
        </tr>
        <tr v-if="usuariosFiltrados.length === 0">
          <td colspan="6" style="text-align:center; color: var(--text-muted); padding: 2rem;">
            No se encontraron usuarios
          </td>
        </tr>
      </tbody>
    </table>

    <!-- Modal crear/editar -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="mostrarForm" class="modal-overlay" @click.self="mostrarForm = false">
          <div class="modal-dialog modal-form">
            <div class="modal-header">
              <UserCog :size="20" class="modal-icon primary" />
              <h3>{{ editando ? 'Editar Usuario' : 'Nuevo Usuario' }}</h3>
            </div>
            <form @submit.prevent="guardar">
              <div class="modal-body">
                <div class="form-grid">
                  <div class="field">
                    <label>Usuario *</label>
                    <input
                      v-model="form.usuario"
                      type="text"
                      required
                      :disabled="editando"
                      maxlength="50"
                      autocomplete="off"
                    />
                  </div>
                  <div class="field">
                    <label>Nombre completo *</label>
                    <input v-model="form.nombreCompleto" type="text" required maxlength="100" />
                  </div>
                  <div class="field">
                    <label>Rol *</label>
                    <select v-model="form.rol" :disabled="editando && form.usuario === authStore.usuario?.usuario">
                      <option value="admin">admin</option>
                      <option value="operador">operador</option>
                      <option value="consulta">consulta</option>
                    </select>
                  </div>
                  <div v-if="!editando" class="field">
                    <label>Contraseña * (min. 8 caracteres)</label>
                    <input v-model="form.password" type="password" required minlength="8" autocomplete="new-password" />
                  </div>
                </div>
                <div v-if="formError" class="error" style="margin-top:1rem;">{{ formError }}</div>
              </div>
              <div class="modal-footer">
                <button type="button" class="btn btn-secondary" @click="mostrarForm = false">Cancelar</button>
                <button type="submit" class="btn btn-primary" :disabled="guardando">
                  {{ guardando ? 'Guardando...' : 'Guardar' }}
                </button>
              </div>
            </form>
          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- Modal restablecer contraseña -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="mostrarPassword" class="modal-overlay" @click.self="mostrarPassword = false">
          <div class="modal-dialog modal-form">
            <div class="modal-header">
              <KeyRound :size="20" class="modal-icon primary" />
              <h3>Restablecer contraseña: {{ usuarioPassword }}</h3>
            </div>
            <form @submit.prevent="guardarPassword">
              <div class="modal-body">
                <div class="form-grid">
                  <div class="field">
                    <label>Nueva contraseña * (min. 8 caracteres)</label>
                    <input v-model="passwordForm.password" type="password" required minlength="8" autocomplete="new-password" />
                  </div>
                  <div class="field">
                    <label>Confirmar contraseña *</label>
                    <input v-model="passwordForm.confirmacion" type="password" required minlength="8" autocomplete="new-password" />
                  </div>
                </div>
                <div v-if="passwordError" class="error" style="margin-top:1rem;">{{ passwordError }}</div>
              </div>
              <div class="modal-footer">
                <button type="button" class="btn btn-secondary" @click="mostrarPassword = false">Cancelar</button>
                <button type="submit" class="btn btn-primary" :disabled="guardando">
                  {{ guardando ? 'Guardando...' : 'Restablecer' }}
                </button>
              </div>
            </form>
          </div>
        </div>
      </Transition>
    </Teleport>

    <ConfirmModal
      v-model="showConfirmToggle"
      :title="usuarioToggle?.estado === 'A' ? 'Inactivar Usuario' : 'Activar Usuario'"
      :message="`¿Esta seguro de ${usuarioToggle?.estado === 'A' ? 'inactivar' : 'activar'} al usuario ${usuarioToggle?.usuario || ''}?`"
      :confirm-text="usuarioToggle?.estado === 'A' ? 'Si, inactivar' : 'Si, activar'"
      :variant="usuarioToggle?.estado === 'A' ? 'danger' : 'primary'"
      @confirm="ejecutarToggle"
    />
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { usuariosApi } from '../api/index.js'
import { useAuthStore } from '../stores/auth.js'
import AlertMessage from '../components/AlertMessage.vue'
import ConfirmModal from '../components/ConfirmModal.vue'
import { Search, Plus, Pencil, Power, UserCog, KeyRound } from 'lucide-vue-next'

const authStore = useAuthStore()
const usuarios = ref([])
const cargando = ref(true)
const error = ref('')
const busqueda = ref('')
const alertMsg = ref('')
const alertType = ref('info')

const mostrarForm = ref(false)
const editando = ref(false)
const guardando = ref(false)
const formError = ref('')
const form = ref({ usuario: '', nombreCompleto: '', rol: 'consulta', password: '' })

const mostrarPassword = ref(false)
const usuarioPassword = ref('')
const passwordError = ref('')
const passwordForm = ref({ password: '', confirmacion: '' })

const showConfirmToggle = ref(false)
const usuarioToggle = ref(null)

const usuariosFiltrados = computed(() => {
  if (!busqueda.value) return usuarios.value
  const q = busqueda.value.toLowerCase()
  return usuarios.value.filter(u =>
    u.usuario.toLowerCase().includes(q) ||
    u.nombreCompleto.toLowerCase().includes(q)
  )
})

function formatFecha(fecha) {
  return fecha ? new Date(fecha).toLocaleDateString('es-CL') : '-'
}

async function cargar() {
  cargando.value = true
  error.value = ''
  try {
    const { data } = await usuariosApi.listar()
    usuarios.value = data
  } catch (e) {
    error.value = 'Error al cargar usuarios'
  } finally {
    cargando.value = false
  }
}

function abrirFormulario(usuario = null) {
  formError.value = ''
  if (usuario) {
    editando.value = true
    form.value = { usuario: usuario.usuario, nombreCompleto: usuario.nombreCompleto, rol: usuario.rol, password: '' }
  } else {
    editando.value = false
    form.value = { usuario: '', nombreCompleto: '', rol: 'consulta', password: '' }
  }
  mostrarForm.value = true
}

async function guardar() {
  guardando.value = true
  formError.value = ''
  try {
    if (editando.value) {
      await usuariosApi.actualizar(form.value.usuario, {
        nombreCompleto: form.value.nombreCompleto,
        rol: form.value.rol
      })
    } else {
      await usuariosApi.crear(form.value)
    }
    mostrarForm.value = false
    alertType.value = 'success'
    alertMsg.value = editando.value ? 'Usuario actualizado.' : 'Usuario creado.'
    await cargar()
  } catch (e) {
    formError.value = e.response?.data?.error || 'Error al guardar'
  } finally {
    guardando.value = false
  }
}

function abrirPassword(usuario) {
  usuarioPassword.value = usuario.usuario
  passwordForm.value = { password: '', confirmacion: '' }
  passwordError.value = ''
  mostrarPassword.value = true
}

async function guardarPassword() {
  if (passwordForm.value.password !== passwordForm.value.confirmacion) {
    passwordError.value = 'Las contraseñas no coinciden'
    return
  }
  guardando.value = true
  passwordError.value = ''
  try {
    await usuariosApi.cambiarPassword(usuarioPassword.value, { password: passwordForm.value.password })
    mostrarPassword.value = false
    alertType.value = 'success'
    alertMsg.value = `Contraseña de ${usuarioPassword.value} restablecida.`
  } catch (e) {
    passwordError.value = e.response?.data?.error || 'Error al restablecer contraseña'
  } finally {
    guardando.value = false
  }
}

function confirmarToggle(usuario) {
  usuarioToggle.value = usuario
  showConfirmToggle.value = true
}

async function ejecutarToggle() {
  const u = usuarioToggle.value
  if (!u) return
  try {
    await usuariosApi.toggle(u.usuario)
    alertType.value = 'success'
    alertMsg.value = `Usuario ${u.usuario} ${u.estado === 'A' ? 'inactivado' : 'activado'}.`
    await cargar()
  } catch (e) {
    alertType.value = 'error'
    alertMsg.value = e.response?.data?.error || 'Error al cambiar estado'
  }
  usuarioToggle.value = null
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
