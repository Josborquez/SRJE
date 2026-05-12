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
        <h3><Briefcase :size="16" /> Funcionario(s) Titular(es)</h3>
        <div v-if="store.detalle.funcionarios?.length">
          <div v-for="f in store.detalle.funcionarios" :key="f.rutFuncionario" style="margin-bottom: 0.4rem;">
            <router-link :to="`/funcionarios/${f.rutFuncionario}`" style="font-weight: 500;">
              {{ f.rutFormateado }}
            </router-link>
            <span style="margin-left: 0.5rem; color: var(--text-secondary);">{{ f.nombreCompleto || '' }}</span>
          </div>
        </div>
        <p v-else style="color: var(--text-muted); font-size: 0.85rem;">Sin funcionarios asociados (sin retenciones activas).</p>
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
      <button class="btn btn-secondary" @click="imprimirFicha">
        <Printer :size="16" /> Imprimir Ficha
      </button>
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
  Pencil, Ban, ArrowLeft, CircleCheck, CircleX, UserX, Printer
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

function imprimirFicha() {
  const d = store.detalle
  const fecha = new Date().toLocaleDateString('es-CL')

  const sexoLabel = d.sexo === 'M' ? 'Masculino' : d.sexo === 'F' ? 'Femenino' : '-'
  const tcLabel = tipoCuentaLabel.value
  const cuenta = d.ctaEstado || d.ctaOtBanco || '-'
  const banco = d.nombreBanco || d.codBanco || '-'

  let funcHtml = ''
  if (d.funcionarios?.length) {
    funcHtml = d.funcionarios.map(f =>
      `<div style="margin-bottom: 3px;"><strong>${f.rutFormateado}</strong> <span style="color: #64748b; margin-left: 6px;">${f.nombreCompleto || ''}</span></div>`
    ).join('')
  } else {
    funcHtml = '<p class="muted">Sin funcionarios asociados.</p>'
  }

  let retHtml = ''
  if (d.retenciones?.length) {
    retHtml = `<table class="ret-table">
      <thead><tr><th>Funcionario</th><th>Monto</th><th>Codigo</th><th>Tipo Pago</th><th>Periodo</th></tr></thead>
      <tbody>${d.retenciones.map(r => `<tr>
        <td>${r.nombreFuncionario || r.rutTitularFormateado || '-'}</td>
        <td>$${(r.monto || 0).toLocaleString('es-CL')}</td>
        <td>${r.codRetencion || '-'}</td>
        <td>${r.tipoPago || '-'}</td>
        <td>${r.periodoProceso || '-'}</td>
      </tr>`).join('')}</tbody></table>`
  } else {
    retHtml = '<p class="muted">Sin retenciones activas.</p>'
  }

  const html = `<!DOCTYPE html><html><head><meta charset="utf-8">
<title>Ficha Beneficiario - ${d.rutFormateado}</title>
<style>
  * { margin: 0; padding: 0; box-sizing: border-box; }
  body { font-family: Arial, Helvetica, sans-serif; font-size: 11px; color: #1e293b; padding: 20px; }
  h1 { font-size: 16px; margin-bottom: 2px; }
  .subtitle { font-size: 12px; color: #64748b; margin-bottom: 12px; }
  .fecha { font-size: 10px; color: #94a3b8; text-align: right; margin-bottom: 10px; }
  .section { margin-bottom: 14px; }
  .section h2 { font-size: 12px; background: #f1f5f9; padding: 4px 8px; margin-bottom: 6px; border-left: 3px solid #2563eb; }
  dl { display: grid; grid-template-columns: 140px 1fr; gap: 2px 10px; padding: 0 8px; }
  dt { font-weight: bold; color: #475569; }
  dd { color: #1e293b; }
  .estado { display: inline-block; padding: 1px 8px; border-radius: 10px; font-size: 10px; font-weight: bold; }
  .estado-a { background: #dcfce7; color: #166534; }
  .estado-i { background: #fee2e2; color: #991b1b; }
  .ret-table { width: 100%; border-collapse: collapse; font-size: 10px; }
  .ret-table th { background: #f1f5f9; text-align: left; padding: 3px 6px; border-bottom: 1px solid #e2e8f0; }
  .ret-table td { padding: 2px 6px; border-bottom: 1px solid #f1f5f9; }
  .muted { color: #94a3b8; font-size: 10px; font-style: italic; }
  @media print { body { padding: 10px; } }
</style></head><body>
<div class="fecha">Impreso: ${fecha}</div>
<h1>${d.nombreBeneficiario}</h1>
<p class="subtitle">RUT: ${d.rutFormateado} | <span class="estado ${d.estado === 'A' ? 'estado-a' : 'estado-i'}">${d.estado === 'A' ? 'Activo' : 'Inactivo'}</span></p>

<div class="section">
  <h2>Datos Personales</h2>
  <dl>
    <dt>Sexo</dt><dd>${sexoLabel}</dd>
    <dt>Estado Civil</dt><dd>${d.estadoCivil || '-'}</dd>
    <dt>Domicilio</dt><dd>${d.domicilio || '-'}</dd>
    <dt>Comuna</dt><dd>${d.comuna || '-'}</dd>
    <dt>Telefono</dt><dd>${d.telefono || '-'}</dd>
  </dl>
</div>

<div class="section">
  <h2>Funcionario(s) Titular(es)</h2>
  ${funcHtml}
</div>

<div class="section">
  <h2>Cuenta Bancaria</h2>
  <dl>
    <dt>Banco</dt><dd>${banco}</dd>
    <dt>Tipo Cuenta</dt><dd>${tcLabel}</dd>
    <dt>Cuenta</dt><dd>${cuenta}</dd>
    <dt>Sucursal</dt><dd>${d.sucursal || '-'}</dd>
  </dl>
</div>

<div class="section">
  <h2>Retenciones Activas</h2>
  ${retHtml}
</div>

<script>window.onload = function() { window.print(); }<\/script>
</body></html>`

  const win = window.open('', '_blank', 'width=800,height=600')
  win.document.write(html)
  win.document.close()
}

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
