# Analisis de Requerimientos — SRJE v3.0
## Sistema de Retenciones Judiciales de Empleados

**Documento fuente:** SRJE_Prompt_Desarrollo_v3.docx
**Fecha de analisis:** 2026-03-10
**Version del prompt:** 3.0

---

## 1. Resumen Ejecutivo

El SRJE es una aplicacion web full-stack para gestionar beneficiarios de retenciones judiciales sobre sueldos de funcionarios publicos. El sistema reemplaza un proceso legacy en **VB6/VBA** (evidenciado por el archivo `Retenciones.sql`) que genera archivos bancarios TEMGE de forma semi-manual.

### Objetivo Principal
Modernizar el proceso de gestion de retenciones judiciales, desde el registro de beneficiarios hasta la generacion de archivos de pago bancario y su conciliacion.

---

## 2. Stack Tecnologico Definido

| Capa | Tecnologia | Version |
|------|-----------|---------|
| Backend | .NET Core MVC | 8.0.16 |
| ORM | Entity Framework Core | 8.0.19 |
| ORM Oracle | Oracle.EntityFrameworkCore | 8.23.90 |
| Base de Datos | Oracle Database | 19c |
| Frontend | Vue.js 3 (Composition API) | 3.x |
| Serialization | Newtonsoft.Json | 13.0.3 |
| UI Components | DevExpress.Data | 22.1.14 |
| Excel | EPPlus | 7.x |
| Auth | JWT Bearer | 8.x |
| Logging | Serilog | 8.x |

### Observaciones sobre el Stack
- **Migracion de BD:** El SQL legacy (`Retenciones.sql`) esta en **SQL Server** (usa `[dbo]`, `nchar`, `numeric`), pero el documento especifica **Oracle 19c**. Esto implica una migracion de esquema SQL Server -> Oracle.
- **Migracion de codigo:** El codigo VB6 en `Retenciones.sql` genera el archivo TEMGE de forma procedural. Debe reescribirse completamente en C#.
- **DevExpress:** Se menciona pero no queda claro si se usara en el frontend Vue o solo en componentes .NET server-side. Posible conflicto con Vue.js puro.

---

## 3. Modelo de Datos

### 3.1 Tablas Principales (7 tablas core)

| Tabla | Proposito | Columnas Clave |
|-------|----------|----------------|
| **BENEFICIARIOS** | Datos personales y bancarios del beneficiario | RUT, nombre, cuenta, banco, tipo cuenta |
| **RETENIDO_JUDICIAL** | Retenciones vigentes (funcionario -> beneficiario) | RUT titular, RUT beneficiario, monto, periodo |
| **FUNCIONARIOS** | Funcionarios publicos afectados | RUT, nombres, apellidos, ID sistema |
| **HISTORIAL_PAGOS_TEMGE** | Registro maestro de archivos TEMGE generados | Fecha, monto total, cantidad registros, estado |
| **DETALLE_PAGO_TEMGE** | Lineas individuales del archivo TEMGE | RUT beneficiario, monto pagado, estado linea |
| **API_CLIENTES** | Clientes autorizados para API publica | API Key (SHA-256), permisos, IPs, expiracion |
| **LOG_API_ACCESOS** | Auditoria de accesos API externa | Cliente, endpoint, IP, codigo respuesta, tiempo |

### 3.2 Tablas de Auditoria y Logs (4 tablas)

| Tabla | Proposito |
|-------|----------|
| **LOG_CARGAS** | Registro maestro de cada proceso de importacion/generacion |
| **LOG_CARGA_DETALLE** | Detalle linea por linea de cada carga |
| **AUDITORIA_CAMBIOS** | Cambios manuales en fichas (INSERT/UPDATE/DELETE) |
| **AUDITORIA_SRJE** | Log general de acciones por usuario (mencionada en seccion 10.2) |

### 3.3 Tablas de Catalogo (implicitas)

| Tabla | Contenido |
|-------|----------|
| Bancos | 11 bancos definidos (012=BancoEstado, 001=Chile, 009=Internacional, etc.) |
| Tipos de Cuenta | 3 tipos (01=Cte, 02=Ahorro, 03=Vista) |
| Tipos de Retencion | 13 codigos (DURETENF, DURETENM, DURETENUF, etc.) |

### 3.4 Diferencias Legacy vs Nuevo Modelo

| Aspecto | Legacy (SQL Server) | Nuevo (Oracle 19c) |
|---------|--------------------|--------------------|
| Tabla Beneficiarios | 10 columnas basicas | 19 columnas (incluye auditoria, personal) |
| Tabla RetenidoJudicial | 6 columnas (float para RUT) | 12 columnas (NUMBER, estados, periodo) |
| Funcionarios | No existe | Nueva tabla completa |
| Historial pagos | No existe | 2 tablas (maestro + detalle) |
| Auditoria | No existe | 4 tablas de auditoria |

**Total: ~11 tablas** a crear en Oracle 19c.

---

## 4. Modulos Funcionales

### 4.1 Modulo Beneficiarios (CRUD)
- **Ficha del Beneficiario:** Formulario con 3 secciones (datos personales, cuenta bancaria, funcionario asociado)
- **Lista de Beneficiarios:** Grilla con busqueda en tiempo real (debounce 300ms), paginacion server-side, filtros, exportacion CSV
- **Detalle de Beneficiario:** Vista completa con retenciones activas
- **Endpoints:** 7 endpoints REST definidos (`/api/beneficiarios/*`)

### 4.2 Modulo Archivos (Import/Export)
Cuatro flujos de archivos distintos:

| Archivo | Direccion | Formato | Proposito |
|---------|-----------|---------|----------|
| ENVIO_REMUNERACIONES | Importacion | Texto ancho fijo (127 chars) | Sincronizar retenciones desde sistema RRHH |
| TEMGE (entrada) | Importacion | Texto ancho fijo (130 chars) | Conciliar pagos bancarios |
| TEMGE (salida) | Generacion | Texto ancho fijo (130 chars) | Enviar pagos al banco |
| NUEVAS_CUENTAS | Importacion | Excel .xlsx | Registrar cuentas bancarias nuevas |

### 4.3 Modulo Previsualizacion (Seccion 13 - Critico)
Sistema de previsualizacion pre-importacion con:
- **3 endpoints de preview** (parse sin persistir): `/api/preview/remuneraciones`, `/api/preview/temge`, `/api/preview/nuevas-cuentas`
- **3 endpoints de confirmacion**: `/api/archivos/*/confirmar`
- **Grilla editable** con estados visuales (OK=verde, ADVERTENCIA=amarillo, ERROR=rojo, EXCLUIDO=gris, NUEVO=azul)
- **Edicion inline** de campos seleccionados (monto, cod retencion, tipo pago, cuenta, banco)
- **Checkbox incluir/excluir** por registro

### 4.4 Modulo API Publica (Seccion 14)
API REST de solo lectura para consultas inter-oficinas:
- **Autenticacion:** API Key (SHA-256) con middleware dedicado
- **4 endpoints:** beneficiarios/{rut}, retenciones, pagos, periodos
- **Seguridad:** Enmascaramiento de cuenta (****XXXX), whitelist de IPs, expiracion de claves
- **Auditoria:** Log de todos los accesos

### 4.5 Modulo Auditoria y Cargas (Seccion 15)
- **Historial de Cargas:** Vista con filtros, timeline visual, KPIs
- **Deteccion de duplicados:** SHA-256 del archivo
- **Reversion de cargas:** Con autorizacion de ADMINISTRADOR
- **Dashboard de monitoreo:** 7 KPIs en tiempo real

---

## 5. Formatos de Archivos de Ancho Fijo

### 5.1 ENVIO_REMUNERACIONES (127 chars + CRLF)
| Pos | Long | Campo |
|-----|------|-------|
| 1 | 9 | RUT_BENEFICIARIO (num, pad izq ceros) |
| 10 | 1 | DV_BENEFICIARIO |
| 11 | 9 | RUT_FUNCIONARIO (num, pad izq ceros) |
| 20 | 1 | DV_FUNCIONARIO |
| 21 | 20 | APELLIDO_PATERNO (pad der espacios) |
| 41 | 20 | APELLIDO_MATERNO (pad der espacios) |
| 61 | 30 | NOMBRES (pad der espacios) |
| 91 | 8 | ID_SISTEMA (num) |
| 99 | 11 | COD_RETENCION |
| 110 | 17 | TIPO_PAGO |
| **Total** | **126** | **Falta 1 char para llegar a 127 (posible campo monto no documentado)** |

**Hallazgo:** La suma de campos da 126 caracteres, no 127. Posiblemente falta documentar el campo MONTO al final de la linea. Esto debe verificarse con un archivo de ejemplo real.

### 5.2 TEMGE (130 chars + CRLF)
**Cabecera (Tipo 1):**
| Pos | Long | Campo | Valor |
|-----|------|-------|-------|
| 1 | 1 | TIPO_REGISTRO | '1' |
| 2 | 21 | COD_EMPRESA | 106110104519640100572 (fijo) |
| 23 | 11 | ESPACIOS | blancos |
| 34 | 8 | FECHA1 | yyyyMMdd |
| 42 | 8 | FECHA2 | yyyyMMdd (duplicado) |
| 50 | 6 | HORA | HHmmss |
| 56 | 75 | ESPACIOS | blancos (total=130) |

**Detalle (Tipo 2):**
| Pos | Long | Campo |
|-----|------|-------|
| 1 | 1 | TIPO_REGISTRO='2' |
| 2 | 9 | RUT_BENEFICIARIO |
| 11 | 1 | DV_BENEFICIARIO |
| 12 | 39 | NOMBRE (pad der espacios) |
| 51 | 1 | SEPARADOR='*' |
| 52 | 1 | IND_BANCO (' '=BcoEstado, '='=Otro) |
| 53 | 15 | CTA_OT_BANCO |
| 68 | 11 | CEROS1 |
| 79 | 2 | TIPO_CUENTA |
| 81 | 3 | COD_BANCO |
| 84 | 11 | CTA_ESTADO |
| 95 | 11 | MONTO (pesos enteros, pad izq ceros) |
| 106 | 2 | DECIMALES='00' |
| 108 | 10 | CEROS2 |
| 118 | 11 | CEROS3 |
| 129 | 2 | ESPACIOS finales |
| **Total** | **130** | **OK - Coincide** |

**Cierre (Tipo 3):**
| Pos | Long | Campo |
|-----|------|-------|
| 1 | 1 | TIPO_REGISTRO='3' |
| 2 | 13 | MONTO_TOTAL (pad izq ceros) |
| 15 | 2 | DECIMALES='00' |
| 17 | 5 | TOTAL_REG (pad izq ceros) |
| 22 | 109 | ESPACIOS (total=130) |

---

## 6. Reglas de Negocio Criticas

### 6.1 Generacion TEMGE
1. **Exclusion de beneficiarios:** Sin cuenta, cuenta vacia/ceros, estado != 'A'
2. **BancoEstado (COD=012):** `IND_BANCO=' '`, `CTA_OT_BANCO='000000000000000'`, `CTA_ESTADO=numero_cuenta`
3. **Otro banco:** `IND_BANCO='='`, `CTA_OT_BANCO=numero_cuenta`, `CTA_ESTADO='00000000000'`
4. **Codigo empresa fijo:** `06110104519640100572` (configurable en appsettings)
5. **Monto:** Pesos enteros sin decimales, decimales siempre '00'
6. **Nombre:** Exactamente 39 caracteres (pad derecha con espacios)

### 6.2 Validacion RUT Chileno
- Algoritmo modulo 11 con factores {2,3,4,5,6,7}
- DV: 11='0', 10='K', otro=digito
- Obligatorio para beneficiarios y funcionarios
- Rechazo si no pasa validacion

### 6.3 Integridad de Datos
- RUT_BENEFICIARIO unico en tabla BENEFICIARIOS
- Cuenta BancoEstado: exactamente 11 digitos numericos
- Cuenta otro banco: maximo 15 caracteres
- Nombre beneficiario: maximo 39 caracteres
- Relacion N:M entre funcionarios y beneficiarios

### 6.4 Control de Periodos
- Formato AAAAMM (ej: 202602)
- No permitir importacion duplicada del mismo periodo
- Retenciones del periodo anterior quedan en historial
- Estado automatico 'A' al importar

### 6.5 Conciliacion
- Comparar RUTs y montos con ultimo archivo generado
- Marcar como 'C' (Conciliado) los que coincidan
- Generar reporte de diferencias (rechazos, diferencias de monto)

---

## 7. Arquitectura y Patrones

### 7.1 Arquitectura General
```
Vue.js 3 (SPA) <--HTTP/JSON--> .NET Core MVC (API REST) <--EF Core--> Oracle 19c
```

### 7.2 Capas Backend
```
Controllers (thin, solo orquestacion HTTP)
    |
Services / Interfaces (logica de negocio)
    |
Repositories / Interfaces (acceso a datos)
    |
EF Core DbContext -> Oracle 19c
```

### 7.3 Patrones Requeridos (Seccion 16)
- **SOLID:** Los 5 principios aplicados explicitamente
- **Clean Code:** Nombres significativos, funciones < 20 lineas, sin magic numbers
- **KISS:** Soluciones simples (Substring para ancho fijo, no frameworks de parsing)
- **DRY:** Helpers compartidos (FixedWidthHelper, RutHelper), componentes Vue reutilizables

### 7.4 Interfaces Segregadas (ISP)
- `IBeneficiarioReader` (consultas)
- `IBeneficiarioWriter` (escritura)
- `ITemgeGenerator` (generacion archivo)
- `IArchivoImporter<T>` (importacion generica)
- `IArchivoParser<T>` (parsing extensible por tipo)
- `IRegistroTemgeBuilder` (estrategia por banco)

---

## 8. Seguridad

### 8.1 Autenticacion y Autorizacion
- **Interna:** ASP.NET Core Identity o Windows Authentication
- **Roles:** ADMINISTRADOR (total), OPERADOR (CRUD + archivos), CONSULTA (solo lectura)
- **API Externa:** API Key (SHA-256 hash), expiración, whitelist IPs

### 8.2 Auditoria
- Campos de auditoria en todas las entidades (usuario, fecha creacion/modificacion)
- Tabla AUDITORIA_CAMBIOS con trigger Oracle o interceptor EF Core
- Tabla LOG_API_ACCESOS para accesos externos
- Registro de IP, usuario, accion, valores anteriores/nuevos

### 8.3 Proteccion de Datos
- Enmascaramiento de cuentas bancarias en API publica (****XXXX)
- Permisos granulares por cliente API (JSON array de recursos)

---

## 9. Frontend Vue.js - Componentes

### 9.1 Vistas Principales (8 vistas)
| Componente | Proposito |
|-----------|----------|
| FichaBeneficiario.vue | Formulario alta/edicion (3 secciones) |
| ListaBeneficiarios.vue | Grilla con busqueda, paginacion, filtros |
| DetalleBeneficiario.vue | Vista completa del beneficiario |
| ImportarRemuneraciones.vue | Carga archivo ENVIO_REMUNERACIONES |
| ImportarTemge.vue | Carga archivo TEMGE bancario |
| ImportarNuevasCuentas.vue | Carga Excel nuevas cuentas |
| GenerarTemge.vue | Generacion archivo pago TEMGE |
| Dashboard.vue | Panel principal |

### 9.2 Componentes Reutilizables
| Componente | Proposito |
|-----------|----------|
| RutInput.vue | Validacion RUT chileno (modulo 11) |
| MontoInput.vue | Formato moneda CLP |
| FileUpload.vue | Drop zone archivos |
| PreviewImportacion.vue | Grilla editable de previsualizacion |

### 9.3 Vistas de Auditoria (3 vistas adicionales)
| Componente | Proposito |
|-----------|----------|
| HistorialCargas.vue | Timeline de cargas con filtros |
| DetalleCarga.vue | Estadisticas y tabla de lineas por carga |
| DashboardCargas.vue | KPIs de monitoreo en tiempo real |

### 9.4 Dependencias Frontend
- Vue 3 + Composition API
- Pinia (state management)
- Vue Router
- Axios (HTTP client)
- Vite (build tool)

---

## 10. Plan de Desarrollo Sugerido (del documento)

| Paso | Descripcion | Dependencias |
|------|-----------|-------------|
| 1 | Crear proyecto .NET Core MVC + NuGet | Ninguna |
| 2 | Definir entidades EF Core + DbContext | Paso 1 |
| 3 | Migracion/Script DDL Oracle | Paso 2 |
| 4 | Implementar Repositories + Services (DI) | Paso 3 |
| 5 | Implementar Controllers API REST | Paso 4 |
| 6 | Crear proyecto Vue.js (Vite, Pinia, Router, Axios) | Ninguna |
| 7 | Componentes Vue: RutInput -> Ficha -> Lista -> Archivos | Paso 6 |
| 8 | ArchivoTemgeService (generacion + parsing) | Paso 4 |
| 9 | Pruebas unitarias (RutHelper, parsers, TEMGE) | Pasos 4-8 |

---

## 11. Riesgos y Puntos de Atencion

### 11.1 Riesgos Tecnicos
1. **Migracion SQL Server -> Oracle:** El esquema legacy usa tipos SQL Server (`nchar`, `numeric`, `float`). La migracion requiere mapeo cuidadoso de tipos.
2. **Campo MONTO en ENVIO_REMUNERACIONES:** El layout suma 126 chars pero el documento dice 127. Falta documentar donde va el monto o hay un campo no especificado.
3. **DevExpress en Vue:** El documento lista DevExpress.Data como dependencia NuGet pero el frontend es Vue.js puro. Verificar si realmente se necesita o es herencia del sistema legacy.
4. **Codigo empresa TEMGE:** El documento muestra dos valores distintos: `06110104519640100572` (20 chars) en texto y `106110104519640100572` (21 chars con '1' prefix) en las tablas. El VB6 legacy usa la version con '1'. Aclarar cual es el correcto.

### 11.2 Complejidad del Sistema
- **~11 tablas** en Oracle 19c
- **~20+ endpoints** REST (7 beneficiarios + 6 archivos + 3 preview + 3 confirmacion + 4 API publica + 8 cargas)
- **~14 componentes Vue** (8 vistas + 3 auditoria + 3 reutilizables + Dashboard)
- **4 tipos de archivos** con parsing de ancho fijo
- **3 flujos de previsualizacion** con edicion inline

### 11.3 Dependencias Externas
- Conexion a Oracle 19c (requiere servidor configurado)
- Sistema de remuneraciones (genera ENVIO_REMUNERACIONES)
- Sistema bancario (consume/genera archivos TEMGE)
- Sistemas de otras oficinas (consumen API publica)

---

## 12. Resumen de Endpoints API

### API Interna (requiere autenticacion por rol)
| Grupo | Cantidad | Ruta Base |
|-------|----------|-----------|
| Beneficiarios | 7 | /api/beneficiarios |
| Archivos | 6 | /api/archivos |
| Preview | 3 | /api/preview |
| Confirmacion | 3 | /api/archivos/*/confirmar |
| Cargas/Auditoria | 8 | /api/cargas |
| **Total Interna** | **27** | |

### API Publica (requiere API Key)
| Grupo | Cantidad | Ruta Base |
|-------|----------|-----------|
| Consultas externas | 4 | /api/publica |
| **Total Publica** | **4** | |

**Gran total: ~31 endpoints REST**

---

## 13. Conclusiones

El documento SRJE_Prompt_Desarrollo_v3.docx es un prompt de desarrollo exhaustivo y bien estructurado que cubre:

1. **Arquitectura completa** backend (.NET 8) y frontend (Vue 3)
2. **Modelo de datos detallado** con 11 tablas Oracle
3. **Formatos de archivos bancarios** especificados campo por campo
4. **Reglas de negocio claras** para generacion TEMGE y conciliacion
5. **Sistema de previsualizacion** robusto con edicion inline
6. **API publica** con seguridad por API Key
7. **Auditoria integral** (cargas, cambios, accesos API)
8. **Principios SOLID + Clean Code** documentados con ejemplos concretos

Los puntos que requieren aclaracion antes de iniciar el desarrollo son:
- Campo faltante en layout ENVIO_REMUNERACIONES (127 vs 126 chars)
- Codigo empresa TEMGE (20 vs 21 caracteres)
- Rol de DevExpress en la arquitectura Vue.js
- Disponibilidad de archivos de ejemplo para validar parsers
