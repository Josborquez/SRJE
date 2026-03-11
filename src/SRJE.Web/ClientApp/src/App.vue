<template>
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
      </ul>
    </nav>
    <main class="main-content">
      <router-view />
    </main>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import {
  Scale,
  LayoutDashboard,
  Users,
  UserPlus,
  FileSpreadsheet,
  FileInput,
  CreditCard,
  FileDown,
  Menu
} from 'lucide-vue-next'

const sidebarOpen = ref(false)
</script>
