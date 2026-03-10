<template>
  <div class="ficha-beneficiario">
    <h1>{{ isEditing ? 'Editar' : 'Nuevo' }} Beneficiario</h1>

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

      <!-- Seccion 2: Cuenta Bancaria -->
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
            <input v-model="form.ctaEstado" maxlength="11" placeholder="Ej: 41762633599" />
          </div>
          <div class="field" v-if="form.codBanco && form.codBanco !== 12">
            <label>Cuenta Otro Banco (max 15)</label>
            <input v-model="form.ctaOtBanco" maxlength="15" />
          </div>
          <div class="field">
            <label>Sucursal</label>
            <input v-model="form.sucursal" maxlength="60" />
          </div>
        </div>
      </fieldset>

      <div class="form-actions">
        <button type="submit" class="btn btn-primary" :disabled="!rutValido">
          {{ isEditing ? 'Actualizar' : 'Crear' }} Beneficiario
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
import { useBeneficiariosStore } from '../stores/beneficiarios.js'
import { catalogosApi } from '../api/index.js'

const route = useRoute()
const router = useRouter()
const store = useBeneficiariosStore()

const isEditing = computed(() => !!route.params.rut)
const rutValido = ref(false)
const bancos = ref([])

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

async function guardar() {
  try {
    if (isEditing.value) {
      await store.actualizar(route.params.rut, form.value)
    } else {
      await store.crear(form.value)
    }
    router.push('/beneficiarios')
  } catch {
    // error ya esta en store.error
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
