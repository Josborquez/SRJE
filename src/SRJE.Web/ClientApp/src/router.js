import { createRouter, createWebHistory } from 'vue-router'

const routes = [
  {
    path: '/login',
    name: 'login',
    component: () => import('./views/Login.vue'),
    meta: { public: true }
  },
  {
    path: '/',
    name: 'dashboard',
    component: () => import('./views/Dashboard.vue')
  },
  {
    path: '/beneficiarios',
    name: 'beneficiarios',
    component: () => import('./views/ListaBeneficiarios.vue')
  },
  {
    path: '/beneficiarios/nuevo',
    name: 'nuevo-beneficiario',
    component: () => import('./views/FichaBeneficiario.vue')
  },
  {
    path: '/beneficiarios/:rut',
    name: 'detalle-beneficiario',
    component: () => import('./views/DetalleBeneficiario.vue'),
    props: true
  },
  {
    path: '/beneficiarios/:rut/editar',
    name: 'editar-beneficiario',
    component: () => import('./views/FichaBeneficiario.vue'),
    props: true
  },
  {
    path: '/funcionarios',
    name: 'funcionarios',
    component: () => import('./views/ListaFuncionarios.vue')
  },
  {
    path: '/funcionarios/:rut',
    name: 'detalle-funcionario',
    component: () => import('./views/DetalleFuncionario.vue'),
    props: true
  },
  {
    path: '/funcionarios/:rut/editar',
    name: 'editar-funcionario',
    component: () => import('./views/FichaFuncionario.vue'),
    props: true
  },
  {
    path: '/archivos/remuneraciones',
    name: 'importar-remuneraciones',
    component: () => import('./views/ImportarRemuneraciones.vue')
  },
  {
    path: '/archivos/temge',
    name: 'importar-temge',
    component: () => import('./views/ImportarTemge.vue')
  },
  {
    path: '/archivos/nuevas-cuentas',
    name: 'importar-nuevas-cuentas',
    component: () => import('./views/ImportarNuevasCuentas.vue')
  },
  {
    path: '/archivos/generar-temge',
    name: 'generar-temge',
    component: () => import('./views/GenerarTemge.vue')
  },
  {
    path: '/archivos/auditoria-beneficiarios',
    name: 'auditoria-beneficiarios',
    component: () => import('./views/AuditoriaBeneficiarios.vue')
  },
  {
    path: '/mantenedores/bancos',
    name: 'mantenedor-bancos',
    component: () => import('./views/MantenedorBancos.vue')
  },
  {
    path: '/mantenedores/tipos-cuenta',
    name: 'mantenedor-tipos-cuenta',
    component: () => import('./views/MantenedorTiposCuenta.vue')
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes,
  scrollBehavior(to, from, savedPosition) {
    if (savedPosition) return savedPosition
    return { top: 0 }
  }
})

// Guard de navegacion: redirige a login si no esta autenticado
router.beforeEach(async (to) => {
  if (to.meta.public) return true

  // Importar store dinamicamente para evitar dependencia circular
  const { useAuthStore } = await import('./stores/auth.js')
  const authStore = useAuthStore()

  // Primera carga: verificar sesion con el backend
  if (authStore.cargando) {
    await authStore.verificarSesion()
  }

  if (!authStore.estaAutenticado()) {
    return { name: 'login' }
  }

  return true
})

export default router
