# Reporte de Analisis de Base de Datos -- SRJE (Oracle 19c)

**Modulo:** Sistema de Retenciones Judiciales Electronicas
**Motor:** Oracle 19c
**Tablas analizadas:** 10 (7 principales + 3 auditoria/log)
**Fecha:** 2026-03-25

---

## 1. Analisis de Normalizacion e Integridad

### Criticidad ALTA

#### 1.1 Datos desnormalizados de Funcionario en BENEFICIARIOS (script 05)

La tabla `BENEFICIARIOS` tiene columnas `RUT_FUNCIONARIO`, `DV_FUNCIONARIO`, `NOMBRE_FUNCIONARIO` que duplican datos de `FUNCIONARIOS`. Esto genera:
- **Anomalia de actualizacion:** Si un funcionario cambia de nombre (correccion, cambio legal), hay que actualizar tanto `FUNCIONARIOS` como todas las filas de `BENEFICIARIOS` que lo referencien.
- **Anomalia de insercion:** Se puede insertar un beneficiario con un RUT de funcionario que no existe en la tabla `FUNCIONARIOS`.
- **No hay FK** de `BENEFICIARIOS.RUT_FUNCIONARIO` hacia `FUNCIONARIOS.RUT_FUNCIONARIO`.

> **Justificacion posible:** Rendimiento en consultas frecuentes que necesitan el nombre. Pero el costo de inconsistencia es alto.

#### 1.2 Datos de RUT duplicados en RETENIDO_JUDICIAL

`RETENIDO_JUDICIAL` almacena `DV_TITULAR` y `DV_BENEFICIARIO` de forma redundante. El DV es deterministico (se calcula del RUT), por lo que almacenarlo viola 3NF y crea riesgo de inconsistencia. Lo mismo ocurre con `DV_BENEFICIARIO` y `DV_FUNCIONARIO` en `BENEFICIARIOS` y `FUNCIONARIOS`.

> **Veredicto:** Violacion de 3NF. Justificable parcialmente porque el DV se necesita para generar archivos de ancho fijo (TEMGE), pero deberia calcularse en la capa de aplicacion, no almacenarse.

#### 1.3 Ausencia de FKs en RETENIDO_JUDICIAL

El script 07 explicitamente dice que no agrega FKs de `RETENIDO_JUDICIAL` hacia `BENEFICIARIOS` ni `FUNCIONARIOS` por orden de importacion. Esto es un problema real:
- No hay integridad referencial en la tabla **mas critica del sistema** (las retenciones).
- `RUT_TITULAR` puede apuntar a un funcionario inexistente permanentemente.
- `RUT_BENEFICIARIO` puede apuntar a un beneficiario inexistente permanentemente.
- `COD_BANCO` (agregado en script 10) no tiene FK a `BANCOS`.
- `COD_RETENCION` no tiene FK a `TIPOS_RETENCION`.

#### 1.4 Falta UNIQUE constraint en RETENIDO_JUDICIAL

No hay restriccion que impida duplicar una retencion para el mismo titular+beneficiario+periodo+idRetencion. El indice compuesto definido en EF Core (`RutTitular, RutBeneficiario, PeriodoProceso`) no es UNIQUE, permitiendo duplicados.

### Criticidad MEDIA

#### 1.5 Relacion BENEFICIARIO-FUNCIONARIO es N:1 pero se modela como atributo

Un beneficiario se asocia a un funcionario mediante columnas sueltas en `BENEFICIARIOS`. Si un beneficiario puede tener retenciones de **multiples** funcionarios (ej: pension alimenticia de dos pagadores), el modelo actual no lo soporta. La tabla `RETENIDO_JUDICIAL` si lo permite (tiene `RUT_TITULAR`), pero `BENEFICIARIOS` solo acepta uno.

#### 1.6 DETALLE_PAGO_TEMGE.ID_RETENIDO_JUDICIAL sin FK

El campo apunta a `RETENIDO_JUDICIAL.ID` pero no tiene `FOREIGN KEY` declarada.

#### 1.7 FK de DETALLE_PAGO_TEMGE.RUT_BENEFICIARIO declarada en script 07 pero incompleta

El script 07 tiene un comentario indicando que se agregaria `FK_DETALLE_BENEF` pero el codigo SQL no aparece -- solo esta la FK de `BENEFICIARIOS.COD_BANCO -> BANCOS`.

---

## 2. Tipos de Datos y Eficiencia

### Criticidad ALTA

#### 2.1 NUMBER(18,0) para RUTs -- sobredimensionado

Los RUTs chilenos van de 1 a ~99.999.999 (8 digitos). `NUMBER(18,0)` soporta hasta 10^18. Un `NUMBER(10,0)` es mas que suficiente y consume menos bytes en Oracle.

| Columna | Actual | Recomendado |
|---------|--------|-------------|
| RUT_BENEFICIARIO | NUMBER(18,0) | NUMBER(10,0) |
| RUT_FUNCIONARIO | NUMBER(18,0) | NUMBER(10,0) |
| RUT_TITULAR | NUMBER(18,0) | NUMBER(10,0) |
| ID_RETENCION | NUMBER(18,0) | NUMBER(10,0) |

#### 2.2 NUMBER(18,0) para COD_BANCO -- sobredimensionado

Codigos de banco van de 1 a ~999. `NUMBER(18,0)` es excesivo. `NUMBER(5,0)` cubre de sobra.

#### 2.3 NUMBER(18,2) para MONTO -- desperdicio

Las retenciones son en pesos chilenos. El monto maximo razonable es del orden de miles de millones (~10^10). `NUMBER(14,2)` basta y es mas eficiente.

### Criticidad MEDIA

#### 2.4 NVARCHAR2 / NCHAR donde VARCHAR2 / CHAR basta

Todo el esquema usa `NVARCHAR2` y `NCHAR` (Unicode, 2 bytes/char). Los datos son exclusivamente texto en espanol (ASCII + tildes). `VARCHAR2` con `AL32UTF8` (charset estandar en Oracle 19c) soporta esto con menos overhead. Si la base ya usa `AL32UTF8`, `NVARCHAR2` duplica almacenamiento innecesariamente.

| Impacto estimado | ~40-50% mas espacio en columnas de texto |
|------------------|------------------------------------------|

#### 2.5 HORA_PROCESO como NCHAR(6) en vez de parte de un TIMESTAMP

`HISTORIAL_PAGOS_TEMGE.HORA_PROCESO` almacena "HHMMSS" como string. Deberia combinarse con `FECHA_PROCESO` en un solo campo `TIMESTAMP`. Almacenarlo como string impide operaciones temporales nativas.

#### 2.6 PERIODO_PROCESO como NCHAR(6)

Almacenar "AAAAMM" como string impide filtros de rango eficientes (ej: "todos los periodos de 2025"). Podria ser `DATE` (primer dia del mes) o al menos `NUMBER(6,0)` para permitir operaciones aritmeticas.

### Criticidad BAJA

#### 2.7 ESTADO como NCHAR(2) en LOG_CARGA_DETALLE

`LOG_CARGA_DETALLE.ESTADO` es `NCHAR(2)` con valores 'OK', 'W', 'E'. Sin embargo 'OK' son 2 caracteres mientras 'W' y 'E' son 1 -- `NCHAR(2)` paddeara con espacios, causando problemas en comparaciones si la aplicacion no usa `TRIM()`. Deberia ser `VARCHAR2(2)`.

---

## 3. Estrategia de Indexacion

### Indices existentes -- Evaluacion

| Indice | Veredicto |
|--------|-----------|
| `IDX_BENEF_ESTADO` | **Baja selectividad.** Solo 3 valores ('A','I','S'). Oracle lo ignora en tablas medianas. Eliminar. |
| `IDX_RETEN_ESTADO` | **Mismo problema.** Solo 2 valores. Eliminar. |
| `IDX_HIST_ESTADO` | **Mismo problema.** Eliminar. |
| `IDX_LOG_ESTADO` | **Mismo problema.** Eliminar. |
| `IDX_BENEF_NOMBRE` | **Util** para busquedas por nombre, pero no sirve para LIKE '%nombre%'. |
| `IDX_LOG_HASH` | **Correcto.** Busqueda de duplicados por igualdad exacta. |

### Indices faltantes sugeridos

| Indice | Tabla | Columnas | Criticidad |
|--------|-------|----------|------------|
| IDX_RETEN_TITULAR_ESTADO | RETENIDO_JUDICIAL | (RUT_TITULAR, ESTADO) | ALTA |
| IDX_RETEN_BENEF_PERIODO | RETENIDO_JUDICIAL | (RUT_BENEFICIARIO, PERIODO_PROCESO) | ALTA |
| IDX_LOG_TIPO_PERIODO | LOG_CARGAS | (TIPO_CARGA, PERIODO_PROCESO) | MEDIA |
| IDX_AUD_ENTIDAD_FECHA | AUDITORIA_CAMBIOS | (ENTIDAD, ID_ENTIDAD, FECHA) | MEDIA |
| IDX_DET_RETENIDO | DETALLE_PAGO_TEMGE | (ID_RETENIDO_JUDICIAL) | BAJA |

### Advertencia sobre exceso de indices

`RETENIDO_JUDICIAL` ya tiene 5 indices + la PK. Con los sugeridos serian 7. Recomendacion: reemplazar los indices individuales por compuestos para mantener el mismo numero total con mejor cobertura.

---

## 4. Escalabilidad y Cuellos de Botella

### Criticidad ALTA

#### 4.1 LOG_CARGA_DETALLE -- tabla de crecimiento explosivo

Cada importacion genera una fila por linea del archivo. Con archivos de miles de lineas procesados mensualmente, esta tabla crece rapido. Ademas, usa 3 columnas `NCLOB` (JSON), que Oracle almacena out-of-row.

**Recomendacion:**
- Particionamiento por rango en `ID_CARGA` o por fecha (requiere agregar columna de fecha).
- Politica de retencion: mover detalles de cargas > 12 meses a tabla historica o comprimir.

#### 4.2 AUDITORIA_CAMBIOS -- crecimiento proporcional a operaciones

Cada cambio manual genera N filas (una por campo modificado). Sin politica de purgado, crecera indefinidamente.

**Recomendacion:** Particionamiento por `FECHA` (interval partitioning mensual).

### Criticidad MEDIA

#### 4.3 RETENIDO_JUDICIAL sin particionamiento

Las retenciones se acumulan por periodo. Consultas tipicas filtran por `PERIODO_PROCESO`. Candidata ideal para interval partitioning por periodo.

#### 4.4 IDENTITY columns -- secuencia implicita

`GENERATED ALWAYS AS IDENTITY` crea secuencias con cache por defecto de 20 en Oracle. En cargas masivas concurrentes, esto puede generar contencion. Considerar `CACHE 1000` para tablas de alto volumen.

---

## 5. Convenciones y Buenas Practicas

### Criticidad MEDIA

#### 5.1 Inconsistencia en tipos de datos entre scripts

| Tabla | Columna | Script 01 | Script 08 |
|-------|---------|-----------|-----------|
| TIPOS_CUENTA | DESCRIPCION | -- | `VARCHAR2(100)` |
| TIPOS_CUENTA | ACTIVO | -- | `CHAR(1)` |
| Resto del esquema | * | `NVARCHAR2` / `NCHAR` | -- |

`TIPOS_CUENTA` usa `VARCHAR2`/`CHAR` mientras todo el resto del esquema usa `NVARCHAR2`/`NCHAR`. Esto genera inconsistencia y potenciales problemas de conversion implicita en JOINs.

#### 5.2 Convencion de nombres de constraints inconsistente

- PKs: `PK_NOMBRE_TABLA` -- consistente
- FKs: mezcla `FK_BENEFICIARIOS_BANCO`, `FK_DETALLE_HISTORIAL`, `FK_LOG_CARGA_DETALLE` -- inconsistente
- CHECKs: `CK_BENEFICIARIOS_SEXO`, `CK_RETEN_ESTADO` -- abrevian distinto

**Recomendacion:** `{TIPO}_{TABLA_ORIGEN}_{DESCRIPCION}` (ej: `FK_DETPAGO_HISTORIAL`, `CK_BENEF_SEXO`).

#### 5.3 Constraints faltantes

| Tabla | Constraint faltante | Criticidad |
|-------|---------------------|------------|
| BANCOS | `CHECK (ACTIVO IN ('S','N'))` | Media |
| TIPOS_RETENCION | `CHECK (ACTIVO IN ('S','N'))` | Media |
| RETENIDO_JUDICIAL | `UNIQUE (ID_RETENCION, PERIODO_PROCESO)` | Alta |
| BENEFICIARIOS | `CHECK (RUT_BENEFICIARIO > 0)` | Baja |
| FUNCIONARIOS | `FECHA_CREACION`, `FECHA_MODIFICACION` columnas ausentes | Media |
| HISTORIAL_PAGOS_TEMGE | `PERIODO_PROCESO` columna ausente | Media |
| LOG_CARGAS | `UNIQUE (HASH_ARCHIVO)` para prevenir duplicados | Alta |

#### 5.4 RETENIDO_JUDICIAL.MONTO sin CHECK > 0

Se podria insertar una retencion con monto negativo o cero.

---

## 6. Resumen Ejecutivo

| # | Hallazgo | Criticidad | Esfuerzo |
|---|----------|-----------|----------|
| 1 | FKs ausentes en RETENIDO_JUDICIAL | **ALTA** | Bajo |
| 2 | Sin UNIQUE para evitar retenciones duplicadas | **ALTA** | Bajo |
| 3 | Sin UNIQUE en HASH_ARCHIVO (cargas duplicadas) | **ALTA** | Bajo |
| 4 | CHECK faltante en MONTO > 0 | **ALTA** | Bajo |
| 5 | Desnormalizacion de funcionario en BENEFICIARIOS | **ALTA** | Alto |
| 6 | NUMBER(18) sobredimensionado para RUTs/bancos | **MEDIA** | Medio |
| 7 | NVARCHAR2 donde VARCHAR2 basta (si charset AL32UTF8) | **MEDIA** | Alto |
| 8 | Inconsistencia VARCHAR2 vs NVARCHAR2 en TIPOS_CUENTA | **MEDIA** | Bajo |
| 9 | HORA_PROCESO como string en vez de TIMESTAMP | **MEDIA** | Medio |
| 10 | Indices de baja selectividad (ESTADO) | **MEDIA** | Bajo |
| 11 | LOG_CARGA_DETALLE sin estrategia de particionamiento | **MEDIA** | Medio |
| 12 | FK BENEFICIARIOS.TIPO_CUENTA -> TIPOS_CUENTA faltante | **MEDIA** | Bajo |
| 13 | DV almacenado redundantemente (violacion 3NF) | **BAJA** | Alto |
| 14 | PERIODO_PROCESO faltante en HISTORIAL_PAGOS_TEMGE | **BAJA** | Bajo |

**Prioridad de implementacion:** Empezar por los items 1-4 (alto impacto, bajo esfuerzo). Los items 6-7 (tipos de datos) requieren migracion de datos y se deberian planificar para una ventana de mantenimiento.
