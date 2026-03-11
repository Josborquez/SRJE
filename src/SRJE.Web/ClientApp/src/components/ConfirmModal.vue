<template>
  <Teleport to="body">
    <Transition name="modal-fade">
      <div v-if="modelValue" class="modal-overlay" @click.self="cancel">
        <div class="modal-dialog">
          <div class="modal-header">
            <AlertTriangle v-if="variant === 'danger'" :size="20" class="modal-icon danger" />
            <Info v-else :size="20" class="modal-icon primary" />
            <h3>{{ title }}</h3>
          </div>
          <div class="modal-body">
            <p>{{ message }}</p>
          </div>
          <div class="modal-footer">
            <button class="btn btn-secondary" @click="cancel">{{ cancelText }}</button>
            <button :class="['btn', confirmClass]" @click="confirm">{{ confirmText }}</button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
import { computed } from 'vue'
import { AlertTriangle, Info } from 'lucide-vue-next'

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  title: { type: String, default: 'Confirmar accion' },
  message: { type: String, default: '¿Esta seguro de realizar esta accion?' },
  confirmText: { type: String, default: 'Confirmar' },
  cancelText: { type: String, default: 'Cancelar' },
  variant: { type: String, default: 'primary', validator: v => ['primary', 'danger'].includes(v) }
})

const emit = defineEmits(['update:modelValue', 'confirm', 'cancel'])

const confirmClass = computed(() => props.variant === 'danger' ? 'btn-danger' : 'btn-primary')

function confirm() {
  emit('confirm')
  emit('update:modelValue', false)
}

function cancel() {
  emit('cancel')
  emit('update:modelValue', false)
}
</script>

<style scoped>
.modal-overlay {
  position: fixed; inset: 0; background: rgba(15, 23, 42, 0.5);
  display: flex; align-items: center; justify-content: center; z-index: 1000;
  backdrop-filter: blur(2px);
}

.modal-dialog {
  background: #fff; border-radius: 14px;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.2);
  max-width: 460px; width: 92%; overflow: hidden;
}

.modal-header {
  padding: 1.25rem 1.5rem 0.75rem;
  display: flex; align-items: center; gap: 0.6rem;
  border-bottom: 1px solid #f1f5f9;
}
.modal-header h3 { margin: 0; font-size: 1.05rem; color: #1e293b; }
.modal-icon.danger { color: #dc2626; }
.modal-icon.primary { color: #2563eb; }

.modal-body { padding: 1rem 1.5rem 1.25rem; }
.modal-body p { margin: 0; color: #64748b; line-height: 1.6; font-size: 0.92rem; }

.modal-footer {
  padding: 0.85rem 1.5rem; display: flex; justify-content: flex-end; gap: 0.6rem;
  border-top: 1px solid #f1f5f9; background: #f8fafc;
}

.btn {
  padding: 0.5rem 1.1rem; border: none; border-radius: 8px;
  cursor: pointer; font-size: 0.88rem; font-weight: 500;
  transition: all 0.2s;
}
.btn-primary { background: #2563eb; color: #fff; }
.btn-primary:hover { background: #1d4ed8; }
.btn-danger { background: #dc2626; color: #fff; }
.btn-danger:hover { background: #b91c1c; }
.btn-secondary { background: #f1f5f9; color: #334155; border: 1px solid #e2e8f0; }
.btn-secondary:hover { background: #e2e8f0; }

.modal-fade-enter-active { transition: opacity 0.2s ease; }
.modal-fade-leave-active { transition: opacity 0.15s ease; }
.modal-fade-enter-from, .modal-fade-leave-to { opacity: 0; }
</style>
