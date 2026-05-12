<template>
  <div class="detalle-funcionario" v-if="store.detalle">
    <div class="page-header">
      <h1><UserCheck :size="24" /> {{ store.detalle.nombres }} {{ store.detalle.apellidoPaterno }} {{ store.detalle.apellidoMaterno }}</h1>
      <p>{{ store.detalle.rutFormateado }}</p>
    </div>

    <AlertMessage v-if="alertMsg" :message="alertMsg" :type="alertType" @close="alertMsg = ''" />

    <div class="stats-bar">
      <div class="stat-card">
        <Users :size="18" class="stat-icon" />
        <div class="stat-content">
          <span class="stat-value">{{ store.detalle.beneficiarios?.length || 0 }}</span>
          <span class="stat-label">Total Beneficiarios</span>
        </div>
      </div>
      <div class="stat-card">
        <DollarSign :size="18" class="stat-icon" />
        <div class="stat-content">
          <span class="stat-value">${{ montoTotalRetenciones.toLocaleString('es-CL') }}</span>
          <span class="stat-label">Monto Total Retenciones</span>
        </div>
      </div>
    </div>

    <div class="sections">
      <section>
        <h3><User :size="16" /> Datos del Funcionario</h3>
        <dl>
          <dt>Estado</dt>
          <dd>
            <span :class="'estado estado-' + (store.detalle.activo === 'S' ? 'a' : 'i')">
              <CircleCheck v-if="store.detalle.activo === 'S'" :size="13" />
              <CircleX v-else :size="13" />
              {{ store.detalle.activo === 'S' ? 'Activo' : 'Inactivo' }}
            </span>
          </dd>
          <dt>Apellido Paterno</dt><dd>{{ store.detalle.apellidoPaterno || '-' }}</dd>
          <dt>Apellido Materno</dt><dd>{{ store.detalle.apellidoMaterno || '-' }}</dd>
          <dt>Nombres</dt><dd>{{ store.detalle.nombres || '-' }}</dd>
          <dt>ID Sistema</dt><dd>{{ store.detalle.idSistema || '-' }}</dd>
        </dl>
      </section>
    </div>

    <!-- Beneficiarios del funcionario -->
    <section v-if="store.detalle.beneficiarios?.length" class="card-section">
      <h3><Users :size="16" /> Beneficiarios</h3>
      <div v-for="b in store.detalle.beneficiarios" :key="b.rutBeneficiario" class="beneficiario-card">
        <div class="beneficiario-header">
          <div class="beneficiario-info">
            <strong>{{ b.nombreBeneficiario }}</strong>
            <span class="text-muted">{{ b.rutFormateado }}</span>
          </div>
          <div class="beneficiario-cuenta">
            <span><Landmark :size="14" /> {{ b.nombreBanco || b.codBanco || '-' }}</span>
            <span>{{ tipoCuentaLabel(b.tipoCuenta) }}</span>
            <span>{{ formatCuenta(b.ctaEstado || b.ctaOtBanco) }}</span>
          </div>
          <button class="btn-sm" @click="abrirEditarCuenta(b)">
            <Pencil :size="14" /> Editar Cuenta
          </button>
        </div>

        <table v-if="b.retenciones?.length" class="data-table">
          <thead>
            <tr>
              <th>Monto</th>
              <th>Codigo Retencion</th>
              <th>Tipo Pago</th>
              <th>Periodo</th>
              <th>Estado</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="r in b.retenciones" :key="r.id">
              <td>${{ r.monto?.toLocaleString('es-CL') }}</td>
              <td>{{ r.codRetencion }}</td>
              <td>{{ r.tipoPago }}</td>
              <td>{{ r.periodoProceso }}</td>
              <td>
                <span :class="'estado estado-' + (r.estado || 'a').toLowerCase()">
                  {{ r.estado === 'A' ? 'Activo' : r.estado === 'I' ? 'Inactivo' : r.estado || '-' }}
                </span>
              </td>
            </tr>
          </tbody>
        </table>
        <p v-else class="text-muted" style="padding: 0.5rem 0; font-size: 0.85rem;">Sin retenciones activas.</p>
      </div>
    </section>

    <div class="actions">
      <router-link :to="`/funcionarios/${rut}/editar`" class="btn btn-primary">
        <Pencil :size="16" /> Editar Funcionario
      </router-link>
      <button class="btn btn-secondary" @click="imprimirFicha">
        <Printer :size="16" /> Imprimir Ficha
      </button>
      <button v-if="store.detalle.activo === 'S'" class="btn btn-danger" @click="showConfirmInactivar = true">
        <Ban :size="16" /> Inactivar
      </button>
      <router-link to="/funcionarios" class="btn btn-secondary">
        <ArrowLeft :size="16" /> Volver
      </router-link>
    </div>

    <!-- Modal inactivar funcionario -->
    <ConfirmModal
      v-model="showConfirmInactivar"
      title="Inactivar Funcionario"
      :message="`¿Esta seguro de inactivar a ${store.detalle.nombres} ${store.detalle.apellidoPaterno}? Esta accion cambiara su estado a Inactivo.`"
      confirmText="Si, inactivar"
      variant="danger"
      @confirm="ejecutarInactivar"
    />

    <!-- Modal editar cuenta bancaria -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showEditarCuenta" class="modal-overlay" @click.self="showEditarCuenta = false">
          <div class="modal-dialog modal-dialog-lg">
            <div class="modal-header">
              <Landmark :size="20" class="modal-icon primary" />
              <h3>Editar Cuenta Bancaria</h3>
            </div>
            <div class="modal-body">
              <p style="margin-bottom: 1rem; font-size: 0.85rem; color: var(--text-secondary);">
                Beneficiario: <strong>{{ cuentaForm.nombreBeneficiario }}</strong> ({{ cuentaForm.rutFormateado }})
              </p>
              <div class="form-grid">
                <div class="field">
                  <label>Banco</label>
                  <select v-model="cuentaForm.codBanco" @change="onBancoChange">
                    <option :value="null">-- Seleccionar banco --</option>
                    <option v-for="banco in bancos" :key="banco.codBanco" :value="banco.codBanco">
                      {{ banco.codBanco }} - {{ banco.nombreBanco }}
                    </option>
                  </select>
                </div>
                <div class="field">
                  <label>Tipo Cuenta</label>
                  <select v-model="cuentaForm.tipoCuenta">
                    <option :value="null">-- Seleccionar tipo --</option>
                    <option v-for="tc in tiposCuenta" :key="tc.codTipoCuenta" :value="tc.codTipoCuenta">
                      {{ tc.codTipoCuenta }} - {{ tc.descripcion }}
                    </option>
                  </select>
                </div>
                <div class="field" v-if="cuentaForm.codBanco === 12">
                  <label>Cuenta BancoEstado</label>
                  <input v-model="cuentaForm.ctaEstado" maxlength="11" placeholder="Ej: 41762633599" @input="limpiarCuenta('ctaEstado')" />
                </div>
                <div class="field" v-if="cuentaForm.codBanco && cuentaForm.codBanco !== 12">
                  <label>Cuenta Otro Banco</label>
                  <input v-model="cuentaForm.ctaOtBanco" maxlength="15" @input="limpiarCuenta('ctaOtBanco')" />
                </div>
              </div>
            </div>
            <div class="modal-footer">
              <button class="btn btn-secondary" @click="showEditarCuenta = false">Cancelar</button>
              <button class="btn btn-primary" @click="guardarCuenta" :disabled="guardandoCuenta">
                {{ guardandoCuenta ? 'Guardando...' : 'Guardar' }}
              </button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>
  </div>
  <div v-else-if="store.loading" class="loading">Cargando datos del funcionario...</div>
  <div v-else class="empty-state">
    <UserX :size="48" class="empty-icon" />
    <p>Funcionario no encontrado</p>
    <router-link to="/funcionarios" class="btn btn-secondary" style="margin-top:0.5rem">
      <ArrowLeft :size="16" /> Volver a la lista
    </router-link>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useFuncionariosStore } from '../stores/funcionarios.js'
import { beneficiariosApi, catalogosApi } from '../api/index.js'
import { formatCuenta } from '../composables/useFormato.js'
import AlertMessage from '../components/AlertMessage.vue'
import ConfirmModal from '../components/ConfirmModal.vue'
import {
  UserCheck, User, Users, Landmark, Scale,
  Pencil, Ban, ArrowLeft, CircleCheck, CircleX, UserX,
  DollarSign, Eye, Printer
} from 'lucide-vue-next'

const props = defineProps({ rut: { type: [String, Number], required: true } })
const store = useFuncionariosStore()
const router = useRouter()

const alertMsg = ref('')
const alertType = ref('info')
const showConfirmInactivar = ref(false)

// Editar cuenta bancaria
const showEditarCuenta = ref(false)
const guardandoCuenta = ref(false)
const bancos = ref([])
const tiposCuenta = ref([])
const cuentaForm = ref({
  rutBeneficiario: null,
  rutFormateado: '',
  nombreBeneficiario: '',
  codBanco: null,
  tipoCuenta: null,
  ctaEstado: '',
  ctaOtBanco: ''
})

// Suma los montos de todas las retenciones de todos los beneficiarios del funcionario
const montoTotalRetenciones = computed(() => {
  if (!store.detalle?.beneficiarios) return 0
  return store.detalle.beneficiarios.reduce((sum, b) => {
    const montosBenef = (b.retenciones || []).reduce((s, r) => s + (r.monto || 0), 0)
    return sum + montosBenef
  }, 0)
})

// Convierte el codigo numerico de tipo de cuenta a su descripcion legible
function tipoCuentaLabel(tc) {
  if (tc === 1) return 'Cuenta Corriente'
  if (tc === 2) return 'Ahorro / CuentaRUT'
  if (tc === 3) return 'Cuenta Vista'
  return '-'
}

// Al montar: carga el detalle del funcionario y los catalogos de bancos/tipos de cuenta
onMounted(async () => {
  store.detalle = null
  store.obtener(Number(props.rut))
  try {
    const [bancosRes, tcRes] = await Promise.all([
      catalogosApi.bancos(),
      catalogosApi.tiposCuenta()
    ])
    bancos.value = bancosRes.data
    tiposCuenta.value = tcRes.data
  } catch (e) {
    alertType.value = 'error'
    alertMsg.value = 'Error al cargar catálogos de bancos/tipos de cuenta.'
  }
})

// Ejecuta la inactivacion del funcionario y redirige a la lista tras exito
async function ejecutarInactivar() {
  const ok = await store.inactivar(Number(props.rut))
  if (ok) {
    alertType.value = 'success'
    alertMsg.value = 'Funcionario inactivado exitosamente. Redirigiendo...'
    setTimeout(() => router.push('/funcionarios'), 1500)
  } else {
    alertType.value = 'error'
    alertMsg.value = store.error || 'Error al inactivar el funcionario.'
  }
}

// Abre el modal de edicion de cuenta bancaria, precargando los datos del beneficiario seleccionado
function abrirEditarCuenta(b) {
  cuentaForm.value = {
    rutBeneficiario: b.rutBeneficiario,
    rutFormateado: b.rutFormateado || '',
    nombreBeneficiario: b.nombreBeneficiario || '',
    codBanco: b.codBanco || null,
    tipoCuenta: b.tipoCuenta || null,
    ctaEstado: b.ctaEstado ? String(b.ctaEstado).replace(/[^0-9]/g, '') : '',
    ctaOtBanco: b.ctaOtBanco ? String(b.ctaOtBanco).replace(/[^0-9]/g, '') : ''
  }
  showEditarCuenta.value = true
}

// Limpia los campos de cuenta al cambiar de banco (evita datos cruzados)
function onBancoChange() {
  cuentaForm.value.ctaEstado = ''
  cuentaForm.value.ctaOtBanco = ''
}

// Elimina caracteres no numericos del campo de cuenta ingresado
function limpiarCuenta(campo) {
  cuentaForm.value[campo] = cuentaForm.value[campo].replace(/[^0-9]/g, '')
}

function esc(str) {
  const div = document.createElement('div')
  div.textContent = str ?? ''
  return div.innerHTML
}

// Genera un HTML de ficha imprimible con los datos del funcionario, beneficiarios y retenciones
function imprimirFicha() {
  const d = store.detalle
  const fecha = new Date().toLocaleDateString('es-CL')

  let benefHtml = ''
  if (d.beneficiarios?.length) {
    for (const b of d.beneficiarios) {
      const banco = b.nombreBanco || b.codBanco || '-'
      const tc = tipoCuentaLabel(b.tipoCuenta)
      const cuenta = b.ctaEstado || b.ctaOtBanco || '-'

      let retHtml = ''
      if (b.retenciones?.length) {
        retHtml = `<table class="ret-table">
          <thead><tr><th>Monto</th><th>Cod. Retencion</th><th>Tipo Pago</th><th>Periodo</th><th>Estado</th></tr></thead>
          <tbody>${b.retenciones.map(r => `<tr>
            <td>$${(r.monto || 0).toLocaleString('es-CL')}</td>
            <td>${esc(r.codRetencion || '-')}</td>
            <td>${esc(r.tipoPago || '-')}</td>
            <td>${esc(r.periodoProceso || '-')}</td>
            <td>${r.estado === 'A' ? 'Activo' : 'Inactivo'}</td>
          </tr>`).join('')}</tbody></table>`
      } else {
        retHtml = '<p class="muted">Sin retenciones activas.</p>'
      }

      benefHtml += `
        <div class="benef-block">
          <div class="benef-header">
            <strong>${esc(b.nombreBeneficiario)}</strong> <span class="rut">${esc(b.rutFormateado)}</span>
          </div>
          <div class="benef-cuenta">Banco: ${esc(banco)} | Tipo: ${esc(tc)} | Cuenta: ${esc(cuenta)}</div>
          ${retHtml}
        </div>`
    }
  } else {
    benefHtml = '<p class="muted">Sin beneficiarios asociados.</p>'
  }

  const html = `<!DOCTYPE html><html><head><meta charset="utf-8">
<title>Ficha Funcionario - ${d.rutFormateado}</title>
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
  .benef-block { border: 1px solid #e2e8f0; border-radius: 4px; padding: 8px; margin-bottom: 8px; page-break-inside: avoid; }
  .benef-header { font-size: 11px; margin-bottom: 3px; }
  .benef-header .rut { color: #64748b; font-size: 10px; margin-left: 6px; }
  .benef-cuenta { font-size: 10px; color: #475569; margin-bottom: 6px; }
  .ret-table { width: 100%; border-collapse: collapse; font-size: 10px; }
  .ret-table th { background: #f1f5f9; text-align: left; padding: 3px 6px; border-bottom: 1px solid #e2e8f0; }
  .ret-table td { padding: 2px 6px; border-bottom: 1px solid #f1f5f9; }
  .muted { color: #94a3b8; font-size: 10px; font-style: italic; }
  .stats { display: flex; gap: 16px; margin-bottom: 12px; }
  .stat-box { border: 1px solid #e2e8f0; border-radius: 4px; padding: 6px 12px; text-align: center; }
  .stat-box .val { font-size: 16px; font-weight: bold; color: #2563eb; }
  .stat-box .lbl { font-size: 9px; color: #64748b; display: block; }
  @media print { body { padding: 10px; } }
</style></head><body>
<div class="fecha">Impreso: ${fecha}</div>
<h1>${esc(d.nombres)} ${esc(d.apellidoPaterno)} ${esc(d.apellidoMaterno)}</h1>
<p class="subtitle">RUT: ${esc(d.rutFormateado)} | <span class="estado ${d.activo === 'S' ? 'estado-a' : 'estado-i'}">${d.activo === 'S' ? 'Activo' : 'Inactivo'}</span></p>

<div class="stats">
  <div class="stat-box"><span class="val">${d.beneficiarios?.length || 0}</span><span class="lbl">Beneficiarios</span></div>
  <div class="stat-box"><span class="val">$${montoTotalRetenciones.value.toLocaleString('es-CL')}</span><span class="lbl">Monto Total</span></div>
</div>

<div class="section">
  <h2>Datos del Funcionario</h2>
  <dl>
    <dt>Apellido Paterno</dt><dd>${esc(d.apellidoPaterno || '-')}</dd>
    <dt>Apellido Materno</dt><dd>${esc(d.apellidoMaterno || '-')}</dd>
    <dt>Nombres</dt><dd>${esc(d.nombres || '-')}</dd>
    <dt>ID Sistema</dt><dd>${esc(d.idSistema || '-')}</dd>
  </dl>
</div>

<div class="section">
  <h2>Beneficiarios y Retenciones</h2>
  ${benefHtml}
</div>

<script>window.onload = function() { window.print(); window.close(); }<\/script>
</body></html>`

  const win = window.open('', '_blank', 'width=800,height=600')
  win.document.write(html)
  win.document.close()
}

// Envía la actualizacion de cuenta bancaria al backend y recarga el detalle del funcionario
async function guardarCuenta() {
  guardandoCuenta.value = true
  try {
    await beneficiariosApi.actualizar(cuentaForm.value.rutBeneficiario, {
      codBanco: cuentaForm.value.codBanco,
      tipoCuenta: cuentaForm.value.tipoCuenta,
      ctaEstado: cuentaForm.value.ctaEstado || null,
      ctaOtBanco: cuentaForm.value.ctaOtBanco || null
    })
    alertType.value = 'success'
    alertMsg.value = 'Cuenta bancaria actualizada exitosamente.'
    showEditarCuenta.value = false
    // Recargar detalle
    await store.obtener(Number(props.rut))
  } catch (e) {
    alertType.value = 'error'
    alertMsg.value = e.response?.data?.error || 'Error al actualizar la cuenta bancaria.'
  } finally {
    guardandoCuenta.value = false
  }
}
</script>

<style scoped>
.beneficiario-card {
  background: var(--bg-card);
  border: 1px solid var(--border-color);
  border-radius: var(--border-radius);
  padding: 1rem;
  margin-bottom: 1rem;
}

.beneficiario-header {
  display: flex;
  align-items: center;
  gap: 1rem;
  margin-bottom: 0.75rem;
  flex-wrap: wrap;
}

.beneficiario-info {
  display: flex;
  flex-direction: column;
  gap: 0.15rem;
  min-width: 180px;
}

.beneficiario-cuenta {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  font-size: 0.85rem;
  color: var(--text-secondary);
  flex: 1;
}

.beneficiario-cuenta span {
  display: inline-flex;
  align-items: center;
  gap: 0.25rem;
}

.text-muted {
  color: var(--text-muted);
  font-size: 0.83rem;
}

.modal-dialog-lg {
  max-width: 720px;
}

/* Dentro del modal, el form-grid usa una sola columna para aprovechar todo el ancho */
.modal-body .form-grid {
  grid-template-columns: 1fr;
}

/* Selects del modal ocupan el 100% del ancho disponible */
.modal-body select,
.modal-body input {
  width: 100%;
}

/* Reuse modal styles from ConfirmModal */
.modal-overlay {
  position: fixed; inset: 0; background: rgba(15, 23, 42, 0.5);
  display: flex; align-items: center; justify-content: center; z-index: 1000;
  backdrop-filter: blur(2px);
}

.modal-dialog, .modal-dialog-lg {
  background: #fff; border-radius: 14px;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.2);
  width: 92%; overflow: visible;
}

.modal-dialog { max-width: 460px; }

.modal-header {
  padding: 1.25rem 1.5rem 0.75rem;
  display: flex; align-items: center; gap: 0.6rem;
  border-bottom: 1px solid #f1f5f9;
  background: #fff;
  border-radius: 14px 14px 0 0;
}
.modal-header h3 { margin: 0; font-size: 1.05rem; color: #1e293b; }
.modal-icon.primary { color: #2563eb; }

.modal-body { padding: 1rem 1.5rem 1.25rem; background: #fff; }

.modal-footer {
  padding: 0.85rem 1.5rem; display: flex; justify-content: flex-end; gap: 0.6rem;
  border-top: 1px solid #f1f5f9; background: #f8fafc;
  border-radius: 0 0 14px 14px;
}

.modal-fade-enter-active { transition: opacity 0.2s ease; }
.modal-fade-leave-active { transition: opacity 0.15s ease; }
.modal-fade-enter-from, .modal-fade-leave-to { opacity: 0; }
</style>
