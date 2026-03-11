<template>
  <div class="dashboard">
    <h1>Dashboard SRJE</h1>
    <p>Sistema de Retenciones Judiciales de Empleados</p>

    <div class="cards">
      <div class="card">
        <h3>Beneficiarios</h3>
        <router-link to="/beneficiarios" class="card-link">Ver lista</router-link>
      </div>
      <div class="card">
        <h3>Importar Remuneraciones</h3>
        <router-link to="/archivos/remuneraciones" class="card-link">Importar archivo</router-link>
      </div>
      <div class="card">
        <h3>Generar TEMGE</h3>
        <router-link to="/archivos/generar-temge" class="card-link">Generar archivo de pago</router-link>
      </div>
      <div class="card">
        <h3>Nuevas Cuentas</h3>
        <router-link to="/archivos/nuevas-cuentas" class="card-link">Importar Excel</router-link>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* Estilos globales aplicados desde assets/styles.css */
/* Solo overrides especificos de este componente */
.dashboard h1 { margin-bottom: 0.5rem; }
</style>
