# Analisis de Requerimientos — SRJE v3.0
## Sistema de Retenciones Judiciales de Empleados

**Documento fuente:** SRJE_Prompt_Desarrollo_v3.docx
**Archivos de ejemplo validados:** ENVIO REMUNERACIONES.txt, ENVIO TEMGE.txt, NUEVAS CUENTAS A BASE DATO.xlsx
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
| Excel | EPPlus | 7.x |
| Auth | JWT Bearer | 8.x |
| Logging | Serilog | 8.x |

### Observaciones sobre el Stack
- **Migracion de BD:** El SQL legacy (`Retenciones.sql`) esta en **SQL Server** (usa `[dbo]`, `nchar`, `numeric`), pero el documento especifica **Oracle 19c**. Esto implica una migracion de esquema SQL Server -> Oracle.
- **Migracion de codigo:** El codigo VB6 en `Retenciones.sql` genera el archivo TEMGE de forma procedural. Debe reescribirse completamente en C#.
- **DevExpress eliminado:** Se descarta del stack por ser innecesario. El frontend es Vue.js 3 puro con Composition API. No se requieren componentes server-side de DevExpress.

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
| ENVIO_REMUNERACIONES | Importacion | Texto ancho fijo (126 chars + CRLF) | Sincronizar retenciones desde sistema RRHH |
| TEMGE (entrada) | Importacion | Texto ancho fijo (cabecera 129, detalle 130, cierre 129 + CRLF) | Conciliar pagos bancarios |
| TEMGE (salida) | Generacion | Texto ancho fijo (cabecera 129, detalle 130, cierre 129 + CRLF) | Enviar pagos al banco |
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

### 5.1 ENVIO_REMUNERACIONES (126 chars + CRLF) -- VALIDADO con archivo real
| Pos | Long | Campo | Ejemplo Real |
|-----|------|-------|-------------|
| 1 | 9 | RUT_BENEFICIARIO (num, pad izq ceros) | `007051537` |
| 10 | 1 | DV_BENEFICIARIO | `7` |
| 11 | 9 | RUT_FUNCIONARIO (num, pad izq ceros) | `016830280` |
| 20 | 1 | DV_FUNCIONARIO | `0` |
| 21 | 20 | APELLIDO_PATERNO (pad der espacios) | `VALDERAS            ` |
| 41 | 20 | APELLIDO_MATERNO (pad der espacios) | `ABURTO              ` |
| 61 | 30 | NOMBRES (pad der espacios) | `EVA CRISTINA                  ` |
| 91 | 8 | ID_SISTEMA (num) | `00072099` |
| 99 | 11 | COD_RETENCION (pad der espacios) | `DURETENF   ` |
| 110 | 17 | TIPO_PAGO (pad der espacios) | `COMPENSACION     ` |
| **Total** | **126** | **CONFIRMADO: 126 chars exactos** | |

**VALIDADO:** El archivo real tiene exactamente **126 caracteres** por linea con terminacion CRLF. El documento mencionaba 127 pero es incorrecto. **NO contiene campo MONTO** — el monto viene por la relacion RUT beneficiario + RUT funcionario en la tabla RETENIDO_JUDICIAL.

**Estadisticas del archivo de ejemplo (1,741 registros):**
- Beneficiarios unicos: 1,477
- Tipos de pago: COMPENSACION (1,567), PERMANENTE SR (174)
- Codigos retencion: DURETENUTM (789), DURETENN (444), DURETENF (412), DURETENS10 (31), DURETENS3 (29), DURETENV (12), DURETENM (9), DURETENR (8), DURETENS9 (3), DURETENS2 (1), DURETENS4 (1), DURETENS11 (1), DURETENUF (1)
- Encoding: Latin-1 (ISO-8859-1) — contiene caracteres con tilde (ej: PEÑA)

### 5.2 TEMGE -- VALIDADO con archivo real

**Cabecera (Tipo 1) — 129 chars + CRLF:**
| Pos | Long | Campo | Valor Real |
|-----|------|-------|-----------|
| 1 | 1 | TIPO_REGISTRO | `1` |
| 2 | 20 | COD_EMPRESA | `06110104519640100572` |
| 22 | 12 | ESPACIOS | blancos |
| 34 | 8 | FECHA1 | `20260223` (yyyyMMdd) |
| 42 | 8 | FECHA2 | `20260223` (mismo valor) |
| 50 | 6 | HORA | `154247` (HHmmss) |
| 56 | 74 | ESPACIOS | blancos |
| **Total** | **129** | | |

**CORRECCION IMPORTANTE:** El COD_EMPRESA es de **20 caracteres** (`06110104519640100572`), NO 21. El documento indicaba `106110104519640100572` (21 chars) pero el `1` inicial es el TIPO_REGISTRO. La cabecera real mide **129 chars**, no 130.

**Detalle (Tipo 2) — 130 chars + CRLF:**
| Pos | Long | Campo | Ejemplo Real |
|-----|------|-------|-------------|
| 1 | 1 | TIPO_REGISTRO | `2` |
| 2 | 9 | RUT_BENEFICIARIO | `007008964` |
| 11 | 1 | DV_BENEFICIARIO | `5` |
| 12 | 39 | NOMBRE (pad der espacios) | `MORENO GOMEZ MYRIAM DE LAS MERCEDES    ` |
| 51 | 1 | SEPARADOR | `*` |
| 52 | 1 | IND_BANCO | ` ` (espacio=BcoEstado, `=`=Otro) |
| 53 | 15 | CTA_OT_BANCO | `000000000000000` (BcoEstado) |
| 68 | 11 | CEROS1 | `00000000000` |
| 79 | 2 | TIPO_CUENTA | `02` |
| 81 | 3 | COD_BANCO | `012` |
| 84 | 11 | CTA_ESTADO | `41762633599` (cuenta BcoEstado) |
| 95 | 11 | MONTO (pad izq ceros) | `00000272116` ($272.116) |
| 106 | 2 | DECIMALES | `00` |
| 108 | 10 | CEROS2 | `0000000000` |
| 118 | 11 | CEROS3 | `00000000000` |
| 129 | 2 | ESPACIOS finales | `  ` |
| **Total** | **130** | **CONFIRMADO** | |

**Cierre (Tipo 3) — 129 chars + CRLF:**
| Pos | Long | Campo | Ejemplo Real |
|-----|------|-------|-------------|
| 1 | 1 | TIPO_REGISTRO | `3` |
| 2 | 13 | MONTO_TOTAL (pad izq ceros) | `0000495592599` ($495.592.599) |
| 15 | 2 | DECIMALES | `00` |
| 17 | 5 | TOTAL_REG (pad izq ceros) | `01718` |
| 22 | 108 | ESPACIOS | blancos |
| **Total** | **129** | | |

**Estadisticas del archivo de ejemplo (1,718 registros detalle):**
- BancoEstado (012): 1,715 registros (99.8%)
- Otros bancos: 3 registros — cod 027 (Corpbanca/Itau), 016 (BCI), 504
- Monto total: $495.592.599
- Verificacion de integridad: Suma de montos = monto footer (OK)
- Cantidad registros = total footer (OK)

### 5.3 NUEVAS_CUENTAS_A_BASE_DATO (Excel .xlsx) -- VALIDADO con archivo real

| Columna | Campo | Ejemplo Real |
|---------|-------|-------------|
| A | Rut_Beneficiario | `18770765` |
| B | Dv_Beneficiario | `K` |
| C | Rut_Titular | `16215261` |
| D | Dv_Titular | `7` |
| E | Beneficiario (nombre) | `CORVALAN SEPULVEDA CINDY` |
| F | Cta_OtBanco | `0` (no aplica si BcoEstado) |
| G | Tipo_Cuenta | `2` (Ahorro/CuentaRUT) |
| H | Cod_Banco | `12` (BancoEstado) |
| I | Cta_Estado | `52166705967` (11 digitos) |
| J | OBSERVACIONES | (vacio) |

**Observaciones del archivo real:**
- 13 registros de datos + fila cabecera
- Todos los registros son BancoEstado (Cod_Banco=12)
- Cta_OtBanco siempre `0` (no aplica para BcoEstado)
- Tipo_Cuenta siempre `2` (Ahorro/CuentaRUT)
- Algunas cuentas tienen 10 digitos en vez de 11 (ej: `1367076816`) — requiere padding izq con ceros
- Columnas K-N existen pero estan vacias (4 columnas sobrantes)

---

## 6. Reglas de Negocio Criticas

### 6.1 Generacion TEMGE
1. **Exclusion de beneficiarios:** Sin cuenta, cuenta vacia/ceros, estado != 'A'
2. **BancoEstado (COD=012):** `IND_BANCO=' '`, `CTA_OT_BANCO='000000000000000'`, `CTA_ESTADO=numero_cuenta`
3. **Otro banco:** `IND_BANCO='='`, `CTA_OT_BANCO=numero_cuenta`, `CTA_ESTADO='00000000000'`
4. **Codigo empresa fijo:** `06110104519640100572` (20 chars, configurable en appsettings)
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

### 11.1 Riesgos Tecnicos (actualizado con validacion de archivos reales)
1. **Migracion SQL Server -> Oracle:** El esquema legacy usa tipos SQL Server (`nchar`, `numeric`, `float`). La migracion requiere mapeo cuidadoso de tipos.
2. **~~Campo MONTO en ENVIO_REMUNERACIONES~~** RESUELTO: El archivo real confirma **126 chars** (no 127). NO contiene campo MONTO. El monto viene de la relacion en BD.
3. **~~DevExpress~~** RESUELTO: Eliminado del stack. No es necesario con Vue.js 3.
4. **~~Codigo empresa TEMGE~~** RESUELTO: El valor correcto es `06110104519640100572` (**20 chars**). El '1' inicial en el documento era el TIPO_REGISTRO de la cabecera, no parte del codigo empresa.
5. **Encoding Latin-1:** El archivo ENVIO_REMUNERACIONES usa encoding Latin-1 (ISO-8859-1), no UTF-8. El parser debe manejar caracteres como Ñ (byte 0xD1).
6. **Cuentas BancoEstado con largo variable:** En el Excel NUEVAS_CUENTAS algunas cuentas tienen 10 digitos en vez de 11. Se debe aplicar padding izquierda con ceros.
7. **Cabecera/Cierre TEMGE 129 chars:** A diferencia del detalle (130 chars), la cabecera y cierre miden 129 chars. El parser debe tolerar esta diferencia.
8. **Banco codigo 504:** El archivo TEMGE real contiene un registro con COD_BANCO=504 que no esta en la tabla de bancos del documento. Requiere catalogo extensible.

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

### Correcciones al documento original (validadas con archivos reales):
- **ENVIO_REMUNERACIONES:** 126 chars (no 127). No contiene MONTO. Encoding Latin-1.
- **TEMGE cabecera/cierre:** 129 chars (no 130). COD_EMPRESA es 20 chars (no 21).
- **DevExpress:** Eliminado del stack — innecesario con Vue.js 3.

### Puntos pendientes RESUELTOS (validados con Retenciones.sql y archivos reales):

**1. Banco codigo 504 — ES VALIDO**
El codigo VB6 legacy en `Retenciones.sql` solo distingue `Cod_Banco = 12` (BancoEstado) vs "todo lo demas". No valida codigos de banco contra un catalogo. El registro con COD_BANCO=504 corresponde a:
- RUT: 21202875-4, Beneficiario: PAZMINO COELLO KARINA
- Cuenta: 000210200353565 (otro banco), Tipo: 02 (Ahorro)
- Monto: $877.842
El codigo 504 podria corresponder a **Banco Consorcio** o un codigo SBIF/CMF vigente. Se recomienda mantener el catalogo de bancos extensible (tabla BD, no hardcoded) e incluir banco 504.

**2. Cuentas BancoEstado con menos de 11 digitos — APLICAR PADDING**
El VB6 legacy usa `Format(Cta_estado, "00000000000")` que aplica padding izquierda con ceros hasta 11 digitos. Por lo tanto, una cuenta `1367076816` (10 digitos) se convierte en `01367076816` (11 digitos). El campo en SQL Server es `[Cta_Estado] [nchar](11)` (largo fijo). **Regla: aplicar LPAD con ceros a 11 digitos** al importar desde Excel, no rechazar.

**3. Cabecera TEMGE — Confirmado 129 chars**
El VB6 genera: `"1" + "06110104519640100572" + Space(11) + fecha + fecha + hora + Space(75)` = exactamente **129 chars**. Confirmado que la cabecera siempre fue 129, no 130. El cierre en VB6 genera 130 chars (`Space(109)`) pero el archivo real muestra 129 — posible trim al escribir. Para el nuevo sistema se recomienda generar cierre de 130 chars (respetando el `Space(109)` del VB6 original).

### Servidor Oracle 19c:
- Confirmado disponible para desarrollo. Scripts DDL generados en `oracle/` para ejecucion manual.
