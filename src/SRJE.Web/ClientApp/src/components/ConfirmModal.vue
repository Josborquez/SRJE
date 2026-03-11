<template>
  <Teleport to="body">
    <Transition name="modal-fade">
      <div v-if="modelValue" class="modal-overlay" @click.self="cancel">
        <div class="modal-dialog">
          <div class="modal-header">
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

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  title: { type: String, default: 'Confirmar accion' },
  message: { type: String, default: 'Esta seguro de realizar esta accion?' },
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
  position: fixed; inset: 0; background: rgba(0, 0, 0, 0.5);
  display: flex; align-items: center; justify-content: center; z-index: 1000;
}

.modal-dialog {
  background: #fff; border-radius: 10px; box-shadow: 0 8px 32px rgba(0, 0, 0, 0.2);
  max-width: 440px; width: 90%; overflow: hidden;
}

.modal-header {
  padding: 1rem 1.25rem 0.5rem; border-bottom: 1px solid #eee;
}
.modal-header h3 { margin: 0; font-size: 1.1rem; color: #333; }

.modal-body { padding: 1rem 1.25rem; }
.modal-body p { margin: 0; color: #555; line-height: 1.5; }

.modal-footer {
  padding: 0.75rem 1.25rem; display: flex; justify-content: flex-end; gap: 0.5rem;
  border-top: 1px solid #eee; background: #fafafa;
}

.btn {
  padding: 0.5rem 1.1rem; border: none; border-radius: 5px;
  cursor: pointer; font-size: 0.9rem; font-weight: 500;
}
.btn-primary { background: #1976d2; color: #fff; }
.btn-primary:hover { background: #1565c0; }
.btn-danger { background: #d32f2f; color: #fff; }
.btn-danger:hover { background: #c62828; }
.btn-secondary { background: #e0e0e0; color: #333; }
.btn-secondary:hover { background: #d5d5d5; }

.modal-fade-enter-active { transition: opacity 0.2s ease; }
.modal-fade-leave-active { transition: opacity 0.15s ease; }
.modal-fade-enter-from, .modal-fade-leave-to { opacity: 0; }
</style>
