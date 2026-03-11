<template>
  <div class="ficha-beneficiario">
    <h1>{{ isEditing ? 'Editar' : 'Nuevo' }} Beneficiario</h1>

    <AlertMessage v-if="alertMsg" :message="alertMsg" :type="alertType" @close="alertMsg = ''" />

    <form @submit.prevent="guardar" class="form-ficha">
      <!-- Seccion 1: Datos Personales -->
      <fieldset>
        <legend>Datos Personales</legend>
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
              <option value="">--</option>
              <option value="M">Masculino</option>
              <option value="F">Femenino</option>
            </select>
          </div>
          <div class="field">
            <label>Estado Civil</label>
            <select v-model="form.estadoCivil">
              <option value="">--</option>
              <option>Soltero</option>
              <option>Casado</option>
              <option>Viudo</option>
              <option>Divorciado</option>
            </select>
          </div>
          <div class="field">
            <label>Domicilio</label>
            <input v-model="form.domicilio" maxlength="100" />
          </div>
          <div class="field">
            <label>Comuna</label>
            <input v-model="form.comuna" maxlength="50" />
          </div>
          <div class="field">
            <label>Telefono</label>
            <input v-model="form.telefono" maxlength="20" placeholder="+56XXXXXXXXX" />
          </div>
        </div>
      </fieldset>

      <!-- Seccion 2: Datos del Funcionario -->
      <fieldset>
        <legend>Datos del Funcionario</legend>
        <div class="form-grid">
          <RutInput
            v-model="form.rutFuncionario"
            label="RUT Funcionario"
            @rutValidado="onRutFuncionarioValidado"
          />
          <div class="field">
            <label>Nombre Funcionario (max 100 chars)</label>
            <input v-model="form.nombreFuncionario" maxlength="100" />
            <small>{{ form.nombreFuncionario?.length || 0 }}/100</small>
          </div>
        </div>
      </fieldset>

      <!-- Seccion 3: Cuenta Bancaria -->
      <fieldset>
        <legend>Cuenta Bancaria</legend>
        <div class="form-grid">
          <div class="field">
            <label>Banco</label>
            <select v-model="form.codBanco" @change="onBancoChange">
              <option :value="null">-- Seleccionar --</option>
              <option v-for="b in bancos" :key="b.codBanco" :value="b.codBanco">
                {{ b.codBanco }} - {{ b.nombreBanco }}
              </option>
            </select>
          </div>
          <div class="field">
            <label>Tipo Cuenta</label>
            <select v-model="form.tipoCuenta">
              <option :value="null">--</option>
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
          {{ guardando ? 'Guardando...' : (isEditing ? 'Actualizar' : 'Crear') + ' Beneficiario' }}
        </button>
        <router-link to="/beneficiarios" class="btn btn-secondary">Cancelar</router-link>
      </div>

      <div v-if="store.error" class="error">{{ store.error }}</div>
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
  sucursal: '',
  rutFuncionario: null,
  dvFuncionario: '',
  nombreFuncionario: ''
})

onMounted(async () => {
  const { data } = await catalogosApi.bancos()
  bancos.value = data

  if (route.params.rut) {
    const detalle = await store.obtener(Number(route.params.rut))
    if (detalle) {
      Object.assign(form.value, detalle)
      // Corregir numero de cuenta si viene en notacion cientifica
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

function onRutFuncionarioValidado({ rut, dv, valido }) {
  form.value.dvFuncionario = dv
}

function onBancoChange() {
  form.value.ctaEstado = ''
  form.value.ctaOtBanco = ''
}

function limpiarCuenta(campo) {
  // Solo permitir digitos en los campos de cuenta
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
.form-ficha { max-width: 800px; }
fieldset { border: 1px solid #ddd; border-radius: 8px; padding: 1rem; margin-bottom: 1rem; background: #fff; }
legend { font-weight: 600; padding: 0 0.5rem; color: #1976d2; }
.form-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 0.75rem; }
.field { display: flex; flex-direction: column; }
.field label { font-size: 0.85rem; margin-bottom: 0.2rem; font-weight: 500; }
.field input, .field select { padding: 0.45rem; border: 1px solid #ccc; border-radius: 4px; }
.field small { color: #999; font-size: 0.75rem; }
.form-actions { display: flex; gap: 0.5rem; margin-top: 1rem; }
.btn { padding: 0.6rem 1.2rem; border: none; border-radius: 4px; cursor: pointer; text-decoration: none; font-size: 0.9rem; }
.btn-primary { background: #1976d2; color: #fff; }
.btn-primary:disabled { background: #ccc; }
.btn-secondary { background: #eee; color: #333; }
.error { margin-top: 1rem; padding: 0.8rem; background: #fce4ec; color: #c62828; border-radius: 4px; }
</style>
