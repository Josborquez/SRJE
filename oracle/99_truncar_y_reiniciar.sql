-- ============================================================================
-- SRJE - Sistema de Retenciones Judiciales de Empleados
-- Script 99: Truncar todas las tablas y reiniciar secuencias IDENTITY
-- Base de Datos: Oracle 19c
--
-- PROPOSITO: Dejar la base limpia para una nueva carga masiva.
--            Preserva los datos de catalogo (BANCOS, TIPOS_RETENCION, TIPOS_CUENTA).
--
-- ADVERTENCIA: Este script ELIMINA TODOS LOS DATOS transaccionales de forma
--              IRREVERSIBLE. No se puede hacer ROLLBACK de un TRUNCATE.
--              Ejecutar UNICAMENTE en ambientes de desarrollo/pruebas o cuando
--              se tenga certeza absoluta de que se desea reiniciar.
--
-- ORDEN: Las tablas se truncan de hijas a padres para respetar FKs.
--        Si hay FKs DEFERRABLE, se desactivan temporalmente.
-- ============================================================================

SET SERVEROUTPUT ON;

-- ============================================================================
-- PASO 1: DESACTIVAR FOREIGN KEYS TEMPORALMENTE
-- Necesario porque TRUNCATE no respeta DEFERRABLE y Oracle no permite
-- truncar tablas referenciadas por FKs activas.
-- ============================================================================

BEGIN
    FOR c IN (
        SELECT constraint_name, table_name
        FROM user_constraints
        WHERE constraint_type = 'R'
          AND status = 'ENABLED'
        ORDER BY table_name
    ) LOOP
        EXECUTE IMMEDIATE 'ALTER TABLE ' || c.table_name
            || ' DISABLE CONSTRAINT ' || c.constraint_name;
        DBMS_OUTPUT.PUT_LINE('FK desactivada: ' || c.table_name || '.' || c.constraint_name);
    END LOOP;
END;
/


-- ============================================================================
-- PASO 2: TRUNCAR TABLAS TRANSACCIONALES Y DE LOG
-- DROP STORAGE libera espacio inmediatamente.
-- ============================================================================

-- Tablas de detalle (hijas)
TRUNCATE TABLE LOG_CARGA_DETALLE DROP STORAGE;
TRUNCATE TABLE DETALLE_PAGO_TEMGE DROP STORAGE;

-- Tablas maestras de log
TRUNCATE TABLE AUDITORIA_CAMBIOS DROP STORAGE;
TRUNCATE TABLE LOG_CARGAS DROP STORAGE;
TRUNCATE TABLE HISTORIAL_PAGOS_TEMGE DROP STORAGE;

-- Tabla principal de retenciones
TRUNCATE TABLE RETENIDO_JUDICIAL DROP STORAGE;

-- Tablas de entidades principales
TRUNCATE TABLE BENEFICIARIOS DROP STORAGE;
TRUNCATE TABLE FUNCIONARIOS DROP STORAGE;


-- NOTA: Las tablas de catalogo (BANCOS, TIPOS_RETENCION, TIPOS_CUENTA)
--       NO se truncan. Sus datos se preservan intactos.


-- ============================================================================
-- PASO 4: REINICIAR SECUENCIAS IDENTITY A 1
-- Oracle no tiene RESTART IDENTITY en TRUNCATE. Se debe hacer ALTER TABLE
-- con MODIFY ... GENERATED ALWAYS AS IDENTITY (START WITH 1).
-- ============================================================================

-- Tablas con IDENTITY: BENEFICIARIOS, FUNCIONARIOS, RETENIDO_JUDICIAL,
--   HISTORIAL_PAGOS_TEMGE, DETALLE_PAGO_TEMGE, LOG_CARGAS,
--   LOG_CARGA_DETALLE, AUDITORIA_CAMBIOS

ALTER TABLE BENEFICIARIOS MODIFY ID GENERATED ALWAYS AS IDENTITY (START WITH 1);
ALTER TABLE FUNCIONARIOS MODIFY ID GENERATED ALWAYS AS IDENTITY (START WITH 1);
ALTER TABLE RETENIDO_JUDICIAL MODIFY ID GENERATED ALWAYS AS IDENTITY (START WITH 1);
ALTER TABLE HISTORIAL_PAGOS_TEMGE MODIFY ID GENERATED ALWAYS AS IDENTITY (START WITH 1);
ALTER TABLE DETALLE_PAGO_TEMGE MODIFY ID GENERATED ALWAYS AS IDENTITY (START WITH 1);
ALTER TABLE LOG_CARGAS MODIFY ID GENERATED ALWAYS AS IDENTITY (START WITH 1);
ALTER TABLE LOG_CARGA_DETALLE MODIFY ID GENERATED ALWAYS AS IDENTITY (START WITH 1);
ALTER TABLE AUDITORIA_CAMBIOS MODIFY ID GENERATED ALWAYS AS IDENTITY (START WITH 1);


-- ============================================================================
-- PASO 5: REACTIVAR FOREIGN KEYS
-- ============================================================================

BEGIN
    FOR c IN (
        SELECT constraint_name, table_name
        FROM user_constraints
        WHERE constraint_type = 'R'
          AND status = 'DISABLED'
        ORDER BY table_name
    ) LOOP
        EXECUTE IMMEDIATE 'ALTER TABLE ' || c.table_name
            || ' ENABLE CONSTRAINT ' || c.constraint_name;
        DBMS_OUTPUT.PUT_LINE('FK reactivada: ' || c.table_name || '.' || c.constraint_name);
    END LOOP;
END;
/


-- ============================================================================
-- PASO 6: VERIFICACION
-- ============================================================================

SELECT 'BENEFICIARIOS' AS tabla, COUNT(*) AS registros FROM BENEFICIARIOS
UNION ALL SELECT 'FUNCIONARIOS', COUNT(*) FROM FUNCIONARIOS
UNION ALL SELECT 'RETENIDO_JUDICIAL', COUNT(*) FROM RETENIDO_JUDICIAL
UNION ALL SELECT 'HISTORIAL_PAGOS_TEMGE', COUNT(*) FROM HISTORIAL_PAGOS_TEMGE
UNION ALL SELECT 'DETALLE_PAGO_TEMGE', COUNT(*) FROM DETALLE_PAGO_TEMGE
UNION ALL SELECT 'LOG_CARGAS', COUNT(*) FROM LOG_CARGAS
UNION ALL SELECT 'LOG_CARGA_DETALLE', COUNT(*) FROM LOG_CARGA_DETALLE
UNION ALL SELECT 'AUDITORIA_CAMBIOS', COUNT(*) FROM AUDITORIA_CAMBIOS
UNION ALL SELECT 'BANCOS', COUNT(*) FROM BANCOS
UNION ALL SELECT 'TIPOS_RETENCION', COUNT(*) FROM TIPOS_RETENCION
UNION ALL SELECT 'TIPOS_CUENTA', COUNT(*) FROM TIPOS_CUENTA;

-- Verificar que las FKs quedaron habilitadas
SELECT constraint_name, table_name, status
FROM user_constraints
WHERE constraint_type = 'R'
ORDER BY table_name, constraint_name;

-- ============================================================================
-- FIN. La base esta lista para una nueva carga masiva.
-- Tablas transaccionales: 0 registros, IDs empiezan desde 1.
-- Tablas catalogo: recargadas con datos iniciales.
-- ============================================================================
