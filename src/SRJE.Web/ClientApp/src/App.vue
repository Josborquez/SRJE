<template>
  <div class="app-layout">
    <nav class="sidebar">
      <div class="sidebar-header">
        <h2>SRJE</h2>
        <small>Retenciones Judiciales</small>
      </div>
      <ul class="nav-menu">
        <li><router-link to="/">Dashboard</router-link></li>
        <li class="nav-section">Beneficiarios</li>
        <li><router-link to="/beneficiarios">Lista</router-link></li>
        <li><router-link to="/beneficiarios/nuevo">Nuevo</router-link></li>
        <li class="nav-section">Archivos</li>
        <li><router-link to="/archivos/remuneraciones">Importar Remuneraciones</router-link></li>
        <li><router-link to="/archivos/temge">Importar TEMGE</router-link></li>
        <li><router-link to="/archivos/nuevas-cuentas">Nuevas Cuentas</router-link></li>
        <li><router-link to="/archivos/generar-temge">Generar TEMGE</router-link></li>
      </ul>
    </nav>
    <main class="main-content">
      <router-view />
    </main>
  </div>
</template>

<!-- Estilos globales importados en main.js desde assets/styles.css -->
