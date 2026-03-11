<template>
  <Transition name="alert-fade">
    <div v-if="visible" :class="['alert', `alert-${type}`]" role="alert">
      <component :is="iconComponent" :size="18" class="alert-icon" />
      <span class="alert-text">{{ message }}</span>
      <button class="alert-close" @click="close" aria-label="Cerrar">
        <X :size="16" />
      </button>
    </div>
  </Transition>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import { CheckCircle, XCircle, AlertTriangle, Info, X } from 'lucide-vue-next'

const props = defineProps({
  message: { type: String, default: '' },
  type: { type: String, default: 'info', validator: v => ['success', 'error', 'warning', 'info'].includes(v) },
  duration: { type: Number, default: 5000 },
  autoClose: { type: Boolean, default: true }
})

const emit = defineEmits(['close'])
const visible = ref(false)
let timer = null

const iconComponent = computed(() => {
  const icons = { success: CheckCircle, error: XCircle, warning: AlertTriangle, info: Info }
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
  gap: 0.6rem;
  padding: 0.85rem 1.1rem;
  border-radius: 10px;
  margin-bottom: 1rem;
  font-size: 0.9rem;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
}

.alert-success { background: #f0fdf4; color: #166534; border: 1px solid #bbf7d0; }
.alert-error { background: #fef2f2; color: #991b1b; border: 1px solid #fecaca; }
.alert-warning { background: #fffbeb; color: #92400e; border: 1px solid #fde68a; }
.alert-info { background: #f0f9ff; color: #075985; border: 1px solid #bae6fd; }

.alert-icon { flex-shrink: 0; }
.alert-text { flex: 1; line-height: 1.4; }
.alert-close {
  background: none; border: none; cursor: pointer;
  color: inherit; opacity: 0.5; padding: 0.15rem;
  display: flex; align-items: center;
  border-radius: 4px; transition: all 0.2s;
}
.alert-close:hover { opacity: 1; background: rgba(0,0,0,0.05); }

.alert-fade-enter-active { transition: all 0.3s ease; }
.alert-fade-leave-active { transition: all 0.2s ease; }
.alert-fade-enter-from { opacity: 0; transform: translateY(-10px); }
.alert-fade-leave-to { opacity: 0; transform: translateY(-5px); }
</style>
