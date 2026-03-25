<template>
  <div class="ficha-funcionario">
    <div class="page-header">
      <h1><UserPen :size="24" /> Editar Funcionario</h1>
      <p>Modifique los campos necesarios y guarde los cambios.</p>
    </div>

    <AlertMessage v-if="alertMsg" :message="alertMsg" :type="alertType" @close="alertMsg = ''" />

    <form @submit.prevent="guardar" class="form-ficha">
      <fieldset>
        <legend><User :size="16" /> Datos del Funcionario</legend>
        <div class="form-grid">
          <div class="field">
            <label>RUT</label>
            <input :value="store.detalle?.rutFormateado || rut" disabled />
            <small>El RUT no puede ser modificado.</small>
          </div>
          <div class="field">
            <label>Apellido Paterno</label>
            <input v-model="form.apellidoPaterno" maxlength="100" required />
          </div>
          <div class="field">
            <label>Apellido Materno</label>
            <input v-model="form.apellidoMaterno" maxlength="100" />
          </div>
          <div class="field">
            <label>Nombres</label>
            <input v-model="form.nombres" maxlength="100" required />
          </div>
          <div class="field">
            <label>ID Sistema</label>
            <input v-model="form.idSistema" maxlength="50" />
          </div>
        </div>
      </fieldset>

      <div class="form-actions">
        <button type="submit" class="btn btn-primary" :disabled="guardando">
          <Save :size="16" />
          {{ guardando ? 'Guardando...' : 'Actualizar Funcionario' }}
        </button>
        <router-link :to="`/funcionarios/${rut}`" class="btn btn-secondary">
          <X :size="16" /> Cancelar
        </router-link>
      </div>

      <div v-if="store.error" class="error"><CircleAlert :size="16" /> {{ store.error }}</div>
    </form>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useFuncionariosStore } from '../stores/funcionarios.js'
import AlertMessage from '../components/AlertMessage.vue'
import {
  UserPen, User, Save, X, CircleAlert
} from 'lucide-vue-next'

const props = defineProps({ rut: { type: [String, Number], required: true } })
const router = useRouter()
const store = useFuncionariosStore()

const guardando = ref(false)
const alertMsg = ref('')
const alertType = ref('info')

const form = ref({
  apellidoPaterno: '',
  apellidoMaterno: '',
  nombres: '',
  idSistema: ''
})

onMounted(async () => {
  const detalle = await store.obtener(Number(props.rut))
  if (detalle) {
    form.value.apellidoPaterno = detalle.apellidoPaterno || ''
    form.value.apellidoMaterno = detalle.apellidoMaterno || ''
    form.value.nombres = detalle.nombres || ''
    form.value.idSistema = detalle.idSistema || ''
  }
})

async function guardar() {
  guardando.value = true
  try {
    await store.actualizar(props.rut, form.value)
    alertType.value = 'success'
    alertMsg.value = 'Funcionario actualizado exitosamente. Redirigiendo...'
    setTimeout(() => router.push(`/funcionarios/${props.rut}`), 1500)
  } catch {
    alertType.value = 'error'
    alertMsg.value = store.error || 'Error al guardar el funcionario.'
  } finally {
    guardando.value = false
  }
}
</script>

<style scoped>
.form-ficha { max-width: 860px; margin: 0 auto; }
</style>
