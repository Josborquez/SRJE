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

<style>
* { margin: 0; padding: 0; box-sizing: border-box; }
body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; }

.app-layout {
  display: flex;
  min-height: 100vh;
}

.sidebar {
  width: 240px;
  background: #1a1a2e;
  color: #eee;
  padding: 1rem 0;
  flex-shrink: 0;
}

.sidebar-header {
  padding: 0 1rem 1rem;
  border-bottom: 1px solid #333;
}

.sidebar-header h2 { color: #4fc3f7; }
.sidebar-header small { color: #999; }

.nav-menu {
  list-style: none;
  padding: 0.5rem 0;
}

.nav-menu li { padding: 0; }

.nav-section {
  padding: 0.8rem 1rem 0.3rem;
  font-size: 0.75rem;
  text-transform: uppercase;
  color: #888;
  letter-spacing: 0.05em;
}

.nav-menu a {
  display: block;
  padding: 0.5rem 1rem;
  color: #ccc;
  text-decoration: none;
  font-size: 0.9rem;
  transition: background 0.2s;
}

.nav-menu a:hover { background: #16213e; color: #fff; }
.nav-menu a.router-link-active { background: #0f3460; color: #4fc3f7; }

.main-content {
  flex: 1;
  padding: 1.5rem;
  background: #f5f5f5;
  overflow-y: auto;
}
</style>
