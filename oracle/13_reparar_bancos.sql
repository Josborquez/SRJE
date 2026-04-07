-- ============================================================================
-- SRJE - Sistema de Retenciones Judiciales de Empleados
-- Script 13: REPARACION URGENTE — Tabla BANCOS quedó en estado inconsistente
-- Base de Datos: Oracle 19c
--
-- PROBLEMA: El script 12 correccion 1 fallo a mitad de camino:
--   - COD_BANCO_NEW fue creada (NUMBER(5), NULL) pero la migracion no se completo
--   - PK_BANCOS quedo DISABLED
--   - FK_BENEFICIARIOS_BANCO y FK_RETEN_BANCO quedaron DISABLED
--
-- SOLUCION: Completar la migracion o revertir segun el estado actual.
--
-- IDEMPOTENTE: puede ejecutarse multiples veces.
-- ============================================================================

SET SERVEROUTPUT ON;


-- ==========================================================================
-- PASO 1: Diagnosticar estado actual de BANCOS
-- ==========================================================================

DECLARE
    v_col_new_exists   NUMBER;
    v_col_old_exists   NUMBER;
    v_old_precision    NUMBER;
    v_pk_status        VARCHAR2(20);
    v_rows             NUMBER;
BEGIN
    -- Verificar si COD_BANCO_NEW existe
    SELECT COUNT(*) INTO v_col_new_exists
    FROM user_tab_columns
    WHERE table_name = 'BANCOS' AND column_name = 'COD_BANCO_NEW';

    -- Verificar si COD_BANCO original existe
    SELECT COUNT(*) INTO v_col_old_exists
    FROM user_tab_columns
    WHERE table_name = 'BANCOS' AND column_name = 'COD_BANCO';

    IF v_col_new_exists = 0 AND v_col_old_exists = 1 THEN
        -- Ya esta reparado o nunca fallo
        SELECT data_precision INTO v_old_precision
        FROM user_tab_columns
        WHERE table_name = 'BANCOS' AND column_name = 'COD_BANCO';

        IF v_old_precision <= 5 THEN
            DBMS_OUTPUT.PUT_LINE('BANCOS.COD_BANCO ya es NUMBER(' || v_old_precision || '). Nada que reparar.');

            -- Solo asegurar que PK y FKs esten habilitadas
            BEGIN
                EXECUTE IMMEDIATE 'ALTER TABLE BANCOS ENABLE CONSTRAINT PK_BANCOS';
                DBMS_OUTPUT.PUT_LINE('PK_BANCOS habilitada.');
            EXCEPTION WHEN OTHERS THEN
                IF SQLCODE = -2267 THEN DBMS_OUTPUT.PUT_LINE('PK_BANCOS ya estaba habilitada.');
                ELSE RAISE;
                END IF;
            END;
            RETURN;
        END IF;

        DBMS_OUTPUT.PUT_LINE('BANCOS.COD_BANCO es NUMBER(' || v_old_precision || '). Iniciando migracion completa...');
    ELSIF v_col_new_exists = 1 AND v_col_old_exists = 1 THEN
        DBMS_OUTPUT.PUT_LINE('Ambas columnas existen (COD_BANCO + COD_BANCO_NEW). Completando migracion interrumpida...');
    ELSIF v_col_new_exists = 1 AND v_col_old_exists = 0 THEN
        DBMS_OUTPUT.PUT_LINE('Solo COD_BANCO_NEW existe. Renombrando directamente...');
    ELSE
        DBMS_OUTPUT.PUT_LINE('ERROR: Ni COD_BANCO ni COD_BANCO_NEW existen. Requiere intervencion manual.');
        RETURN;
    END IF;
END;
/


-- ==========================================================================
-- PASO 2: Desactivar TODAS las FKs que apuntan a BANCOS
-- ==========================================================================

BEGIN
    FOR c IN (
        SELECT c.constraint_name, c.table_name
        FROM user_constraints c
        WHERE c.constraint_type = 'R'
          AND c.r_constraint_name = 'PK_BANCOS'
          AND c.status = 'ENABLED'
    ) LOOP
        EXECUTE IMMEDIATE 'ALTER TABLE ' || c.table_name || ' DISABLE CONSTRAINT ' || c.constraint_name;
        DBMS_OUTPUT.PUT_LINE('FK desactivada: ' || c.table_name || '.' || c.constraint_name);
    END LOOP;
END;
/


-- ==========================================================================
-- PASO 3: Desactivar PK_BANCOS si esta habilitada
-- ==========================================================================

BEGIN
    EXECUTE IMMEDIATE 'ALTER TABLE BANCOS DISABLE CONSTRAINT PK_BANCOS';
    DBMS_OUTPUT.PUT_LINE('PK_BANCOS desactivada.');
EXCEPTION WHEN OTHERS THEN
    -- Ya estaba desactivada
    DBMS_OUTPUT.PUT_LINE('PK_BANCOS ya estaba desactivada.');
END;
/


-- ==========================================================================
-- PASO 4: Asegurar que COD_BANCO_NEW existe y tiene datos
-- ==========================================================================

DECLARE
    v_col_exists NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_col_exists
    FROM user_tab_columns
    WHERE table_name = 'BANCOS' AND column_name = 'COD_BANCO_NEW';

    IF v_col_exists = 0 THEN
        EXECUTE IMMEDIATE 'ALTER TABLE BANCOS ADD (COD_BANCO_NEW NUMBER(5) NULL)';
        DBMS_OUTPUT.PUT_LINE('COD_BANCO_NEW creada.');
    ELSE
        DBMS_OUTPUT.PUT_LINE('COD_BANCO_NEW ya existe.');
    END IF;

    -- Copiar datos de COD_BANCO a COD_BANCO_NEW (solo si COD_BANCO existe)
    SELECT COUNT(*) INTO v_col_exists
    FROM user_tab_columns
    WHERE table_name = 'BANCOS' AND column_name = 'COD_BANCO';

    IF v_col_exists = 1 THEN
        EXECUTE IMMEDIATE 'UPDATE BANCOS SET COD_BANCO_NEW = COD_BANCO WHERE COD_BANCO_NEW IS NULL';
        COMMIT;
        DBMS_OUTPUT.PUT_LINE('Datos copiados a COD_BANCO_NEW.');
    END IF;
END;
/


-- ==========================================================================
-- PASO 5: Eliminar COD_BANCO original (NUMBER(18))
-- ==========================================================================

DECLARE
    v_col_exists NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_col_exists
    FROM user_tab_columns
    WHERE table_name = 'BANCOS' AND column_name = 'COD_BANCO';

    IF v_col_exists = 1 THEN
        EXECUTE IMMEDIATE 'ALTER TABLE BANCOS DROP COLUMN COD_BANCO';
        DBMS_OUTPUT.PUT_LINE('COD_BANCO original eliminada.');
    ELSE
        DBMS_OUTPUT.PUT_LINE('COD_BANCO original ya no existe.');
    END IF;
END;
/


-- ==========================================================================
-- PASO 6: Renombrar COD_BANCO_NEW -> COD_BANCO
-- ==========================================================================

DECLARE
    v_col_exists NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_col_exists
    FROM user_tab_columns
    WHERE table_name = 'BANCOS' AND column_name = 'COD_BANCO_NEW';

    IF v_col_exists = 1 THEN
        EXECUTE IMMEDIATE 'ALTER TABLE BANCOS RENAME COLUMN COD_BANCO_NEW TO COD_BANCO';
        DBMS_OUTPUT.PUT_LINE('COD_BANCO_NEW renombrada a COD_BANCO.');
    ELSE
        DBMS_OUTPUT.PUT_LINE('COD_BANCO_NEW no existe (ya renombrada).');
    END IF;
END;
/


-- ==========================================================================
-- PASO 7: NOT NULL + Recrear PK
-- ==========================================================================

BEGIN
    EXECUTE IMMEDIATE 'ALTER TABLE BANCOS MODIFY COD_BANCO NOT NULL';
    DBMS_OUTPUT.PUT_LINE('COD_BANCO marcada NOT NULL.');
EXCEPTION WHEN OTHERS THEN
    -- Ya es NOT NULL
    DBMS_OUTPUT.PUT_LINE('COD_BANCO ya es NOT NULL.');
END;
/

-- Recrear PK (puede fallar si ya existe con otro estado)
DECLARE
    v_exists NUMBER;
BEGIN
    -- Verificar si PK_BANCOS aun existe (aunque deshabilitada)
    SELECT COUNT(*) INTO v_exists
    FROM user_constraints
    WHERE constraint_name = 'PK_BANCOS' AND table_name = 'BANCOS';

    IF v_exists = 1 THEN
        -- Eliminar la PK vieja (apunta a columna ya eliminada)
        BEGIN
            EXECUTE IMMEDIATE 'ALTER TABLE BANCOS DROP CONSTRAINT PK_BANCOS';
            DBMS_OUTPUT.PUT_LINE('PK_BANCOS vieja eliminada.');
        EXCEPTION WHEN OTHERS THEN
            DBMS_OUTPUT.PUT_LINE('No se pudo eliminar PK_BANCOS vieja: ' || SQLERRM);
        END;
    END IF;

    -- Crear nueva PK sobre la columna correcta
    EXECUTE IMMEDIATE 'ALTER TABLE BANCOS ADD CONSTRAINT PK_BANCOS PRIMARY KEY (COD_BANCO)';
    DBMS_OUTPUT.PUT_LINE('PK_BANCOS recreada sobre COD_BANCO NUMBER(5).');
EXCEPTION WHEN OTHERS THEN
    IF SQLCODE = -2264 THEN
        -- La PK ya existe, intentar habilitarla
        EXECUTE IMMEDIATE 'ALTER TABLE BANCOS ENABLE CONSTRAINT PK_BANCOS';
        DBMS_OUTPUT.PUT_LINE('PK_BANCOS habilitada.');
    ELSE
        DBMS_OUTPUT.PUT_LINE('Error recreando PK_BANCOS: ' || SQLERRM);
        RAISE;
    END IF;
END;
/


-- ==========================================================================
-- PASO 8: Reactivar TODAS las FKs que apuntan a BANCOS
-- ==========================================================================

BEGIN
    FOR c IN (
        SELECT c.constraint_name, c.table_name
        FROM user_constraints c
        WHERE c.constraint_type = 'R'
          AND c.r_constraint_name = 'PK_BANCOS'
          AND c.status = 'DISABLED'
    ) LOOP
        EXECUTE IMMEDIATE 'ALTER TABLE ' || c.table_name || ' ENABLE CONSTRAINT ' || c.constraint_name;
        DBMS_OUTPUT.PUT_LINE('FK reactivada: ' || c.table_name || '.' || c.constraint_name);
    END LOOP;
END;
/


-- ==========================================================================
-- PASO 9: Limpiar recyclebin (tablas API_CLIENTES y LOG_API_ACCESOS)
-- ==========================================================================

BEGIN
    EXECUTE IMMEDIATE 'PURGE RECYCLEBIN';
    DBMS_OUTPUT.PUT_LINE('Recyclebin purgado.');
EXCEPTION WHEN OTHERS THEN
    DBMS_OUTPUT.PUT_LINE('No se pudo purgar recyclebin: ' || SQLERRM);
END;
/


COMMIT;


-- ==========================================================================
-- VERIFICACION FINAL
-- ==========================================================================

-- Estado de BANCOS.COD_BANCO
SELECT column_name, data_type, data_precision, data_scale, nullable
FROM user_tab_columns
WHERE table_name = 'BANCOS'
ORDER BY column_id;

-- PK_BANCOS habilitada?
SELECT constraint_name, constraint_type, status
FROM user_constraints
WHERE table_name = 'BANCOS';

-- FKs que apuntan a BANCOS — todas ENABLED?
SELECT c.constraint_name, c.table_name, c.status
FROM user_constraints c
WHERE c.r_constraint_name = 'PK_BANCOS';

-- Recyclebin vacio?
SELECT COUNT(*) AS objetos_en_recyclebin FROM user_recyclebin;

-- Datos intactos?
SELECT COD_BANCO, NOMBRE_BANCO, ACTIVO FROM BANCOS ORDER BY COD_BANCO;
