import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { funcionariosApi } from '../api/index.js'

export const useFuncionariosStore = defineStore('funcionarios', () => {
  const items = ref([])
  const totalCount = ref(0)
  const page = ref(1)
  const pageSize = ref(20)
  const loading = ref(false)
  const error = ref(null)
  const detalle = ref(null)
  const stats = ref(null)

  const totalPages = computed(() => Math.ceil(totalCount.value / pageSize.value))

  async function listar(params = {}) {
    loading.value = true
    error.value = null
    try {
      const { data } = await funcionariosApi.listar({
        page: page.value,
        pageSize: pageSize.value,
        ...params
      })
      items.value = data.items
      totalCount.value = data.totalCount
    } catch (e) {
      error.value = e.response?.data?.error || e.message
    } finally {
      loading.value = false
    }
  }

  async function obtener(rut) {
    loading.value = true
    error.value = null
    try {
      const { data } = await funcionariosApi.obtener(rut)
      detalle.value = data
      return data
    } catch (e) {
      error.value = e.response?.data?.error || e.message
      return null
    } finally {
      loading.value = false
    }
  }

  async function actualizar(rut, funcionario) {
    loading.value = true
    error.value = null
    try {
      const { data } = await funcionariosApi.actualizar(rut, funcionario)
      return data
    } catch (e) {
      error.value = e.response?.data?.error || e.message
      throw e
    } finally {
      loading.value = false
    }
  }

  async function inactivar(rut) {
    loading.value = true
    error.value = null
    try {
      await funcionariosApi.inactivar(rut)
      return true
    } catch (e) {
      error.value = e.response?.data?.error || e.message
      return false
    } finally {
      loading.value = false
    }
  }

  async function cargarStats() {
    try {
      const { data } = await funcionariosApi.stats()
      stats.value = data
    } catch (e) {
      error.value = e.response?.data?.error || e.message
    }
  }

  return {
    items, totalCount,
    page, pageSize, loading, error, detalle, stats, totalPages,
    listar, obtener, actualizar, inactivar, cargarStats
  }
})
