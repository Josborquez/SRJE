<template>
  <div class="rut-input">
    <label v-if="label">{{ label }}</label>
    <div class="rut-field">
      <div class="rut-input-wrapper" :class="{ 'invalid': touched && !isValid, 'valid': touched && isValid }">
        <Hash :size="16" class="rut-prefix-icon" />
        <input
          type="text"
          :value="displayValue"
          @input="onInput"
          @blur="onBlur"
          :placeholder="placeholder"
          maxlength="12"
        />
      </div>
      <span class="dv-badge" v-if="dv">-{{ dv }}</span>
    </div>
    <small v-if="touched && !isValid" class="error-msg">
      <CircleAlert :size="12" /> RUT invalido
    </small>
    <small v-if="touched && isValid" class="success-msg">
      <CircleCheck :size="12" /> RUT valido
    </small>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import { calcularDv, validarRut, formatearRut } from '../composables/useRut.js'
import { Hash, CircleAlert, CircleCheck } from 'lucide-vue-next'

const props = defineProps({
  modelValue: { type: [Number, String], default: '' },
  label: { type: String, default: '' },
  placeholder: { type: String, default: 'Ej: 7.051.537' }
})

const emit = defineEmits(['update:modelValue', 'rutValidado'])

const rawValue = ref('')
const dv = ref('')
const touched = ref(false)

const isValid = computed(() => {
  const num = parseInt(rawValue.value.replace(/\./g, ''))
  return !isNaN(num) && num > 0 && validarRut(num, dv.value)
})

const displayValue = computed(() => {
  const clean = rawValue.value.replace(/\./g, '')
  if (!clean) return ''
  return Number(clean).toLocaleString('es-CL')
})

function onInput(e) {
  const clean = e.target.value.replace(/[^0-9]/g, '')
  rawValue.value = clean
  if (clean.length >= 7) {
    const num = parseInt(clean)
    dv.value = calcularDv(num)
    emit('update:modelValue', num)
    emit('rutValidado', { rut: num, dv: dv.value, valido: validarRut(num, dv.value) })
  } else {
    dv.value = ''
  }
}

function onBlur() {
  touched.value = true
}

watch(() => props.modelValue, (val) => {
  if (val && typeof val === 'number') {
    rawValue.value = String(val)
    dv.value = calcularDv(val)
  }
}, { immediate: true })
</script>

<style scoped>
.rut-input { margin-bottom: 0.75rem; }
.rut-input label {
  display: block;
  font-size: 0.83rem;
  margin-bottom: 0.3rem;
  font-weight: 500;
  color: var(--text-secondary, #64748b);
}
.rut-field { display: flex; align-items: center; gap: 0.3rem; }

.rut-input-wrapper {
  display: flex;
  align-items: center;
  gap: 0.3rem;
  padding: 0 0.6rem;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  background: #fff;
  transition: border-color 0.2s, box-shadow 0.2s;
}
.rut-input-wrapper:focus-within {
  border-color: #2563eb;
  box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
}
.rut-input-wrapper.valid {
  border-color: #16a34a;
}
.rut-input-wrapper.valid:focus-within {
  box-shadow: 0 0 0 3px rgba(22, 163, 74, 0.1);
}
.rut-input-wrapper.invalid {
  border-color: #dc2626;
}
.rut-input-wrapper.invalid:focus-within {
  box-shadow: 0 0 0 3px rgba(220, 38, 38, 0.1);
}
.rut-prefix-icon { color: #94a3b8; flex-shrink: 0; }
.rut-input-wrapper input {
  padding: 0.5rem 0.3rem;
  border: none;
  font-size: 0.95rem;
  width: 140px;
  outline: none;
  background: transparent;
}
.dv-badge { font-weight: 700; font-size: 1.1rem; color: #334155; }
.error-msg {
  color: #dc2626;
  font-size: 0.78rem;
  display: flex;
  align-items: center;
  gap: 0.2rem;
  margin-top: 0.2rem;
}
.success-msg {
  color: #16a34a;
  font-size: 0.78rem;
  display: flex;
  align-items: center;
  gap: 0.2rem;
  margin-top: 0.2rem;
}
</style>
