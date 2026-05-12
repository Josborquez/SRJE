import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { beneficiariosApi } from '../api/index.js'

export const useBeneficiariosStore = defineStore('beneficiarios', () => {
  const items = ref([])
  const totalCount = ref(0)
  const totalInscritos = ref(0)
  const totalActivos = ref(0)
  const totalInactivos = ref(0)
  const page = ref(1)
  const pageSize = ref(20)
  const loading = ref(false)
  const error = ref(null)
  const detalle = ref(null)

  const totalPages = computed(() => Math.ceil(totalCount.value / pageSize.value))

  async function listar(params = {}) {
    loading.value = true
    error.value = null
    try {
      const { data } = await beneficiariosApi.listar({
        page: page.value,
        pageSize: pageSize.value,
        ...params
      })
      items.value = data.items
      totalCount.value = data.totalCount
      totalInscritos.value = data.totalInscritos
      totalActivos.value = data.totalActivos
      totalInactivos.value = data.totalInactivos
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
      const { data } = await beneficiariosApi.obtener(rut)
      detalle.value = data
      return data
    } catch (e) {
      error.value = e.response?.data?.error || e.message
      return null
    } finally {
      loading.value = false
    }
  }

  async function crear(beneficiario) {
    loading.value = true
    error.value = null
    try {
      const { data } = await beneficiariosApi.crear(beneficiario)
      return data
    } catch (e) {
      error.value = e.response?.data?.error || e.message
      throw e
    } finally {
      loading.value = false
    }
  }

  async function actualizar(rut, beneficiario) {
    loading.value = true
    error.value = null
    try {
      const { data } = await beneficiariosApi.actualizar(rut, beneficiario)
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
      await beneficiariosApi.inactivar(rut)
      return true
    } catch (e) {
      error.value = e.response?.data?.error || e.message
      return false
    } finally {
      loading.value = false
    }
  }

  function $reset() {
    items.value = []
    totalCount.value = 0
    totalInscritos.value = 0
    totalActivos.value = 0
    totalInactivos.value = 0
    page.value = 1
    loading.value = false
    error.value = null
    detalle.value = null
  }

  return {
    items, totalCount, totalInscritos, totalActivos, totalInactivos,
    page, pageSize, loading, error, detalle, totalPages,
    listar, obtener, crear, actualizar, inactivar, $reset
  }
})
