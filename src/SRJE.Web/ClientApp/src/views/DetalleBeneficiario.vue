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
        <h3><Landmark :size="16" /> Cuenta Bancaria Principal</h3>
        <dl>
          <dt>Banco</dt><dd>{{ store.detalle.nombreBanco || store.detalle.codBanco || '-' }}</dd>
          <dt>Tipo Cuenta</dt><dd>{{ tipoCuentaLabel }}</dd>
          <dt>Cuenta</dt><dd>{{ formatCuenta(store.detalle.ctaEstado || store.detalle.ctaOtBanco) }}</dd>
          <dt>Sucursal</dt><dd>{{ store.detalle.sucursal || '-' }}</dd>
        </dl>
      </section>

      <section v-if="store.detalle.cuentas?.length">
        <h3><Landmark :size="16" /> Cuentas Almacenadas ({{ store.detalle.cuentas.length }})</h3>
        <table class="data-table">
          <thead>
            <tr>
              <th>Banco</th>
              <th>Tipo Cuenta</th>
              <th>N° Cuenta</th>
              <th>Alias</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="c in store.detalle.cuentas" :key="c.id">
              <td>{{ c.nombreBanco || c.codBanco }}</td>
              <td>{{ c.tipoCuentaDescripcion || c.tipoCuenta }}</td>
              <td>{{ formatCuenta(c.numeroCuenta) }}</td>
              <td>{{ c.alias || '-' }}</td>
            </tr>
          </tbody>
        </table>
      </section>
    </div>

    <section v-if="store.detalle.retenciones?.length" class="card-section">
      <h3><Scale :size="16" /> Retenciones Activas</h3>
      <table class="data-table">
        <thead>
          <tr>
            <th>Funcionario</th>
            <th>Banco</th>
            <th>Cuenta</th>
            <th>Monto</th>
            <th>Codigo</th>
            <th>Tipo Pago</th>
            <th>Periodo</th>
            <th>Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="r in store.detalle.retenciones" :key="r.id">
            <td>{{ r.nombreFuncionario || r.rutTitularFormateado }}</td>
            <td>{{ r.nombreBanco || '-' }}</td>
            <td>{{ formatCuenta(r.numeroCuenta) }}</td>
            <td>${{ r.monto?.toLocaleString('es-CL') }}</td>
            <td>{{ r.codRetencion }}</td>
            <td>{{ r.tipoPago }}</td>
            <td>{{ r.periodoProceso }}</td>
            <td>
              <button class="btn-sm" @click="editarRetencion(r)">
                <Pencil :size="14" /> Editar
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </section>

    <!-- Modal editar retencion -->
    <div v-if="editRet" class="modal-overlay" @click.self="cerrarModal">
      <div class="modal-box">
        <h3>Editar Retencion</h3>
        <p style="color: var(--text-secondary); font-size: 0.85rem; margin-bottom: 1rem;">
          Funcionario: {{ editRet.nombreFuncionario || editRet.rutTitularFormateado }}
          — Periodo: {{ editRet.periodoProceso }}
        </p>
        <div v-if="store.detalle.cuentas?.length" class="form-group">
          <label>Cuenta almacenada</label>
          <select v-model="cuentaSeleccionada" @change="aplicarCuenta" class="form-control cuenta-select">
            <option value="">— Ingresar manualmente —</option>
            <option v-for="c in store.detalle.cuentas" :key="c.id" :value="c.id">
              {{ c.nombreBanco || c.codBanco }} — {{ c.tipoCuentaDescripcion || '' }} — {{ c.numeroCuenta }} {{ c.alias ? `(${c.alias})` : '' }}
            </option>
          </select>
        </div>
        <div v-if="!cuentaSeleccionada" class="campos-manuales">
          <div class="form-group">
            <label>Banco</label>
            <select v-model="editForm.codBanco" class="form-control">
              <option :value="null">— Sin banco —</option>
              <option v-for="b in bancos" :key="b.codBanco" :value="b.codBanco">{{ b.nombreBanco }}</option>
            </select>
          </div>
          <div class="form-group">
            <label>Tipo Cuenta</label>
            <select v-model="editForm.tipoCuenta" class="form-control">
              <option :value="null">—</option>
              <option v-for="tc in tiposCuenta" :key="tc.codigo" :value="tc.codigo">{{ tc.descripcion }}</option>
            </select>
          </div>
          <div class="form-group">
            <label>N° Cuenta</label>
            <input v-model="editForm.numeroCuenta" class="form-control" maxlength="15" />
          </div>
        </div>
        <div v-else class="cuenta-preview">
          <span class="cuenta-badge">{{ cuentaPreview }}</span>
        </div>
        <div class="form-group">
          <label>Monto ($)</label>
          <input v-model.number="editForm.monto" type="number" min="0" class="form-control" />
        </div>
        <div class="modal-actions">
          <button class="btn btn-primary" @click="guardarRetencion" :disabled="guardando">
            {{ guardando ? 'Guardando...' : 'Guardar' }}
          </button>
          <button class="btn btn-secondary" @click="cerrarModal">Cancelar</button>
        </div>
      </div>
    </div>

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
import { catalogosApi } from '../api/index.js'
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

// Editar retencion
const editRet = ref(null)
const editForm = ref({ monto: 0, codBanco: null, tipoCuenta: null, numeroCuenta: '' })
const cuentaSeleccionada = ref('')
const guardando = ref(false)
const bancos = ref([])
const tiposCuenta = ref([])

const cuentaPreview = computed(() => {
  if (!cuentaSeleccionada.value) return ''
  const c = store.detalle?.cuentas?.find(c => c.id === cuentaSeleccionada.value)
  if (!c) return ''
  return `${c.nombreBanco || c.codBanco} — ${c.tipoCuentaDescripcion || ''} — ${c.numeroCuenta}`
})

const tipoCuentaLabel = computed(() => {
  const tc = store.detalle?.tipoCuenta
  if (tc === 1) return 'Cuenta Corriente'
  if (tc === 2) return 'Ahorro / CuentaRUT'
  if (tc === 3) return 'Cuenta Vista'
  return '-'
})

onMounted(async () => {
  store.detalle = null
  store.obtener(Number(props.rut))
  try {
    const [bancosRes, tiposRes] = await Promise.all([
      catalogosApi.bancos(),
      catalogosApi.tiposCuenta()
    ])
    bancos.value = bancosRes.data
    tiposCuenta.value = tiposRes.data
  } catch { /* catalogos opcionales */ }
})

function editarRetencion(r) {
  editRet.value = r
  editForm.value = {
    monto: r.monto,
    codBanco: r.codBanco ?? null,
    tipoCuenta: r.tipoCuenta ?? null,
    numeroCuenta: r.numeroCuenta ?? ''
  }
  // Pre-seleccionar cuenta almacenada si coincide
  const cuentas = store.detalle?.cuentas || []
  const match = cuentas.find(c =>
    c.codBanco === r.codBanco && c.numeroCuenta === r.numeroCuenta
  )
  cuentaSeleccionada.value = match ? match.id : ''
}

function aplicarCuenta() {
  if (!cuentaSeleccionada.value) return
  const c = store.detalle?.cuentas?.find(c => c.id === cuentaSeleccionada.value)
  if (!c) return
  editForm.value.codBanco = c.codBanco
  editForm.value.tipoCuenta = c.tipoCuenta
  editForm.value.numeroCuenta = c.numeroCuenta
}

function cerrarModal() {
  editRet.value = null
  cuentaSeleccionada.value = ''
}

async function guardarRetencion() {
  guardando.value = true
  try {
    await store.actualizarRetencion(Number(props.rut), editRet.value.id, editForm.value)
    cerrarModal()
    alertType.value = 'success'
    alertMsg.value = 'Retencion actualizada exitosamente.'
  } catch (e) {
    alertType.value = 'error'
    alertMsg.value = store.error || 'Error al actualizar la retencion.'
  } finally {
    guardando.value = false
  }
}

function esc(str) {
  const div = document.createElement('div')
  div.textContent = str ?? ''
  return div.innerHTML
}

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
      `<div style="margin-bottom: 3px;"><strong>${esc(f.rutFormateado)}</strong> <span style="color: #64748b; margin-left: 6px;">${esc(f.nombreCompleto)}</span></div>`
    ).join('')
  } else {
    funcHtml = '<p class="muted">Sin funcionarios asociados.</p>'
  }

  let retHtml = ''
  if (d.retenciones?.length) {
    retHtml = `<table class="ret-table">
      <thead><tr><th>Funcionario</th><th>Banco</th><th>Cuenta</th><th>Monto</th><th>Codigo</th><th>Tipo Pago</th><th>Periodo</th></tr></thead>
      <tbody>${d.retenciones.map(r => `<tr>
        <td>${esc(r.nombreFuncionario || r.rutTitularFormateado || '-')}</td>
        <td>${esc(r.nombreBanco || '-')}</td>
        <td>${esc(r.numeroCuenta || '-')}</td>
        <td>$${(r.monto || 0).toLocaleString('es-CL')}</td>
        <td>${esc(r.codRetencion || '-')}</td>
        <td>${esc(r.tipoPago || '-')}</td>
        <td>${esc(r.periodoProceso || '-')}</td>
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
<h1>${esc(d.nombreBeneficiario)}</h1>
<p class="subtitle">RUT: ${esc(d.rutFormateado)} | <span class="estado ${d.estado === 'A' ? 'estado-a' : 'estado-i'}">${d.estado === 'A' ? 'Activo' : 'Inactivo'}</span></p>

<div class="section">
  <h2>Datos Personales</h2>
  <dl>
    <dt>Sexo</dt><dd>${esc(sexoLabel)}</dd>
    <dt>Estado Civil</dt><dd>${esc(d.estadoCivil || '-')}</dd>
    <dt>Domicilio</dt><dd>${esc(d.domicilio || '-')}</dd>
    <dt>Comuna</dt><dd>${esc(d.comuna || '-')}</dd>
    <dt>Telefono</dt><dd>${esc(d.telefono || '-')}</dd>
  </dl>
</div>

<div class="section">
  <h2>Funcionario(s) Titular(es)</h2>
  ${funcHtml}
</div>

<div class="section">
  <h2>Cuenta Bancaria Principal</h2>
  <dl>
    <dt>Banco</dt><dd>${esc(banco)}</dd>
    <dt>Tipo Cuenta</dt><dd>${esc(tcLabel)}</dd>
    <dt>Cuenta</dt><dd>${esc(cuenta)}</dd>
    <dt>Sucursal</dt><dd>${esc(d.sucursal || '-')}</dd>
  </dl>
</div>

${d.cuentas?.length ? `<div class="section">
  <h2>Cuentas Almacenadas (${d.cuentas.length})</h2>
  <table class="ret-table">
    <thead><tr><th>Banco</th><th>Tipo Cuenta</th><th>N° Cuenta</th><th>Alias</th></tr></thead>
    <tbody>${d.cuentas.map(c => `<tr>
      <td>${esc(c.nombreBanco || String(c.codBanco))}</td>
      <td>${esc(c.tipoCuentaDescripcion || String(c.tipoCuenta))}</td>
      <td>${esc(c.numeroCuenta || '-')}</td>
      <td>${esc(c.alias || '-')}</td>
    </tr>`).join('')}</tbody>
  </table>
</div>` : ''}

<div class="section">
  <h2>Retenciones Activas</h2>
  ${retHtml}
</div>

<script>window.onload = function() { window.print(); window.close(); }<\/script>
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

<style scoped>
.modal-overlay {
  position: fixed; inset: 0; background: rgba(15, 23, 42, 0.5);
  display: flex; align-items: center; justify-content: center; z-index: 1000;
  backdrop-filter: blur(2px);
}
.modal-box {
  background: #fff; border-radius: 14px; padding: 1.5rem;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.2);
  max-width: 480px; width: 92%;
}
.modal-box h3 { margin: 0 0 0.25rem; font-size: 1.05rem; }
.form-group { margin-bottom: 0.75rem; }
.form-group label { display: block; font-size: 0.82rem; font-weight: 600; color: #475569; margin-bottom: 0.25rem; }
.form-control {
  width: 100%; padding: 0.5rem 0.65rem; border: 1px solid #cbd5e1; border-radius: 6px;
  font-size: 0.9rem; background: #fff;
}
.form-control:focus { outline: none; border-color: #2563eb; box-shadow: 0 0 0 2px rgba(37, 99, 235, 0.15); }
.modal-actions { display: flex; gap: 0.6rem; margin-top: 1rem; }
.cuenta-select { font-size: 0.85rem; }
.cuenta-preview {
  margin-bottom: 0.75rem; padding: 0.5rem 0.75rem;
  background: #f0fdf4; border: 1px solid #bbf7d0; border-radius: 6px;
}
.cuenta-badge { font-size: 0.85rem; color: #166534; font-weight: 500; }
.campos-manuales { border-left: 3px solid #e2e8f0; padding-left: 0.75rem; margin-bottom: 0.25rem; }
</style>
