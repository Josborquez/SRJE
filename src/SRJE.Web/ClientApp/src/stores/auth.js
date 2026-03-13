import { defineStore } from 'pinia'
import { ref } from 'vue'
import api from '../api/index.js'

export const useAuthStore = defineStore('auth', () => {
  const usuario = ref(null)
  const cargando = ref(true)

  async function verificarSesion() {
    try {
      const { data } = await api.get('/auth/me')
      usuario.value = data
    } catch {
      usuario.value = null
    } finally {
      cargando.value = false
    }
  }

  async function login(user, password) {
    const { data } = await api.post('/auth/login', {
      usuario: user,
      password
    })
    usuario.value = data
    return data
  }

  async function logout() {
    await api.post('/auth/logout')
    usuario.value = null
  }

  const estaAutenticado = () => !!usuario.value

  return { usuario, cargando, verificarSesion, login, logout, estaAutenticado }
})
