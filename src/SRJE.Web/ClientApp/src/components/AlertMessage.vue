<template>
  <Transition name="alert-fade">
    <div v-if="visible" :class="['alert', `alert-${type}`]" role="alert">
      <span class="alert-icon">{{ icon }}</span>
      <span class="alert-text">{{ message }}</span>
      <button class="alert-close" @click="close" aria-label="Cerrar">&times;</button>
    </div>
  </Transition>
</template>

<script setup>
import { ref, computed, watch } from 'vue'

const props = defineProps({
  message: { type: String, default: '' },
  type: { type: String, default: 'info', validator: v => ['success', 'error', 'warning', 'info'].includes(v) },
  duration: { type: Number, default: 5000 },
  autoClose: { type: Boolean, default: true }
})

const emit = defineEmits(['close'])
const visible = ref(false)
let timer = null

const icon = computed(() => {
  const icons = { success: '\u2713', error: '\u2717', warning: '\u26A0', info: '\u2139' }
  return icons[props.type] || icons.info
})

watch(() => props.message, (val) => {
  if (val) {
    visible.value = true
    if (timer) clearTimeout(timer)
    if (props.autoClose && props.duration > 0) {
      timer = setTimeout(close, props.duration)
    }
  }
}, { immediate: true })

function close() {
  visible.value = false
  if (timer) clearTimeout(timer)
  emit('close')
}
</script>

<style scoped>
.alert {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.75rem 1rem;
  border-radius: 6px;
  margin-bottom: 1rem;
  font-size: 0.9rem;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.1);
}

.alert-success { background: #e8f5e9; color: #2e7d32; border-left: 4px solid #4caf50; }
.alert-error { background: #fce4ec; color: #c62828; border-left: 4px solid #f44336; }
.alert-warning { background: #fff3e0; color: #e65100; border-left: 4px solid #ff9800; }
.alert-info { background: #e3f2fd; color: #1565c0; border-left: 4px solid #2196f3; }

.alert-icon { font-size: 1.1rem; flex-shrink: 0; }
.alert-text { flex: 1; }
.alert-close {
  background: none; border: none; font-size: 1.2rem; cursor: pointer;
  color: inherit; opacity: 0.6; padding: 0 0.25rem; line-height: 1;
}
.alert-close:hover { opacity: 1; }

.alert-fade-enter-active { transition: all 0.3s ease; }
.alert-fade-leave-active { transition: all 0.2s ease; }
.alert-fade-enter-from { opacity: 0; transform: translateY(-10px); }
.alert-fade-leave-to { opacity: 0; transform: translateY(-5px); }
</style>
