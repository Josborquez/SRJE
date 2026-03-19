<template>
  <!-- Login page: sin layout -->
  <router-view v-if="route.name === 'login'" />

  <!-- App layout: con sidebar -->
  <template v-else>
    <!-- Mobile header -->
    <div class="mobile-header">
      <button class="hamburger-btn" @click="sidebarOpen = !sidebarOpen" aria-label="Menu">
        <Menu :size="24" />
      </button>
      <h2>SRJE</h2>
    </div>

    <!-- Sidebar overlay for mobile -->
    <div
      class="sidebar-overlay"
      :class="{ visible: sidebarOpen }"
      @click="sidebarOpen = false"
    ></div>

    <div class="app-layout">
      <nav class="sidebar" :class="{ 'sidebar-open': sidebarOpen }">
        <div class="sidebar-header">
          <div class="sidebar-brand">
            <Scale :size="22" color="#60a5fa" />
            <div>
              <h2>SRJE</h2>
              <small>Retenciones Judiciales</small>
            </div>
          </div>
        </div>
        <ul class="nav-menu">
          <li>
            <router-link to="/" @click="sidebarOpen = false">
              <LayoutDashboard :size="18" class="nav-icon" />
              Dashboard
            </router-link>
          </li>
          <li class="nav-section">Beneficiarios</li>
          <li>
            <router-link to="/beneficiarios" @click="sidebarOpen = false">
              <Users :size="18" class="nav-icon" />
              Lista
            </router-link>
          </li>
          <li>
            <router-link to="/beneficiarios/nuevo" @click="sidebarOpen = false">
              <UserPlus :size="18" class="nav-icon" />
              Nuevo
            </router-link>
          </li>
          <li class="nav-section">Archivos</li>
          <li>
            <router-link to="/archivos/remuneraciones" @click="sidebarOpen = false">
              <FileSpreadsheet :size="18" class="nav-icon" />
              Importar Remuneraciones
            </router-link>
          </li>
          <li>
            <router-link to="/archivos/temge" @click="sidebarOpen = false">
              <FileInput :size="18" class="nav-icon" />
              Importar TEMGE
            </router-link>
          </li>
          <li>
            <router-link to="/archivos/nuevas-cuentas" @click="sidebarOpen = false">
              <CreditCard :size="18" class="nav-icon" />
              Nuevas Cuentas
            </router-link>
          </li>
          <li>
            <router-link to="/archivos/generar-temge" @click="sidebarOpen = false">
              <FileDown :size="18" class="nav-icon" />
              Generar TEMGE
            </router-link>
          </li>
          <li>
            <router-link to="/archivos/auditoria-beneficiarios" @click="sidebarOpen = false">
              <FileSearch :size="18" class="nav-icon" />
              Auditoria Beneficiarios
            </router-link>
          </li>
          <li class="nav-section">Mantenedores</li>
          <li>
            <router-link to="/mantenedores/bancos" @click="sidebarOpen = false">
              <Building2 :size="18" class="nav-icon" />
              Bancos
            </router-link>
          </li>
          <li>
            <router-link to="/mantenedores/tipos-cuenta" @click="sidebarOpen = false">
              <Wallet :size="18" class="nav-icon" />
              Tipos de Cuenta
            </router-link>
          </li>
        </ul>
        <div class="sidebar-footer">
          <div class="user-info">
            <User :size="16" />
            <span>{{ authStore.usuario?.nombreCompleto || authStore.usuario?.usuario }}</span>
          </div>
          <button class="logout-btn" @click="handleLogout" title="Cerrar sesion">
            <LogOut :size="16" />
          </button>
        </div>
      </nav>
      <main class="main-content">
        <router-view />
      </main>
    </div>
  </template>
</template>

<script setup>
import { ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from './stores/auth.js'
import {
  Scale,
  LayoutDashboard,
  Users,
  UserPlus,
  FileSpreadsheet,
  FileInput,
  CreditCard,
  FileDown,
  FileSearch,
  Menu,
  User,
  LogOut,
  Building2,
  Wallet
} from 'lucide-vue-next'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const sidebarOpen = ref(false)

async function handleLogout() {
  await authStore.logout()
  router.push('/login')
}
</script>

<style scoped>
.sidebar-footer {
  padding: 0.75rem 1.25rem;
  border-top: 1px solid rgba(255, 255, 255, 0.08);
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.5rem;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: #94a3b8;
  font-size: 0.82rem;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  min-width: 0;
}

.logout-btn {
  background: none;
  border: none;
  color: #94a3b8;
  cursor: pointer;
  padding: 0.35rem;
  border-radius: 6px;
  display: flex;
  align-items: center;
  transition: all 0.2s ease;
  flex-shrink: 0;
}

.logout-btn:hover {
  color: #ef4444;
  background: rgba(239, 68, 68, 0.1);
}
</style>
