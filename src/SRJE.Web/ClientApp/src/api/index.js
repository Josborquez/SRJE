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
  actualizarRetencion: (rut, id, data) => api.put(`/beneficiarios/${rut}/retenciones/${id}`, data),
  buscar: (q) => api.get('/beneficiarios/buscar', { params: { q } }),
  exportarExcel: (estado) => api.get('/beneficiarios/exportar/excel', { params: { estado }, responseType: 'blob' }),
  exportarCsv: (estado) => api.get('/beneficiarios/exportar/csv', { params: { estado }, responseType: 'blob' }),
  listarCuentas: (rut) => api.get(`/beneficiarios/${rut}/cuentas`),
  agregarCuenta: (rut, data) => api.post(`/beneficiarios/${rut}/cuentas`, data),
  actualizarCuenta: (rut, id, data) => api.put(`/beneficiarios/${rut}/cuentas/${id}`, data),
  eliminarCuenta: (rut, id) => api.delete(`/beneficiarios/${rut}/cuentas/${id}`)
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
  generarTemge: (periodo) => api.get('/archivos/temge/generar', { params: { periodo }, responseType: 'blob' }),
  compararAuditoria: (file) => {
    const form = new FormData()
    form.append('archivo', file)
    return api.post('/archivos/auditoria/comparar', form)
  },
  exportarAuditoriaExcel: (file) => {
    const form = new FormData()
    form.append('archivo', file)
    return api.post('/archivos/auditoria/exportar-excel', form, { responseType: 'blob' })
  },
  exportarAuditoriaCsv: (file) => {
    const form = new FormData()
    form.append('archivo', file)
    return api.post('/archivos/auditoria/exportar-csv', form, { responseType: 'blob' })
  }
}

// Mantenedores
export const mantenedoresApi = {
  // Bancos
  listarBancos: () => api.get('/mantenedores/bancos'),
  obtenerBanco: (cod) => api.get(`/mantenedores/bancos/${cod}`),
  crearBanco: (data) => api.post('/mantenedores/bancos', data),
  actualizarBanco: (cod, data) => api.put(`/mantenedores/bancos/${cod}`, data),
  toggleBanco: (cod) => api.patch(`/mantenedores/bancos/${cod}/toggle`),
  // Tipos de cuenta
  listarTiposCuenta: () => api.get('/mantenedores/tipos-cuenta'),
  obtenerTipoCuenta: (cod) => api.get(`/mantenedores/tipos-cuenta/${cod}`),
  crearTipoCuenta: (data) => api.post('/mantenedores/tipos-cuenta', data),
  actualizarTipoCuenta: (cod, data) => api.put(`/mantenedores/tipos-cuenta/${cod}`, data),
  toggleTipoCuenta: (cod) => api.patch(`/mantenedores/tipos-cuenta/${cod}/toggle`)
}

// Funcionarios
export const funcionariosApi = {
  listar: (params) => api.get('/funcionarios', { params }),
  obtener: (rut) => api.get(`/funcionarios/${rut}`),
  actualizar: (rut, data) => api.put(`/funcionarios/${rut}`, data),
  inactivar: (rut) => api.delete(`/funcionarios/${rut}`),
  stats: () => api.get('/funcionarios/stats')
}

// Usuarios del sistema (solo admin)
export const usuariosApi = {
  listar: () => api.get('/usuarios'),
  obtener: (usuario) => api.get(`/usuarios/${usuario}`),
  crear: (data) => api.post('/usuarios', data),
  actualizar: (usuario, data) => api.put(`/usuarios/${usuario}`, data),
  cambiarPassword: (usuario, data) => api.put(`/usuarios/${usuario}/password`, data),
  toggle: (usuario) => api.patch(`/usuarios/${usuario}/toggle`),
  accesos: (params) => api.get('/usuarios/accesos', { params }),
  cargas: (params) => api.get('/usuarios/cargas', { params })
}

// Catalogos
export const catalogosApi = {
  bancos: () => api.get('/catalogos/bancos'),
  tiposRetencion: () => api.get('/catalogos/tipos-retencion'),
  tiposCuenta: () => api.get('/catalogos/tipos-cuenta')
}

export default api
