# Auditoria del Esquema Real v2 -- Oracle 192.168.69.92/test

**Schema:** RETENCION | **Motor:** Oracle 19c | **Fecha:** 2026-03-25
**Metodo:** Conexion directa via Oracle.ManagedDataAccess
**Estado:** Post-ejecucion de scripts 11 y 12 (parcial)

---

## Estado Critico: Script 12 dejo BANCOS corrupta

El script `12_correcciones_post_auditoria.sql` Correccion 1 fallo a mitad de la migracion de `BANCOS.COD_BANCO`. Estado actual:

| Elemento | Estado | Problema |
|----------|--------|----------|
| BANCOS.COD_BANCO | NUMBER(18), columna #1 | **No se elimino** |
| BANCOS.COD_BANCO_NEW | NUMBER(5) NULL, columna #5 | **Columna fantasma** — la migracion se detuvo aqui |
| PK_BANCOS | **DISABLED** | Indice PK deshabilitado |
| FK_BENEFICIARIOS_BANCO | **DISABLED** | FK rota |
| FK_RETEN_BANCO | **DISABLED** | FK rota |

**Impacto:** No hay integridad referencial en las columnas COD_BANCO. Cualquier INSERT con banco invalido se aceptaria.

**Correccion:** Script `13_reparar_bancos.sql`

---

## 1. Normalizacion e Integridad

### Aplicado correctamente (scripts 11/12)

| Constraint | Tabla | Estado |
|-----------|-------|--------|
| FK_RETEN_TIPO_RETENCION | RETENIDO_JUDICIAL -> TIPOS_RETENCION | ENABLED, DEFERRED |
| FK_RETEN_BENEFICIARIO | RETENIDO_JUDICIAL -> BENEFICIARIOS | ENABLED, DEFERRED |
| FK_RETEN_FUNCIONARIO | RETENIDO_JUDICIAL -> FUNCIONARIOS | ENABLED, DEFERRED |
| FK_RETEN_TIPO_CUENTA | RETENIDO_JUDICIAL -> TIPOS_CUENTA | ENABLED |
| FK_DETPAGO_RETENIDO | DETALLE_PAGO_TEMGE -> RETENIDO_JUDICIAL | ENABLED |
| FK_DETPAGO_BENEFICIARIO | DETALLE_PAGO_TEMGE -> BENEFICIARIOS | ENABLED |
| FK_DETPAGO_TIPO_CUENTA | DETALLE_PAGO_TEMGE -> TIPOS_CUENTA | ENABLED |
| FK_BENEF_FUNCIONARIO | BENEFICIARIOS -> FUNCIONARIOS | ENABLED, DEFERRED |
| FK_BENEF_TIPO_CUENTA | BENEFICIARIOS -> TIPOS_CUENTA | ENABLED |
| UK_RETEN_UNICA | RETENIDO_JUDICIAL (4 columnas) | ENABLED |
| UK_LOG_HASH | LOG_CARGAS.HASH_ARCHIVO | UNIQUE, ENABLED |

### Pendiente de reparacion

| Constraint | Estado | Script corrector |
|-----------|--------|-----------------|
| PK_BANCOS | DISABLED | 13_reparar_bancos.sql |
| FK_BENEFICIARIOS_BANCO | DISABLED | 13_reparar_bancos.sql |
| FK_RETEN_BANCO | DISABLED | 13_reparar_bancos.sql |

---

## 2. Tipos de Datos

### Redimensionamiento

| Columna | Esperado | Real | Estado |
|---------|----------|------|--------|
| RUTs (todas las tablas) | NUMBER(10) | NUMBER(10) | OK |
| COD_BANCO (BENEFICIARIOS, RETENIDO, DETALLE) | NUMBER(5) | NUMBER(5) | OK |
| **COD_BANCO (BANCOS PK)** | NUMBER(5) | **NUMBER(18)** | **FALLO** |
| TIPO_CUENTA | NUMBER(5) | NUMBER(5) | OK |
| MONTO/MONTO_PAGADO/MONTO_TOTAL | NUMBER(14,2) | NUMBER(14,2) | OK |
| TIPOS_CUENTA.DESCRIPCION | NVARCHAR2 | NVARCHAR2(100) | OK |
| TIPOS_CUENTA.ACTIVO | NCHAR | NCHAR(1) | OK |
| FECHA_PROCESO | TIMESTAMP | TIMESTAMP(6) | OK |

---

## 3. Indices — Estado actual: 32

| Tabla | Indices | Notas |
|-------|---------|-------|
| RETENIDO_JUDICIAL | 5 | IDX_RETEN_PERIODO eliminado correctamente |
| AUDITORIA_CAMBIOS | 5 | IDX_AUD_FECHA posiblemente redundante |
| BENEFICIARIOS | 5 | OK |
| DETALLE_PAGO_TEMGE | 4 | OK |
| LOG_CARGAS | 4 | UK_LOG_HASH ahora UNIQUE |
| LOG_CARGA_DETALLE | 3 | OK |
| HISTORIAL_PAGOS_TEMGE | 2 | OK |
| FUNCIONARIOS | 2 | OK |
| Catalogos (3 tablas) | 3 | Solo PKs |

Tablas API_CLIENTES y LOG_API_ACCESOS eliminadas correctamente (6 indices menos).

---

## 4. Recyclebin

Las tablas eliminadas (API_CLIENTES, LOG_API_ACCESOS) fueron al recyclebin de Oracle en vez de eliminarse permanentemente. Hay objetos BIN$ visibles en constraints.

**Correccion:** `PURGE RECYCLEBIN` incluido en script 13.

---

## 5. Resumen de acciones

| Prioridad | Accion | Script |
|-----------|--------|--------|
| **URGENTE** | Reparar BANCOS: completar migracion COD_BANCO, reactivar PK + FKs | `13_reparar_bancos.sql` |
| **URGENTE** | Purgar recyclebin | `13_reparar_bancos.sql` |
| BAJA | Constraints SYS_C00xxxx sin nombre explicito | Cosmetico, no funcional |
| BAJA | Secuencias IDENTITY con cache=20 | Optimizacion futura para cargas masivas |

## 6. Orden de ejecucion

```
1. 13_reparar_bancos.sql   <-- EJECUTAR PRIMERO
2. Verificar con las queries de verificacion al final del script
3. Solo cuando BANCOS este OK, proceder con cargas de datos
```
