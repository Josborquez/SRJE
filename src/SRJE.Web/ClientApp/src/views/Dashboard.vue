<template>
  <div class="dashboard">
    <div class="page-header">
      <h1>Panel de Control</h1>
      <p>Sistema de Retenciones Judiciales de Empleados</p>
    </div>

    <div class="cards">
      <div class="card">
        <div class="card-icon blue">
          <Users :size="22" />
        </div>
        <h3>Beneficiarios</h3>
        <p>Consulta y administra el listado completo de beneficiarios registrados en el sistema.</p>
        <router-link to="/beneficiarios" class="card-link">
          Ver lista <ArrowRight :size="16" />
        </router-link>
      </div>

      <div class="card">
        <div class="card-icon green">
          <FileSpreadsheet :size="22" />
        </div>
        <h3>Importar Remuneraciones</h3>
        <p>Carga el archivo de texto con los datos de remuneraciones para procesar retenciones.</p>
        <router-link to="/archivos/remuneraciones" class="card-link">
          Importar archivo <ArrowRight :size="16" />
        </router-link>
      </div>

      <div class="card">
        <div class="card-icon orange">
          <FileDown :size="22" />
        </div>
        <h3>Generar TEMGE</h3>
        <p>Genera el archivo de pago en formato bancario para transferencias a beneficiarios.</p>
        <router-link to="/archivos/generar-temge" class="card-link">
          Generar archivo <ArrowRight :size="16" />
        </router-link>
      </div>

      <div class="card">
        <div class="card-icon purple">
          <CreditCard :size="22" />
        </div>
        <h3>Nuevas Cuentas</h3>
        <p>Importa un archivo Excel con datos de nuevas cuentas bancarias de beneficiarios.</p>
        <router-link to="/archivos/nuevas-cuentas" class="card-link">
          Importar Excel <ArrowRight :size="16" />
        </router-link>
      </div>
    </div>
  </div>
</template>

<script setup>
import { Users, FileSpreadsheet, FileDown, CreditCard, ArrowRight } from 'lucide-vue-next'
</script>
