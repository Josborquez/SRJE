import axios from 'axios'

const api = axios.create({
  baseURL: '/api',
  timeout: 30000,
  withCredentials: true
})

// Interceptor: redirigir a login si la sesion expiro (401)
api.interceptors.response.use(
  response => response,
  error => {
    if (error.response?.status === 401 && !error.config.url?.includes('/auth/')) {
      window.location.href = '/login'
    }
    return Promise.reject(error)
  }
)

// Auth
export const authApi = {
  login: (data) => api.post('/auth/login', data),
  logout: () => api.post('/auth/logout'),
  me: () => api.get('/auth/me')
}

// Beneficiarios
export const beneficiariosApi = {
  listar: (params) => api.get('/beneficiarios', { params }),
  obtener: (rut) => api.get(`/beneficiarios/${rut}`),
  crear: (data) => api.post('/beneficiarios', data),
  actualizar: (rut, data) => api.put(`/beneficiarios/${rut}`, data),
  inactivar: (rut) => api.delete(`/beneficiarios/${rut}`),
  retenciones: (rut) => api.get(`/beneficiarios/${rut}/retenciones`),
  buscar: (q) => api.get('/beneficiarios/buscar', { params: { q } })
}

// Archivos
export const archivosApi = {
  previewRemuneraciones: (file) => {
    const form = new FormData()
    form.append('archivo', file)
    return api.post('/archivos/remuneraciones/preview', form)
  },
  confirmarRemuneraciones: (data) => api.post('/archivos/remuneraciones/confirmar', data),
  previewTemge: (file) => {
    const form = new FormData()
    form.append('archivo', file)
    return api.post('/archivos/temge/preview', form)
  },
  confirmarTemge: (data) => api.post('/archivos/temge/confirmar', data),
  previewNuevasCuentas: (file) => {
    const form = new FormData()
    form.append('archivo', file)
    return api.post('/archivos/nuevas-cuentas/preview', form)
  },
  confirmarNuevasCuentas: (data) => api.post('/archivos/nuevas-cuentas/confirmar', data),
  generarTemge: () => api.get('/archivos/temge/generar', { responseType: 'blob' })
}

// Catalogos
export const catalogosApi = {
  bancos: () => api.get('/catalogos/bancos'),
  tiposRetencion: () => api.get('/catalogos/tipos-retencion'),
  tiposCuenta: () => api.get('/catalogos/tipos-cuenta')
}

export default api
