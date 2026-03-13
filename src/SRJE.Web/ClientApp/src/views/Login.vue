<template>
  <div class="login-page">
    <div class="login-card">
      <div class="login-header">
        <Scale :size="32" color="#2563eb" />
        <h1>SRJE</h1>
        <p>Sistema de Retenciones Judiciales</p>
      </div>

      <form @submit.prevent="handleLogin" class="login-form">
        <div class="login-field">
          <label for="usuario">Usuario</label>
          <input
            id="usuario"
            v-model="form.usuario"
            type="text"
            placeholder="Ingrese su usuario"
            required
            autofocus
          />
        </div>

        <div class="login-field">
          <label for="password">Contraseña</label>
          <input
            id="password"
            v-model="form.password"
            type="password"
            placeholder="Ingrese su contraseña"
            required
          />
        </div>

        <div v-if="error" class="login-error">
          {{ error }}
        </div>

        <button type="submit" class="btn btn-primary login-btn" :disabled="enviando">
          {{ enviando ? 'Ingresando...' : 'Ingresar' }}
        </button>
      </form>

      <div class="login-footer">
        <small>Entorno de desarrollo</small>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth.js'
import { Scale } from 'lucide-vue-next'

const router = useRouter()
const authStore = useAuthStore()

const form = reactive({ usuario: '', password: '' })
const error = ref('')
const enviando = ref(false)

async function handleLogin() {
  error.value = ''
  enviando.value = true
  try {
    await authStore.login(form.usuario, form.password)
    router.push('/')
  } catch (err) {
    error.value = err.response?.data?.error || 'Error al iniciar sesion'
  } finally {
    enviando.value = false
  }
}
</script>

<style scoped>
.login-page {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--bg-page);
  padding: 1rem;
}

.login-card {
  background: var(--bg-card);
  border-radius: var(--border-radius-lg);
  box-shadow: var(--shadow-lg);
  border: 1px solid var(--border-color);
  padding: 2.5rem;
  width: 100%;
  max-width: 400px;
}

.login-header {
  text-align: center;
  margin-bottom: 2rem;
}

.login-header h1 {
  font-size: 1.8rem;
  font-weight: 700;
  color: var(--text-primary);
  margin-top: 0.75rem;
  letter-spacing: -0.02em;
}

.login-header p {
  color: var(--text-secondary);
  font-size: 0.9rem;
  margin-top: 0.25rem;
}

.login-form {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

.login-field {
  display: flex;
  flex-direction: column;
}

.login-field label {
  font-size: 0.85rem;
  font-weight: 500;
  color: var(--text-secondary);
  margin-bottom: 0.4rem;
}

.login-field input {
  padding: 0.65rem 0.85rem;
  border: 1px solid var(--border-color);
  border-radius: var(--border-radius);
  font-size: 0.95rem;
  background: var(--bg-input);
  color: var(--text-primary);
  transition: border-color var(--transition), box-shadow var(--transition);
}

.login-field input:focus {
  outline: none;
  border-color: var(--color-primary);
  box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
}

.login-error {
  padding: 0.7rem 0.9rem;
  background: var(--color-danger-light);
  color: var(--color-danger);
  border-radius: var(--border-radius);
  border: 1px solid #fecaca;
  font-size: 0.88rem;
}

.login-btn {
  width: 100%;
  justify-content: center;
  padding: 0.7rem;
  font-size: 0.95rem;
  margin-top: 0.25rem;
}

.login-footer {
  text-align: center;
  margin-top: 1.5rem;
  color: var(--text-muted);
  font-size: 0.8rem;
}
</style>
