import { createRouter, createWebHistory } from 'vue-router'

const routes = [
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
  }
]

export default createRouter({
  history: createWebHistory(),
  routes
})
