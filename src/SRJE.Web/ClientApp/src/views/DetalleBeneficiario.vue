<template>
  <div class="detalle-beneficiario" v-if="store.detalle">
    <div class="page-header">
      <h1><UserCheck :size="24" /> {{ store.detalle.nombreBeneficiario }}</h1>
      <p>{{ store.detalle.rutFormateado }}</p>
    </div>

    <AlertMessage v-if="alertMsg" :message="alertMsg" :type="alertType" @close="alertMsg = ''" />

    <div class="sections">
      <section>
        <h3><User :size="16" /> Datos Personales</h3>
        <dl>
          <dt>Estado</dt>
          <dd>
            <span :class="'estado estado-' + store.detalle.estado.toLowerCase()">
              <CircleCheck v-if="store.detalle.estado === 'A'" :size="13" />
              <CircleX v-else :size="13" />
              {{ store.detalle.estado === 'A' ? 'Activo' : 'Inactivo' }}
            </span>
          </dd>
          <dt>Sexo</dt><dd>{{ store.detalle.sexo === 'M' ? 'Masculino' : store.detalle.sexo === 'F' ? 'Femenino' : '-' }}</dd>
          <dt>Estado Civil</dt><dd>{{ store.detalle.estadoCivil || '-' }}</dd>
          <dt>Domicilio</dt><dd>{{ store.detalle.domicilio || '-' }}</dd>
          <dt>Comuna</dt><dd>{{ store.detalle.comuna || '-' }}</dd>
          <dt>Telefono</dt><dd>{{ store.detalle.telefono || '-' }}</dd>
        </dl>
      </section>

      <section>
        <h3><Briefcase :size="16" /> Datos del Funcionario</h3>
        <dl>
          <dt>RUT Funcionario</dt><dd>{{ store.detalle.rutFuncionarioFormateado || '-' }}</dd>
          <dt>Nombre Funcionario</dt><dd>{{ store.detalle.nombreFuncionario || '-' }}</dd>
        </dl>
      </section>

      <section>
        <h3><Landmark :size="16" /> Cuenta Bancaria</h3>
        <dl>
          <dt>Banco</dt><dd>{{ store.detalle.nombreBanco || store.detalle.codBanco || '-' }}</dd>
          <dt>Tipo Cuenta</dt><dd>{{ tipoCuentaLabel }}</dd>
          <dt>Cuenta</dt><dd>{{ formatCuenta(store.detalle.ctaEstado || store.detalle.ctaOtBanco) }}</dd>
          <dt>Sucursal</dt><dd>{{ store.detalle.sucursal || '-' }}</dd>
        </dl>
      </section>
    </div>

    <section v-if="store.detalle.retenciones?.length" class="card-section">
      <h3><Scale :size="16" /> Retenciones Activas</h3>
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
            <td>{{ r.nombreFuncionario || r.rutTitularFormateado }}</td>
            <td>${{ r.monto?.toLocaleString('es-CL') }}</td>
            <td>{{ r.codRetencion }}</td>
            <td>{{ r.tipoPago }}</td>
            <td>{{ r.periodoProceso }}</td>
          </tr>
        </tbody>
      </table>
    </section>

    <div class="actions">
      <router-link :to="`/beneficiarios/${rut}/editar`" class="btn btn-primary">
        <Pencil :size="16" /> Editar
      </router-link>
      <button v-if="store.detalle.estado === 'A'" class="btn btn-danger" @click="showConfirm = true">
        <Ban :size="16" /> Inactivar
      </button>
      <router-link to="/beneficiarios" class="btn btn-secondary">
        <ArrowLeft :size="16" /> Volver
      </router-link>
    </div>

    <ConfirmModal
      v-model="showConfirm"
      title="Inactivar Beneficiario"
      :message="`¿Esta seguro de inactivar a ${store.detalle.nombreBeneficiario}? Esta accion cambiara su estado a Inactivo.`"
      confirmText="Si, inactivar"
      variant="danger"
      @confirm="ejecutarInactivar"
    />
  </div>
  <div v-else-if="store.loading" class="loading">Cargando datos del beneficiario...</div>
  <div v-else class="empty-state">
    <UserX :size="48" class="empty-icon" />
    <p>Beneficiario no encontrado</p>
    <router-link to="/beneficiarios" class="btn btn-secondary" style="margin-top:0.5rem">
      <ArrowLeft :size="16" /> Volver a la lista
    </router-link>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useBeneficiariosStore } from '../stores/beneficiarios.js'
import { formatCuenta } from '../composables/useFormato.js'
import AlertMessage from '../components/AlertMessage.vue'
import ConfirmModal from '../components/ConfirmModal.vue'
import {
  UserCheck, User, Briefcase, Landmark, Scale,
  Pencil, Ban, ArrowLeft, CircleCheck, CircleX, UserX
} from 'lucide-vue-next'

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
