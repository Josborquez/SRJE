<template>
  <div class="detalle-beneficiario" v-if="store.detalle">
    <h1>{{ store.detalle.nombreBeneficiario }}</h1>
    <p class="rut">{{ store.detalle.rutFormateado }}</p>

    <AlertMessage v-if="alertMsg" :message="alertMsg" :type="alertType" @close="alertMsg = ''" />

    <div class="sections">
      <section>
        <h3>Datos Personales</h3>
        <dl>
          <dt>Estado</dt><dd><span :class="'estado estado-' + store.detalle.estado.toLowerCase()">{{ store.detalle.estado === 'A' ? 'Activo' : 'Inactivo' }}</span></dd>
          <dt>Sexo</dt><dd>{{ store.detalle.sexo === 'M' ? 'Masculino' : store.detalle.sexo === 'F' ? 'Femenino' : '-' }}</dd>
          <dt>Estado Civil</dt><dd>{{ store.detalle.estadoCivil || '-' }}</dd>
          <dt>Domicilio</dt><dd>{{ store.detalle.domicilio || '-' }}</dd>
          <dt>Comuna</dt><dd>{{ store.detalle.comuna || '-' }}</dd>
          <dt>Telefono</dt><dd>{{ store.detalle.telefono || '-' }}</dd>
        </dl>
      </section>

      <section>
        <h3>Datos del Funcionario</h3>
        <dl>
          <dt>RUT Funcionario</dt><dd>{{ store.detalle.rutFuncionarioFormateado || '-' }}</dd>
          <dt>Nombre Funcionario</dt><dd>{{ store.detalle.nombreFuncionario || '-' }}</dd>
        </dl>
      </section>

      <section>
        <h3>Cuenta Bancaria</h3>
        <dl>
          <dt>Banco</dt><dd>{{ store.detalle.nombreBanco || store.detalle.codBanco || '-' }}</dd>
          <dt>Tipo Cuenta</dt><dd>{{ tipoCuentaLabel }}</dd>
          <dt>Cuenta</dt><dd>{{ formatCuenta(store.detalle.ctaEstado || store.detalle.ctaOtBanco) }}</dd>
          <dt>Sucursal</dt><dd>{{ store.detalle.sucursal || '-' }}</dd>
        </dl>
      </section>
    </div>

    <section v-if="store.detalle.retenciones?.length">
      <h3>Retenciones Activas</h3>
      <table class="data-table">
        <thead>
          <tr>
            <th>Funcionario</th>
            <th>Monto</th>
            <th>Codigo</th>
            <th>Tipo Pago</th>
            <th>Periodo</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="r in store.detalle.retenciones" :key="r.id">
            <td>{{ r.nombreFuncionario || r.rutTitular }}</td>
            <td>${{ r.monto?.toLocaleString('es-CL') }}</td>
            <td>{{ r.codRetencion }}</td>
            <td>{{ r.tipoPago }}</td>
            <td>{{ r.periodoProceso }}</td>
          </tr>
        </tbody>
      </table>
    </section>

    <div class="actions">
      <router-link :to="`/beneficiarios/${rut}/editar`" class="btn btn-primary">Editar</router-link>
      <button v-if="store.detalle.estado === 'A'" class="btn btn-danger" @click="showConfirm = true">Inactivar</button>
      <router-link to="/beneficiarios" class="btn btn-secondary">Volver</router-link>
    </div>

    <ConfirmModal
      v-model="showConfirm"
      title="Inactivar Beneficiario"
      :message="`Esta seguro de inactivar a ${store.detalle.nombreBeneficiario}? Esta accion cambiara su estado a Inactivo.`"
      confirmText="Si, inactivar"
      variant="danger"
      @confirm="ejecutarInactivar"
    />
  </div>
  <div v-else-if="store.loading" class="loading">Cargando...</div>
  <div v-else class="error">Beneficiario no encontrado</div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useBeneficiariosStore } from '../stores/beneficiarios.js'
import { formatCuenta } from '../composables/useFormato.js'
import AlertMessage from '../components/AlertMessage.vue'
import ConfirmModal from '../components/ConfirmModal.vue'

const props = defineProps({ rut: { type: [String, Number], required: true } })
const store = useBeneficiariosStore()
const router = useRouter()

const alertMsg = ref('')
const alertType = ref('info')
const showConfirm = ref(false)

const tipoCuentaLabel = computed(() => {
  const tc = store.detalle?.tipoCuenta
  if (tc === 1) return 'Cuenta Corriente'
  if (tc === 2) return 'Ahorro / CuentaRUT'
  if (tc === 3) return 'Cuenta Vista'
  return '-'
})

onMounted(() => store.obtener(Number(props.rut)))

async function ejecutarInactivar() {
  const ok = await store.inactivar(Number(props.rut))
  if (ok) {
    alertType.value = 'success'
    alertMsg.value = 'Beneficiario inactivado exitosamente. Redirigiendo...'
    setTimeout(() => router.push('/beneficiarios'), 1500)
  } else {
    alertType.value = 'error'
    alertMsg.value = store.error || 'Error al inactivar el beneficiario.'
  }
}
</script>

<style scoped>
.rut { font-size: 1.2rem; color: #666; margin-bottom: 1rem; }
.sections { display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 1rem; margin-bottom: 1rem; }
section { background: #fff; padding: 1rem; border-radius: 8px; box-shadow: 0 1px 3px rgba(0,0,0,0.1); margin-bottom: 1rem; }
h3 { margin-bottom: 0.75rem; color: #1976d2; }
dl { display: grid; grid-template-columns: auto 1fr; gap: 0.3rem 1rem; }
dt { font-weight: 500; color: #666; }
dd { margin: 0; }
.estado { padding: 0.15rem 0.5rem; border-radius: 3px; font-size: 0.8rem; }
.estado-a { background: #e8f5e9; color: #2e7d32; }
.estado-i { background: #ffebee; color: #c62828; }
.data-table { width: 100%; border-collapse: collapse; }
.data-table th, .data-table td { padding: 0.5rem; border-bottom: 1px solid #eee; text-align: left; }
.data-table th { background: #f5f5f5; }
.actions { display: flex; gap: 0.5rem; margin-top: 1rem; }
.btn { padding: 0.5rem 1rem; border: none; border-radius: 4px; cursor: pointer; text-decoration: none; font-size: 0.9rem; }
.btn-primary { background: #1976d2; color: #fff; }
.btn-danger { background: #d32f2f; color: #fff; }
.btn-danger:hover { background: #c62828; }
.btn-secondary { background: #eee; color: #333; }
.loading { padding: 2rem; text-align: center; }
.error { padding: 1rem; background: #fce4ec; color: #c62828; border-radius: 4px; }
</style>
