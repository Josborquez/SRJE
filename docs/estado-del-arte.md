# SRJE — Estado del Arte

**Sistema de Retenciones Judiciales Electrónicas**
Fecha: 7 de abril de 2026

---

## 1. Resumen Ejecutivo

SRJE es un sistema web para la gestión de retenciones judiciales en nómina de remuneraciones, orientado al contexto chileno. Permite importar archivos de remuneraciones, gestionar beneficiarios y funcionarios, generar archivos TEMGE para pagos bancarios, y auditar la integridad de los datos.

**Stack tecnológico:** .NET 8 (backend) + Vue 3 SPA (frontend) + Oracle Database.

**Estado general:** El sistema se encuentra **operativo en ambiente de desarrollo**, con todas las funcionalidades core implementadas y funcionales. Pendiente la implementación de autenticación productiva (LDAP/AD/OAuth).

---

## 2. Arquitectura

```
┌─────────────────────┐     ┌─────────────────────┐     ┌──────────────┐
│   Vue 3 SPA         │────▶│   .NET 8 Web API    │────▶│  Oracle DB   │
│   (Vite + Pinia)    │ /api│   (Controllers +    │ EF  │  (10 tablas) │
│   Puerto 5173 (dev) │◀────│    Services)        │Core │              │
└─────────────────────┘     │   Puerto 5000       │◀────└──────────────┘
                            └─────────────────────┘
```

### Backend (.NET 8)

| Capa | Ubicación | Responsabilidad |
|------|-----------|-----------------|
| Controllers | `Controllers/` | 5 controladores, ~40 endpoints REST |
| Services | `Services/` | 7 servicios con interfaces (DI scoped) |
| Parsers | `Parsers/` | 3 parsers de archivos + 1 builder TEMGE |
| Entities | `Models/Entities/` | 10 entidades mapeadas a Oracle |
| DTOs | `Models/ViewModels/`, `Models/Requests/` | ~20 DTOs de entrada/salida |
| Helpers | `Helpers/` | RUT chileno, campos ancho fijo |
| Middleware | `Middleware/` | Manejo global de excepciones |
| Data | `Infrastructure/Data/` | EF Core DbContext con Oracle provider |

### Frontend (Vue 3)

| Capa | Ubicación | Contenido |
|------|-----------|-----------|
| Vistas | `views/` | 15 páginas (CRUD, importación, auditoría) |
| Componentes | `components/` | 5 componentes reutilizables |
| Stores | `stores/` | 3 stores Pinia (auth, beneficiarios, funcionarios) |
| Composables | `composables/` | 2 composables (RUT, formato) |
| API | `api/` | Cliente Axios con interceptores |
| Router | `router.js` | 16 rutas con guard de autenticación |

### Base de Datos (Oracle)

- **7 tablas de negocio:** BENEFICIARIOS, FUNCIONARIOS, RETENIDO_JUDICIAL, HISTORIAL_PAGOS_TEMGE, DETALLE_PAGO_TEMGE, BANCOS, TIPOS_RETENCION
- **3 tablas de auditoría:** LOG_CARGAS, LOG_CARGA_DETALLE, AUDITORIA_CAMBIOS
- **1 tabla catálogo:** TIPOS_CUENTA
- **14 scripts SQL** de creación/migración en `oracle/`

---

## 3. Módulos Funcionales

### 3.1 Autenticación

- Autenticación basada en cookies (HttpOnly, SameSite=Strict, 8h expiración)
- Interfaz pluggable `IAuthService`
- **Implementación actual:** `DevAuthService` con 3 usuarios hardcodeados:
  - `admin/admin123` — Administrador
  - `operador/operador123` — Operador
  - `consulta/consulta123` — Consulta
- **Pendiente:** Implementación productiva (LDAP/AD/OAuth)
- Frontend redirige a `/login` ante 401

### 3.2 Gestión de Beneficiarios

| Funcionalidad | Estado | Endpoint |
|---------------|--------|----------|
| Listar con paginación, búsqueda y filtros | ✅ Completo | `GET /api/beneficiarios` |
| Ver detalle con retenciones | ✅ Completo | `GET /api/beneficiarios/{rut}` |
| Crear beneficiario | ✅ Completo | `POST /api/beneficiarios` |
| Editar beneficiario | ✅ Completo | `PUT /api/beneficiarios/{rut}` |
| Inactivar (soft delete) | ✅ Completo | `DELETE /api/beneficiarios/{rut}` |
| Búsqueda rápida (autocompletado) | ✅ Completo | `GET /api/beneficiarios/buscar` |
| Exportar a Excel | ✅ Completo | `GET /api/beneficiarios/exportar/excel` |
| Exportar a CSV | ✅ Completo | `GET /api/beneficiarios/exportar/csv` |

### 3.3 Gestión de Funcionarios

| Funcionalidad | Estado | Endpoint |
|---------------|--------|----------|
| Listar con paginación y filtros | ✅ Completo | `GET /api/funcionarios` |
| Ver detalle con beneficiarios | ✅ Completo | `GET /api/funcionarios/{rut}` |
| Editar funcionario | ✅ Completo | `PUT /api/funcionarios/{rut}` |
| Inactivar (soft delete) | ✅ Completo | `DELETE /api/funcionarios/{rut}` |
| Estadísticas generales | ✅ Completo | `GET /api/funcionarios/stats` |

### 3.4 Importación de Remuneraciones

- **Formato:** Archivo texto ancho fijo, 126 caracteres/línea, codificación Latin-1
- **Flujo:** Preview (dry-run) → Revisión/edición en tabla → Confirmación
- **Capacidades:**
  - Detección automática de beneficiarios nuevos vs. existentes
  - Validación de RUT (dígito verificador)
  - Detección y manejo de multicuenta (mismo beneficiario, múltiples retenciones)
  - Edición inline de código de retención, tipo de pago, banco y cuenta
  - Log detallado línea por línea

### 3.5 Importación TEMGE (entrada)

- **Formato:** Archivo texto ancho fijo con 3 tipos de registro (cabecera 129, detalle 130, cierre 129)
- **Flujo:** Preview con validación de integridad → Confirmación
- **Validaciones:**
  - Integridad: monto total y cantidad de registros coinciden con cierre
  - Existencia de beneficiarios en el sistema
  - Formato de RUT

### 3.6 Generación TEMGE (salida)

- Genera archivo de pago TEMGE a partir de retenciones activas del período
- **Lógica de resolución de cuenta:**
  1. Cuenta propia de la retención (si existe)
  2. Cuenta del beneficiario
  3. Excluir si no hay cuenta
- Manejo especial Banco Estado (código 12) vs. otros bancos
- Registro en historial de pagos con detalle por línea

### 3.7 Importación de Nuevas Cuentas

- **Formato:** Excel XLSX con columnas definidas
- **Flujo:** Preview → Confirmación
- Valida RUT, longitud de cuenta, existencia previa
- Crea o actualiza beneficiarios con datos bancarios

### 3.8 Auditoría de Beneficiarios

- Compara archivo de remuneraciones contra base de datos
- Detecta 4 categorías:
  - Solo en archivo (no están en BD)
  - Solo en sistema (no están en archivo)
  - Con diferencias (nombre, RUT funcionario)
  - Coincidentes
- Exportable a Excel y CSV

### 3.9 Mantenedores

| Mantenedor | Operaciones |
|------------|-------------|
| Bancos | CRUD + toggle activo/inactivo |
| Tipos de Cuenta | CRUD + toggle activo/inactivo |
| Catálogos (solo lectura) | Bancos, Tipos de retención, Tipos de cuenta |

---

## 4. Patrones de Diseño Implementados

### Preview-then-Commit (Importaciones)
Todas las importaciones de archivos siguen un flujo de dos pasos: primero se parsea el archivo y se devuelve una previsualización con validaciones (sin persistir), luego el usuario selecciona las líneas a importar y confirma la operación.

### Auditoría Completa
- `AuditoriaCambios`: Registra cada cambio individual (campo, valor anterior, valor nuevo, usuario, IP)
- `LogCarga` + `LogCargaDetalle`: Registra cada importación con detalle línea por línea, incluyendo hash SHA-256 del archivo

### Soft Delete
Beneficiarios y funcionarios no se eliminan físicamente; se marcan como inactivos (`Estado = 'I'` / `Activo = 'N'`).

### Multicuenta
Un mismo par (beneficiario, funcionario) puede tener múltiples retenciones en un mismo período. El sistema detecta estos casos en la previsualización y permite al usuario asignar banco/cuenta por retención.

---

## 5. Cobertura de Tests

| Área | Archivo de Test | Tests |
|------|----------------|-------|
| Helpers | `RutHelperTests.cs` | 9 |
| Helpers | `FixedWidthHelperTests.cs` | 9 |
| Parsers | `RemuneracionesParserTests.cs` | 7 |
| Parsers | `TemgeParserTests.cs` | 6 |
| Parsers | `TemgeBuilderTests.cs` | 9 |
| Services | `BeneficiarioServiceTests.cs` | 15 |
| Services | `TemgeServiceTests.cs` | 10 |
| **Total** | | **~65 tests** |

**Tecnología:** xUnit + Moq + FluentAssertions + EF Core InMemory

**Áreas sin cobertura de tests:**
- Controllers (pruebas de integración HTTP)
- FuncionarioService
- RemuneracionesService (lógica de confirmación)
- NuevasCuentasService
- AuditoriaBeneficiariosService
- Frontend (sin tests unitarios ni E2E)

---

## 6. Dependencias Principales

### Backend
| Paquete | Versión | Uso |
|---------|---------|-----|
| .NET | 8.0 | Framework |
| Oracle.EntityFrameworkCore | 8.23.90 | Proveedor Oracle para EF Core |
| EPPlus | 7.5.2 | Lectura/escritura Excel |
| Serilog.AspNetCore | 8.0.3 | Logging estructurado |
| Newtonsoft.Json | 13.0.3 | Serialización JSON |

### Frontend
| Paquete | Versión | Uso |
|---------|---------|-----|
| Vue | ^3.5.13 | Framework SPA |
| Vue Router | ^4.5.0 | Enrutamiento |
| Pinia | ^2.3.0 | Estado global |
| Axios | ^1.7.9 | Cliente HTTP |
| Lucide Vue Next | ^0.577.0 | Iconografía |
| Vite | ^6.0.0 | Bundler/Dev server |

---

## 7. Pendientes y Oportunidades de Mejora

### Alta Prioridad
1. **Autenticación productiva** — Implementar LDAP/AD/OAuth reemplazando `DevAuthService`
2. **Autorización por roles** — El frontend no diferencia permisos por rol; todos los usuarios ven todo
3. **Validación de duplicados en retenciones** — No existe constraint UNIQUE que prevenga duplicados en `RETENIDO_JUDICIAL`

### Media Prioridad
4. **Tests de integración** — Controllers y flujos end-to-end sin cobertura
5. **Tests frontend** — Sin framework de testing configurado (Vitest/Cypress)
6. **Datos denormalizados** — `NOMBRE_FUNCIONARIO` en BENEFICIARIOS es redundante (ya existe la relación por RUT)
7. **Tipos de datos Oracle** — Campos numéricos sobredimensionados (RUT como NUMBER(18,0) en vez de NUMBER(10,0))

### Baja Prioridad
8. **Manejo de errores frontend** — Algunos flujos no muestran errores al usuario
9. **Internacionalización** — Todo hardcodeado en español (aceptable para el contexto)
10. **PWA/Offline** — No contemplado

---

## 8. Configuración de Ambientes

| Aspecto | Desarrollo | Producción |
|---------|------------|------------|
| Auth | DevAuthService (hardcoded) | Pendiente (LDAP/AD/OAuth) |
| BD Oracle | 192.168.69.92:1521 | Por configurar |
| CORS | localhost:5173 permitido | Deshabilitado |
| Frontend | Vite dev server (:5173) | Build estático en wwwroot/ |
| Logging | Debug level | Warning+ (Serilog a archivos) |

---

## 9. Estructura de Directorios

```
SRJE/
├── src/SRJE.Web/                    # Backend .NET 8
│   ├── Controllers/                 # 5 controladores API
│   ├── Services/                    # 7 servicios de negocio
│   ├── Parsers/                     # 3 parsers + 1 builder
│   ├── Models/                      # Entities, ViewModels, Requests
│   ├── Helpers/                     # RUT, FixedWidth
│   ├── Infrastructure/Data/         # EF Core DbContext
│   ├── Middleware/                   # Exception handler
│   ├── ClientApp/                   # Frontend Vue 3
│   │   └── src/
│   │       ├── views/               # 15 páginas
│   │       ├── components/          # 5 componentes
│   │       ├── stores/              # 3 stores Pinia
│   │       ├── composables/         # 2 composables
│   │       └── api/                 # Cliente Axios
│   └── Program.cs                   # Punto de entrada y DI
├── tests/SRJE.Tests/                # ~65 tests xUnit
├── oracle/                          # 14 scripts DDL/DML
├── docs/                            # Documentación técnica
└── legado/                          # Archivos de referencia
```
