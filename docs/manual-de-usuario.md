# SRJE — Manual de Usuario

**Sistema de Retenciones Judiciales Electrónicas**
Versión 1.0 — Abril 2026

---

## 1. Introducción

SRJE es un sistema web para gestionar las retenciones judiciales aplicadas sobre las remuneraciones de funcionarios. Permite:

- Administrar beneficiarios y funcionarios
- Importar archivos de remuneraciones y cuentas bancarias
- Generar archivos TEMGE para pago bancario
- Auditar la consistencia entre archivos y base de datos
- Mantener catálogos de bancos y tipos de cuenta

---

## 2. Acceso al Sistema

### 2.1 Inicio de Sesión

1. Abra el navegador y acceda a la URL del sistema
2. Ingrese su **usuario** y **contraseña**
3. Haga clic en **Iniciar Sesión**

La sesión dura 8 horas. Si expira, el sistema lo redirigirá automáticamente a la pantalla de login.

### 2.2 Cerrar Sesión

Haga clic en el botón **Cerrar sesión** ubicado en la esquina inferior izquierda de la barra lateral.

---

## 3. Navegación

Al ingresar al sistema, verá una **barra lateral izquierda** con las siguientes secciones:

| Sección | Ícono | Descripción |
|---------|-------|-------------|
| Inicio | 🏠 | Panel principal con accesos directos |
| Beneficiarios | 👤 | Gestión de beneficiarios de retenciones |
| Funcionarios | 👥 | Gestión de funcionarios con retenciones |
| Importar Remuneraciones | 📄 | Carga de archivo de remuneraciones |
| Importar TEMGE | 📥 | Carga de archivo TEMGE de entrada |
| Nuevas Cuentas | 🏦 | Carga de cuentas bancarias desde Excel |
| Generar TEMGE | 📤 | Generación de archivo de pago |
| Auditoría | 🔍 | Comparación archivo vs. base de datos |
| Bancos | ⚙️ | Mantenedor de bancos |
| Tipos de Cuenta | ⚙️ | Mantenedor de tipos de cuenta |

En dispositivos móviles, la barra lateral se oculta y aparece un botón de menú (☰) en la esquina superior izquierda.

---

## 4. Beneficiarios

### 4.1 Listar Beneficiarios

**Ruta:** Barra lateral → **Beneficiarios**

La pantalla muestra:
- **Contadores:** Total inscritos, activos e inactivos
- **Buscador:** Filtra por nombre o RUT (se aplica automáticamente al escribir)
- **Tabla:** RUT, Nombre, RUT Funcionario, Nombre Funcionario, Banco, Cuenta, N° Retenciones, Monto Mensual, Estado

**Acciones disponibles:**
- **Buscar:** Escriba en el campo de búsqueda para filtrar
- **Filtrar por estado:** Use el selector Todos/Activos/Inactivos
- **Exportar:** Botón "Exportar" → Excel o CSV (descarga todas las filas, no solo la página actual)
- **Nuevo:** Botón "Nuevo Beneficiario" para crear uno manualmente
- **Paginación:** Seleccione 10, 20, 50 o 100 registros por página

### 4.2 Ver Detalle de Beneficiario

**Acción:** En la lista, haga clic en el ícono **Ver** (ojo) de un beneficiario.

Se muestran tres secciones:
1. **Datos Personales:** RUT, nombre, fecha nacimiento, sexo, estado civil, domicilio, comuna, teléfono
2. **Datos del Funcionario:** RUT y nombre del funcionario asociado
3. **Cuenta Bancaria:** Banco, tipo de cuenta, número de cuenta, sucursal

Además, una tabla de **Retenciones Activas** con: funcionario, monto, código de retención, tipo de pago y período.

### 4.3 Crear Beneficiario

**Ruta:** Beneficiarios → **Nuevo Beneficiario**

Complete el formulario:

1. **Datos Personales**
   - RUT del beneficiario (se valida automáticamente el dígito verificador)
   - Nombre (máximo 39 caracteres)
   - Fecha de nacimiento, sexo, estado civil (opcionales)
   - Domicilio, comuna, teléfono (opcionales)

2. **Datos del Funcionario**
   - RUT del funcionario titular
   - Nombre del funcionario

3. **Cuenta Bancaria**
   - Seleccione el banco
   - Seleccione el tipo de cuenta (Corriente, Ahorro, Vista)
   - Ingrese el número de cuenta:
     - Si es **Banco Estado** (código 12): campo "Cuenta Estado" (máx. 15 dígitos)
     - Si es **otro banco**: campo "Cuenta Otro Banco" (máx. 15 dígitos)
   - Sucursal (opcional)

Haga clic en **Guardar** para crear el beneficiario.

### 4.4 Editar Beneficiario

**Acción:** En la lista o detalle, haga clic en **Editar** (ícono lápiz).

El formulario se carga con los datos actuales. Modifique los campos necesarios y haga clic en **Guardar**. Todos los cambios quedan registrados en la auditoría.

### 4.5 Inactivar Beneficiario

**Acción:** En la lista o detalle, haga clic en **Inactivar** (ícono papelera).

Aparecerá un diálogo de confirmación. Al confirmar, el beneficiario pasa a estado **Inactivo**. No se elimina de la base de datos.

---

## 5. Funcionarios

### 5.1 Listar Funcionarios

**Ruta:** Barra lateral → **Funcionarios**

La pantalla muestra:
- **Estadísticas:** Total de funcionarios, activos, monto mensual total, período actual
- **Buscador:** Filtra por nombre o RUT
- **Filtro de estado:** Todos / Activos / Inactivos
- **Tabla:** RUT, Apellido Paterno, Apellido Materno, Nombres, N° Beneficiarios, Monto Total, Estado

### 5.2 Ver Detalle de Funcionario

**Acción:** Haga clic en **Ver** en la lista.

Se muestra:
- **Información del funcionario:** RUT, nombres, ID sistema, estado
- **Estadísticas:** Total de beneficiarios y monto total de retenciones
- **Beneficiarios asociados:** Tarjetas individuales con datos bancarios y tabla de retenciones por cada beneficiario

### 5.3 Editar Funcionario

**Acción:** Haga clic en **Editar** en la lista o detalle.

Campos editables: Apellido Paterno, Apellido Materno, Nombres, ID Sistema.

> **Nota:** El RUT del funcionario no es editable.

---

## 6. Importación de Archivos

Todas las importaciones siguen el mismo flujo de dos pasos:

```
Subir archivo → Previsualización → Seleccionar líneas → Confirmar
```

### 6.1 Importar Remuneraciones

**Ruta:** Barra lateral → **Importar Remuneraciones**

**Paso 1 — Subir archivo:**
1. Arrastre un archivo `.txt` de remuneraciones al área de carga, o haga clic para seleccionarlo
2. Ingrese el **período** en formato AAAAMM (ejemplo: `202603` para marzo 2026)
3. Haga clic en **Previsualizar**

**Paso 2 — Revisar previsualización:**

La tabla muestra cada línea del archivo con un estado:

| Estado | Color | Significado |
|--------|-------|-------------|
| OK | Verde | Beneficiario existente, datos válidos |
| NUEVO | Azul | Beneficiario no existe en el sistema, se creará |
| ADVERTENCIA | Amarillo | Datos válidos con observaciones (ej. RUT inválido en archivo original) |
| ERROR | Rojo | Datos inválidos, no se puede importar |
| MULTICUENTA | Morado | Mismo beneficiario con múltiples retenciones |

**Acciones en la previsualización:**
- **Filtrar por estado:** Haga clic en los contadores superiores (OK, NUEVO, etc.) para filtrar
- **Seleccionar/deseleccionar líneas:** Use las casillas de verificación para incluir o excluir líneas
- **Editar campos:** En líneas multicuenta, puede editar banco, tipo de cuenta y número de cuenta
- **Editar código de retención y tipo de pago:** Seleccione de las listas desplegables

**Paso 3 — Confirmar:**
1. Revise que las líneas seleccionadas son correctas
2. Haga clic en **Confirmar Importación**
3. El sistema muestra un resumen: insertados, actualizados, excluidos y errores

### 6.2 Importar TEMGE

**Ruta:** Barra lateral → **Importar TEMGE**

1. Suba un archivo `.txt` TEMGE
2. El sistema valida la integridad del archivo (monto total y cantidad de registros deben coincidir con el registro de cierre)
3. Se muestra un indicador de integridad: ✓ OK o ✗ Error
4. Revise la previsualización y confirme

### 6.3 Importar Nuevas Cuentas

**Ruta:** Barra lateral → **Nuevas Cuentas**

1. Suba un archivo Excel (`.xlsx`) con las columnas requeridas:
   - RUT, DV, RUT_Titular, DV_Titular, Nombre, Cta_OtBanco, Tipo_Cuenta, Cod_Banco, Cta_Estado, Observaciones
2. Revise la previsualización
3. Confirme para crear o actualizar beneficiarios con los datos bancarios

---

## 7. Generación de Archivo TEMGE

**Ruta:** Barra lateral → **Generar TEMGE**

El archivo TEMGE es el archivo de pago que se envía al banco para ejecutar las transferencias a los beneficiarios.

**Pasos:**
1. Ingrese el **período** en formato AAAAMM (ejemplo: `202603`)
2. Haga clic en **Generar**
3. Aparecerá un diálogo de confirmación indicando qué se incluirá
4. Confirme para generar y descargar el archivo

**Contenido del archivo generado:**
- Registro de cabecera con código de empresa y fecha
- Un registro de detalle por cada retención activa del período que tenga cuenta bancaria asignada
- Registro de cierre con totales

> **Importante:** Las retenciones sin cuenta bancaria asignada serán excluidas del archivo. Verifique que todos los beneficiarios tengan cuenta antes de generar.

---

## 8. Auditoría de Beneficiarios

**Ruta:** Barra lateral → **Auditoría**

Esta herramienta compara un archivo de remuneraciones contra la base de datos para detectar inconsistencias.

**Pasos:**
1. Suba un archivo `.txt` de remuneraciones
2. El sistema analiza y muestra un resumen con 4 categorías:

| Categoría | Descripción |
|-----------|-------------|
| **Solo en Archivo** | Beneficiarios presentes en el archivo pero no en la base de datos |
| **Solo en Sistema** | Beneficiarios en la base de datos pero ausentes del archivo |
| **Con Diferencias** | Beneficiarios en ambos pero con datos distintos (nombre, RUT funcionario) |
| **Coincidentes** | Beneficiarios idénticos en archivo y base de datos |

3. Navegue entre las pestañas para ver el detalle de cada categoría
4. Exporte los resultados a **Excel** o **CSV** para análisis externo

---

## 9. Mantenedores

### 9.1 Bancos

**Ruta:** Barra lateral → **Bancos**

Permite gestionar el catálogo de bancos del sistema.

**Operaciones:**
- **Crear banco:** Haga clic en "Nuevo Banco", complete código, nombre y si usa cuenta otro banco (S/N)
- **Editar banco:** Haga clic en el ícono de edición en la fila
- **Activar/Inactivar:** Use el botón de toggle en la columna Estado

> **Nota:** El Banco Estado (código 12) tiene tratamiento especial en el sistema. Los beneficiarios con Banco Estado usan el campo "Cuenta Estado" en lugar de "Cuenta Otro Banco".

### 9.2 Tipos de Cuenta

**Ruta:** Barra lateral → **Tipos de Cuenta**

Permite gestionar los tipos de cuenta bancaria (Corriente, Ahorro, Vista, etc.).

**Operaciones:**
- **Crear tipo:** Haga clic en "Nuevo Tipo de Cuenta", complete código y descripción
- **Editar tipo:** Haga clic en el ícono de edición
- **Activar/Inactivar:** Use el botón de toggle

---

## 10. Exportaciones

El sistema permite exportar datos en dos formatos:

| Formato | Extensión | Uso recomendado |
|---------|-----------|-----------------|
| Excel | `.xlsx` | Análisis con formato, filtros y colores |
| CSV | `.csv` | Importación a otros sistemas, procesamiento masivo |

**Exportaciones disponibles:**
- Lista de beneficiarios (desde la pantalla de beneficiarios)
- Resultados de auditoría (desde la pantalla de auditoría)
- Archivo TEMGE generado (desde generar TEMGE)

---

## 11. Conceptos Clave

### RUT (Rol Único Tributario)
Identificador nacional chileno. El sistema valida automáticamente el dígito verificador usando el algoritmo módulo 11. Se muestra formateado (ej. `12.345.678-5`) pero se almacena sin formato.

### Retención Judicial
Descuento aplicado sobre la remuneración de un funcionario a favor de un beneficiario, ordenado por un tribunal. Cada retención tiene un monto, código de retención, tipo de pago y período.

### Multicuenta
Situación donde un mismo beneficiario tiene múltiples retenciones en un mismo período (por ejemplo, retenciones de distintos funcionarios o con distintos montos). El sistema detecta estos casos durante la importación y permite asignar cuentas bancarias diferentes a cada retención.

### TEMGE
Formato de archivo bancario utilizado para instruir pagos masivos. El archivo contiene tres tipos de registro:
- **Cabecera:** Identificación de la empresa y fecha
- **Detalle:** Un registro por cada pago a realizar
- **Cierre:** Totales de control (monto y cantidad)

### Período
Identificador temporal en formato AAAAMM (año 4 dígitos + mes 2 dígitos). Ejemplo: `202603` = marzo 2026.

---

## 12. Preguntas Frecuentes

**¿Qué pasa si importo el mismo archivo dos veces?**
El sistema detecta archivos duplicados mediante hash SHA-256. Las retenciones existentes se actualizan en lugar de duplicarse.

**¿Puedo revertir una importación?**
Las importaciones registran si son reversibles en el log. Actualmente la funcionalidad de reversión no está implementada en la interfaz.

**¿Por qué un beneficiario aparece sin cuenta bancaria?**
Puede que haya sido creado mediante importación de remuneraciones (que no incluye datos bancarios). Use "Nuevas Cuentas" o edite el beneficiario manualmente para asignar cuenta.

**¿Qué significa "Integridad: Error" al importar TEMGE?**
El monto total o la cantidad de registros del cuerpo del archivo no coinciden con lo declarado en el registro de cierre. Verifique que el archivo no esté corrupto o truncado.

**¿Puedo editar el RUT de un beneficiario o funcionario?**
No. El RUT es el identificador principal y no puede modificarse después de la creación. Si hay un error, inactive el registro y cree uno nuevo.

---

## 13. Flujo de Trabajo Típico (Mensual)

```
1. Recibir archivo de remuneraciones del período
         │
         ▼
2. Importar Remuneraciones (preview → confirmar)
         │
         ▼
3. Revisar beneficiarios nuevos, asignar cuentas si es necesario
         │
         ▼
4. (Opcional) Importar Nuevas Cuentas si hay actualizaciones bancarias
         │
         ▼
5. Ejecutar Auditoría para verificar consistencia
         │
         ▼
6. Generar archivo TEMGE del período
         │
         ▼
7. Enviar archivo TEMGE al banco para ejecución de pagos
         │
         ▼
8. Recibir archivo TEMGE de respuesta del banco
         │
         ▼
9. Importar TEMGE de respuesta para actualizar estado de pagos
```

---

## 14. Soporte Técnico

Para reportar problemas o solicitar cambios, contacte al equipo de desarrollo proporcionando:
- Descripción del problema
- Pantalla donde ocurre
- Captura de pantalla (si aplica)
- Archivo involucrado (si aplica)
