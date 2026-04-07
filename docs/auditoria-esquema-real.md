# Auditoria del Esquema Real -- Oracle 192.168.69.92/test

**Schema:** RETENCION | **Motor:** Oracle 19c | **Fecha:** 2026-03-25
**Metodo:** Conexion directa via Oracle.ManagedDataAccess, extraccion de user_tables/user_constraints/user_indexes

## Resumen del Estado

| Metrica | Valor |
|---------|-------|
| Tablas | 12 (10 activas + 2 huerfanas) |
| Constraints | ~60 (PKs, FKs, UNIQUEs, CHECKs) |
| Indices | 37 |
| Filas transaccionales | 0 (base limpia) |
| Catalogos cargados | BANCOS(13), TIPOS_RETENCION(13), TIPOS_CUENTA(5) |
| Secuencias IDENTITY | 10 (todas en last=1) |

---

## 1. Normalizacion e Integridad

### ALTA: BANCOS.COD_BANCO inconsistencia de precision

`BANCOS.COD_BANCO` es `NUMBER(18)` (PK). Las FKs hijas ya se redimensionaron a `NUMBER(5)`. Oracle convierte implicitamente pero genera overhead en JOINs.

**Estado:** Corregido en script 12 (recreacion de columna).

### ALTA: IDX_LOG_HASH no es UNIQUE

El indice sobre `LOG_CARGAS.HASH_ARCHIVO` es `NONUNIQUE`. No previene cargas duplicadas del mismo archivo.

**Estado:** Corregido en script 12 (reemplazo por UK_LOG_HASH UNIQUE).

### ALTA: Tablas API_CLIENTES y LOG_API_ACCESOS huerfanas

Existen en la base pero no en el backend (SrjeDbContext no las mapea). Fueron declaradas como eliminadas en el script 02 pero nunca se ejecuto el DROP.

**Estado:** Corregido en script 12 (DROP TABLE).

### OK: FKs aplicadas correctamente

Todas las FKs del script 11 estan presentes y ENABLED:

| FK | Tabla | Referencia | Deferrable |
|----|-------|-----------|------------|
| FK_RETEN_BANCO | RETENIDO_JUDICIAL.COD_BANCO | BANCOS.COD_BANCO | DEFERRED |
| FK_RETEN_TIPO_RETENCION | RETENIDO_JUDICIAL.COD_RETENCION | TIPOS_RETENCION | DEFERRED |
| FK_RETEN_BENEFICIARIO | RETENIDO_JUDICIAL.RUT_BENEFICIARIO | BENEFICIARIOS | DEFERRED |
| FK_RETEN_FUNCIONARIO | RETENIDO_JUDICIAL.RUT_TITULAR | FUNCIONARIOS | DEFERRED |
| FK_RETEN_TIPO_CUENTA | RETENIDO_JUDICIAL.TIPO_CUENTA | TIPOS_CUENTA | IMMEDIATE |
| FK_DETPAGO_RETENIDO | DETALLE_PAGO_TEMGE.ID_RETENIDO_JUDICIAL | RETENIDO_JUDICIAL | IMMEDIATE |
| FK_DETPAGO_BENEFICIARIO | DETALLE_PAGO_TEMGE.RUT_BENEFICIARIO | BENEFICIARIOS | IMMEDIATE |
| FK_DETPAGO_TIPO_CUENTA | DETALLE_PAGO_TEMGE.TIPO_CUENTA | TIPOS_CUENTA | IMMEDIATE |
| FK_BENEF_FUNCIONARIO | BENEFICIARIOS.RUT_FUNCIONARIO | FUNCIONARIOS | DEFERRED |
| FK_BENEF_TIPO_CUENTA | BENEFICIARIOS.TIPO_CUENTA | TIPOS_CUENTA | IMMEDIATE |
| FK_BENEFICIARIOS_BANCO | BENEFICIARIOS.COD_BANCO | BANCOS | IMMEDIATE |

### OK: UNIQUE y CHECK constraints aplicados

| Constraint | Tabla | Estado |
|-----------|-------|--------|
| UK_RETEN_UNICA | RETENIDO_JUDICIAL | ENABLED |
| UK_BENEFICIARIOS_RUT | BENEFICIARIOS | ENABLED |
| UK_FUNCIONARIOS_RUT | FUNCIONARIOS | ENABLED |
| CK_RETEN_MONTO_POS | RETENIDO_JUDICIAL | ENABLED |
| CK_RETEN_RUT_TITULAR_POS | RETENIDO_JUDICIAL | ENABLED |
| CK_RETEN_RUT_BENEF_POS | RETENIDO_JUDICIAL | ENABLED |
| CK_BENEF_RUT_POS | BENEFICIARIOS | ENABLED |
| CK_FUNC_RUT_POS | FUNCIONARIOS | ENABLED |
| CK_BANCOS_ACTIVO | BANCOS | ENABLED |
| CK_TIPOSRET_ACTIVO | TIPOS_RETENCION | ENABLED |
| CK_BENEF_ESTADO_CIVIL | BENEFICIARIOS | ENABLED |

---

## 2. Tipos de Datos

### Redimensionamiento aplicado correctamente

| Columna | Antes | Ahora | Estado |
|---------|-------|-------|--------|
| RUTs (todas las tablas) | NUMBER(18) | NUMBER(10) | OK |
| COD_BANCO (hijas) | NUMBER(18) | NUMBER(5) | OK |
| **COD_BANCO (BANCOS PK)** | NUMBER(18) | **NUMBER(18)** | **PENDIENTE** |
| TIPO_CUENTA | NUMBER(18) | NUMBER(5) | OK |
| MONTO/MONTO_PAGADO/MONTO_TOTAL | NUMBER(18,2) | NUMBER(14,2) | OK |
| TIPOS_CUENTA.DESCRIPCION | VARCHAR2 | NVARCHAR2(100) | OK |
| TIPOS_CUENTA.ACTIVO | CHAR | NCHAR(1) | OK |
| FECHA_PROCESO | DATE + HORA_PROCESO | TIMESTAMP(6) | OK |

---

## 3. Indices

### Estado actual: 37 indices

| Tabla | Indices | Observacion |
|-------|---------|-------------|
| RETENIDO_JUDICIAL | 6 | IDX_RETEN_PERIODO redundante |
| AUDITORIA_CAMBIOS | 5 | IDX_AUD_FECHA posiblemente redundante |
| BENEFICIARIOS | 5 | OK |
| LOG_CARGAS | 4 | IDX_LOG_HASH debe ser UNIQUE |
| DETALLE_PAGO_TEMGE | 4 | OK |
| LOG_CARGA_DETALLE | 3 | OK |
| LOG_API_ACCESOS | 4 | Tabla huerfana, eliminar |
| API_CLIENTES | 2 | Tabla huerfana, eliminar |
| Catalogos (3 tablas) | 3 | Solo PKs, OK |
| FUNCIONARIOS | 2 | OK |

### Post-correccion (script 12): ~29 indices

Se eliminan: IDX_RETEN_PERIODO, 6 indices de tablas huerfanas, IDX_LOG_HASH (reemplazado por UK_LOG_HASH).

---

## 4. Escalabilidad

### Secuencias IDENTITY

Todas con `cache=20` (default Oracle). Para cargas masivas se recomienda aumentar a `cache=1000` en las tablas de alto volumen:

| Secuencia | Tabla | Cache actual | Recomendado |
|-----------|-------|-------------|-------------|
| ISEQ$$_73096 | BENEFICIARIOS | 20 | 1000 |
| ISEQ$$_73104 | RETENIDO_JUDICIAL | 20 | 1000 |
| ISEQ$$_73110 | DETALLE_PAGO_TEMGE | 20 | 1000 |
| ISEQ$$_73117 | LOG_CARGAS | 20 | 100 |
| ISEQ$$_73120 | LOG_CARGA_DETALLE | 20 | 1000 |

---

## 5. Hallazgos Consolidados

| # | Hallazgo | Criticidad | Script 12 |
|---|----------|-----------|-----------|
| 1 | BANCOS.COD_BANCO NUMBER(18) vs FKs NUMBER(5) | **ALTA** | Correccion 1 |
| 2 | IDX_LOG_HASH NONUNIQUE | **ALTA** | Correccion 2 |
| 3 | Tablas huerfanas API_CLIENTES + LOG_API_ACCESOS | **ALTA** | Correccion 4 |
| 4 | IDX_RETEN_PERIODO redundante | **MEDIA** | Correccion 3 |
| 5 | IDX_AUD_FECHA posiblemente redundante | **BAJA** | Correccion 5 (comentada) |
| 6 | ~30 constraints SYS_C00xxxx sin nombre | **BAJA** | No corregido (cosmetico) |
| 7 | Secuencias IDENTITY con cache=20 | **BAJA** | No corregido (optimizacion futura) |
