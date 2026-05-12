<template>
  <div class="ficha-beneficiario">
    <div class="page-header">
      <h1>
        <UserPen v-if="isEditing" :size="24" />
        <UserPlus v-else :size="24" />
        {{ isEditing ? 'Editar' : 'Nuevo' }} Beneficiario
      </h1>
      <p v-if="!isEditing">Complete los datos para registrar un nuevo beneficiario en el sistema.</p>
      <p v-else>Modifique los campos necesarios y guarde los cambios.</p>
    </div>

    <AlertMessage v-if="alertMsg" :message="alertMsg" :type="alertType" @close="alertMsg = ''" />

    <form @submit.prevent="guardar" class="form-ficha">
      <!-- Seccion 1: Datos Personales -->
      <fieldset>
        <legend><User :size="16" /> Datos Personales</legend>
        <div class="form-grid">
          <RutInput
            v-model="form.rutBeneficiario"
            label="RUT Beneficiario"
            @rutValidado="onRutValidado"
          />
          <div class="field">
            <label>Nombre Completo (max 39 chars)</label>
            <input v-model="form.nombreBeneficiario" maxlength="39" required />
            <small>{{ form.nombreBeneficiario?.length || 0 }}/39</small>
          </div>
          <div class="field">
            <label>Fecha Nacimiento</label>
            <input type="date" v-model="form.fechaNacimiento" />
          </div>
          <div class="field">
            <label>Sexo</label>
            <select v-model="form.sexo">
              <option value="">-- Seleccionar --</option>
              <option value="M">Masculino</option>
              <option value="F">Femenino</option>
            </select>
          </div>
          <div class="field">
            <label>Estado Civil</label>
            <select v-model="form.estadoCivil">
              <option value="">-- Seleccionar --</option>
              <option>Soltero</option>
              <option>Casado</option>
              <option>Viudo</option>
              <option>Divorciado</option>
            </select>
          </div>
          <div class="field">
            <label>Domicilio</label>
            <input v-model="form.domicilio" maxlength="100" placeholder="Direccion completa" />
          </div>
          <div class="field">
            <label>Comuna</label>
            <input v-model="form.comuna" maxlength="50" />
          </div>
          <div class="field">
            <label>Telefono</label>
            <input v-model="form.telefono" maxlength="20" placeholder="+56 9 XXXX XXXX" />
          </div>
        </div>
      </fieldset>

      <!-- Seccion 2: Cuenta Bancaria -->
      <fieldset>
        <legend><Landmark :size="16" /> Cuenta Bancaria</legend>
        <div class="form-grid">
          <div class="field">
            <label>Banco</label>
            <select v-model="form.codBanco" @change="onBancoChange">
              <option :value="null">-- Seleccionar banco --</option>
              <option v-for="b in bancos" :key="b.codBanco" :value="b.codBanco">
                {{ b.codBanco }} - {{ b.nombreBanco }}
              </option>
            </select>
          </div>
          <div class="field">
            <label>Tipo Cuenta</label>
            <select v-model="form.tipoCuenta">
              <option :value="null">-- Seleccionar tipo --</option>
              <option :value="1">01 - Cuenta Corriente</option>
              <option :value="2">02 - Cuenta de Ahorro / CuentaRUT</option>
              <option :value="3">03 - Cuenta Vista</option>
            </select>
          </div>
          <div class="field" v-if="form.codBanco === 12">
            <label>Cuenta BancoEstado (11 digitos)</label>
            <input v-model="form.ctaEstado" maxlength="11" placeholder="Ej: 41762633599" @input="limpiarCuenta('ctaEstado')" />
          </div>
          <div class="field" v-if="form.codBanco && form.codBanco !== 12">
            <label>Cuenta Otro Banco (max 15)</label>
            <input v-model="form.ctaOtBanco" maxlength="15" @input="limpiarCuenta('ctaOtBanco')" />
          </div>
          <div class="field">
            <label>Sucursal</label>
            <input v-model="form.sucursal" maxlength="60" />
          </div>
        </div>
      </fieldset>

      <div class="form-actions">
        <button type="submit" class="btn btn-primary" :disabled="!rutValido || guardando">
          <Save :size="16" />
          {{ guardando ? 'Guardando...' : (isEditing ? 'Actualizar' : 'Crear') + ' Beneficiario' }}
        </button>
        <router-link to="/beneficiarios" class="btn btn-secondary">
          <X :size="16" /> Cancelar
        </router-link>
      </div>

      <div v-if="store.error" class="error"><CircleAlert :size="16" /> {{ store.error }}</div>
    </form>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import RutInput from '../components/RutInput.vue'
import AlertMessage from '../components/AlertMessage.vue'
import { useBeneficiariosStore } from '../stores/beneficiarios.js'
import { catalogosApi } from '../api/index.js'
import { formatCuenta } from '../composables/useFormato.js'
import {
  UserPlus, UserPen, User, Landmark,
  Save, X, CircleAlert
} from 'lucide-vue-next'

const route = useRoute()
const router = useRouter()
const store = useBeneficiariosStore()

const isEditing = computed(() => !!route.params.rut)
const rutValido = ref(false)
const bancos = ref([])
const guardando = ref(false)
const alertMsg = ref('')
const alertType = ref('info')

const form = ref({
  rutBeneficiario: null,
  dvBeneficiario: '',
  nombreBeneficiario: '',
  fechaNacimiento: null,
  sexo: '',
  estadoCivil: '',
  domicilio: '',
  comuna: '',
  telefono: '',
  ctaOtBanco: '',
  tipoCuenta: null,
  codBanco: null,
  ctaEstado: '',
  sucursal: ''
})

onMounted(async () => {
  const { data } = await catalogosApi.bancos()
  bancos.value = data

  if (route.params.rut) {
    const detalle = await store.obtener(Number(route.params.rut))
    if (detalle) {
      Object.assign(form.value, detalle)
      if (form.value.ctaEstado) {
        form.value.ctaEstado = formatCuenta(form.value.ctaEstado)
        if (form.value.ctaEstado === '-') form.value.ctaEstado = ''
      }
      if (form.value.ctaOtBanco) {
        form.value.ctaOtBanco = formatCuenta(form.value.ctaOtBanco)
        if (form.value.ctaOtBanco === '-') form.value.ctaOtBanco = ''
      }
      rutValido.value = true
    }
  }
})

function onRutValidado({ rut, dv, valido }) {
  form.value.dvBeneficiario = dv
  rutValido.value = valido
}

function onBancoChange() {
  form.value.ctaEstado = ''
  form.value.ctaOtBanco = ''
}

function limpiarCuenta(campo) {
  form.value[campo] = form.value[campo].replace(/[^0-9]/g, '')
}

async function guardar() {
  guardando.value = true
  try {
    if (isEditing.value) {
      await store.actualizar(route.params.rut, form.value)
      alertType.value = 'success'
      alertMsg.value = 'Beneficiario actualizado exitosamente. Redirigiendo...'
    } else {
      await store.crear(form.value)
      alertType.value = 'success'
      alertMsg.value = 'Beneficiario creado exitosamente. Redirigiendo...'
    }
    setTimeout(() => router.push('/beneficiarios'), 1500)
  } catch {
    alertType.value = 'error'
    alertMsg.value = store.error || 'Error al guardar el beneficiario.'
  } finally {
    guardando.value = false
  }
}
</script>

<style scoped>
.form-ficha { max-width: 860px; margin: 0 auto; }
</style>
