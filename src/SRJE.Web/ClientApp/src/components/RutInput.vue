<template>
  <div class="rut-input">
    <label v-if="label">{{ label }}</label>
    <div class="rut-field">
      <input
        type="text"
        :value="displayValue"
        @input="onInput"
        @blur="onBlur"
        :placeholder="placeholder"
        :class="{ 'invalid': touched && !isValid, 'valid': touched && isValid }"
        maxlength="12"
      />
      <span class="dv-badge" v-if="dv">-{{ dv }}</span>
    </div>
    <small v-if="touched && !isValid" class="error-msg">RUT invalido</small>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import { calcularDv, validarRut, formatearRut } from '../composables/useRut.js'

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
.rut-input label { display: block; font-size: 0.85rem; margin-bottom: 0.25rem; font-weight: 500; }
.rut-field { display: flex; align-items: center; gap: 0.25rem; }
.rut-field input {
  padding: 0.5rem; border: 1px solid #ccc; border-radius: 4px;
  font-size: 0.95rem; width: 160px;
}
.rut-field input.valid { border-color: #4caf50; }
.rut-field input.invalid { border-color: #f44336; }
.dv-badge { font-weight: bold; font-size: 1.1rem; }
.error-msg { color: #f44336; font-size: 0.8rem; }
</style>
