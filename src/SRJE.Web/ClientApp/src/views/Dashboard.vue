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
.dashboard h1 { margin-bottom: 0.5rem; }
.cards { display: grid; grid-template-columns: repeat(auto-fill, minmax(240px, 1fr)); gap: 1rem; margin-top: 1.5rem; }
.card { background: #fff; border-radius: 8px; padding: 1.5rem; box-shadow: 0 1px 3px rgba(0,0,0,0.1); }
.card h3 { margin-bottom: 0.5rem; }
.card-link { color: #1976d2; text-decoration: none; font-weight: 500; }
.card-link:hover { text-decoration: underline; }
</style>
