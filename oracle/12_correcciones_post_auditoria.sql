-- ============================================================================
-- SRJE - Sistema de Retenciones Judiciales de Empleados
-- Script 12: Correcciones post-auditoria del esquema real
-- Base de Datos: Oracle 19c (192.168.69.92/test)
-- Fecha: 2026-03-25
--
-- Hallazgos corregidos:
--   1. BANCOS.COD_BANCO sigue NUMBER(18), FKs hijas ya son NUMBER(5)
--   2. IDX_LOG_HASH es NONUNIQUE, deberia ser UNIQUE
--   3. IDX_RETEN_PERIODO es redundante
--   4. Tablas API_CLIENTES y LOG_API_ACCESOS huerfanas
--   5. IDX_AUD_FECHA redundante con IDX_AUD_ENTIDAD_FECHA
--
-- Script IDEMPOTENTE — puede ejecutarse multiples veces.
-- ============================================================================

SET SERVEROUTPUT ON;


-- ==========================================================================
-- CORRECCION 1: BANCOS.COD_BANCO NUMBER(18) -> NUMBER(5)
-- Problema: Oracle no permite reducir precision con datos en la columna.
-- Solucion: Crear columna temporal, migrar, eliminar original, renombrar.
-- ==========================================================================

DECLARE
    v_precision NUMBER;
BEGIN
    SELECT data_precision INTO v_precision
    FROM user_tab_columns
    WHERE table_name = 'BANCOS' AND column_name = 'COD_BANCO';

    IF v_precision > 5 THEN
        DBMS_OUTPUT.PUT_LINE('BANCOS.COD_BANCO es NUMBER(' || v_precision || '), corrigiendo a NUMBER(5)...');

        -- 1a. Desactivar FKs que referencian BANCOS.COD_BANCO
        FOR c IN (
            SELECT constraint_name, table_name
            FROM user_constraints
            WHERE r_constraint_name = 'PK_BANCOS' AND status = 'ENABLED'
        ) LOOP
            EXECUTE IMMEDIATE 'ALTER TABLE ' || c.table_name || ' DISABLE CONSTRAINT ' || c.constraint_name;
            DBMS_OUTPUT.PUT_LINE('  FK desactivada: ' || c.table_name || '.' || c.constraint_name);
        END LOOP;

        -- 1b. Desactivar PK de BANCOS
        EXECUTE IMMEDIATE 'ALTER TABLE BANCOS DISABLE CONSTRAINT PK_BANCOS';

        -- 1c. Agregar columna temporal
        EXECUTE IMMEDIATE 'ALTER TABLE BANCOS ADD (COD_BANCO_NEW NUMBER(5) NULL)';

        -- 1d. Copiar datos
        EXECUTE IMMEDIATE 'UPDATE BANCOS SET COD_BANCO_NEW = COD_BANCO';
        COMMIT;

        -- 1e. Eliminar columna original
        EXECUTE IMMEDIATE 'ALTER TABLE BANCOS DROP COLUMN COD_BANCO';

        -- 1f. Renombrar nueva columna
        EXECUTE IMMEDIATE 'ALTER TABLE BANCOS RENAME COLUMN COD_BANCO_NEW TO COD_BANCO';

        -- 1g. Agregar NOT NULL
        EXECUTE IMMEDIATE 'ALTER TABLE BANCOS MODIFY COD_BANCO NOT NULL';

        -- 1h. Recrear PK
        EXECUTE IMMEDIATE 'ALTER TABLE BANCOS ADD CONSTRAINT PK_BANCOS PRIMARY KEY (COD_BANCO)';

        -- 1i. Reactivar FKs
        FOR c IN (
            SELECT constraint_name, table_name
            FROM user_constraints
            WHERE r_constraint_name = 'PK_BANCOS' AND status = 'DISABLED'
        ) LOOP
            EXECUTE IMMEDIATE 'ALTER TABLE ' || c.table_name || ' ENABLE CONSTRAINT ' || c.constraint_name;
            DBMS_OUTPUT.PUT_LINE('  FK reactivada: ' || c.table_name || '.' || c.constraint_name);
        END LOOP;

        DBMS_OUTPUT.PUT_LINE('BANCOS.COD_BANCO corregido a NUMBER(5).');
    ELSE
        DBMS_OUTPUT.PUT_LINE('BANCOS.COD_BANCO ya es NUMBER(' || v_precision || '), nada que hacer.');
    END IF;
END;
/


-- ==========================================================================
-- CORRECCION 2: IDX_LOG_HASH debe ser UNIQUE (prevenir cargas duplicadas)
-- Problema: El indice actual es NONUNIQUE, se necesita UNIQUE.
-- ==========================================================================

DECLARE
    v_uniqueness VARCHAR2(20);
BEGIN
    SELECT uniqueness INTO v_uniqueness
    FROM user_indexes
    WHERE index_name = 'IDX_LOG_HASH';

    IF v_uniqueness = 'NONUNIQUE' THEN
        EXECUTE IMMEDIATE 'DROP INDEX IDX_LOG_HASH';
        EXECUTE IMMEDIATE 'CREATE UNIQUE INDEX UK_LOG_HASH ON LOG_CARGAS(HASH_ARCHIVO)';
        DBMS_OUTPUT.PUT_LINE('IDX_LOG_HASH reemplazado por UK_LOG_HASH (UNIQUE).');
    ELSE
        DBMS_OUTPUT.PUT_LINE('IDX_LOG_HASH ya es UNIQUE, nada que hacer.');
    END IF;
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        -- Puede que ya se llame UK_LOG_HASH
        BEGIN
            SELECT uniqueness INTO v_uniqueness
            FROM user_indexes WHERE index_name = 'UK_LOG_HASH';
            DBMS_OUTPUT.PUT_LINE('UK_LOG_HASH ya existe como UNIQUE.');
        EXCEPTION
            WHEN NO_DATA_FOUND THEN
                EXECUTE IMMEDIATE 'CREATE UNIQUE INDEX UK_LOG_HASH ON LOG_CARGAS(HASH_ARCHIVO)';
                DBMS_OUTPUT.PUT_LINE('UK_LOG_HASH creado.');
        END;
END;
/


-- ==========================================================================
-- CORRECCION 3: Eliminar indice redundante IDX_RETEN_PERIODO
-- Cubierto por IDX_RETEN_BENEF_PERIODO y UK_RETEN_UNICA.
-- ==========================================================================

BEGIN
    EXECUTE IMMEDIATE 'DROP INDEX IDX_RETEN_PERIODO';
    DBMS_OUTPUT.PUT_LINE('IDX_RETEN_PERIODO eliminado (redundante).');
EXCEPTION WHEN OTHERS THEN
    IF SQLCODE = -1418 THEN
        DBMS_OUTPUT.PUT_LINE('IDX_RETEN_PERIODO no existe, omitido.');
    ELSE RAISE;
    END IF;
END;
/


-- ==========================================================================
-- CORRECCION 4: Eliminar tablas huerfanas API_CLIENTES y LOG_API_ACCESOS
-- No tienen implementacion en el backend (SrjeDbContext no las mapea).
-- Se pueden recrear cuando se implemente el modulo de API publica.
-- ==========================================================================

-- 4a. LOG_API_ACCESOS primero (tiene FK a API_CLIENTES)
BEGIN
    EXECUTE IMMEDIATE 'DROP TABLE LOG_API_ACCESOS CASCADE CONSTRAINTS';
    DBMS_OUTPUT.PUT_LINE('LOG_API_ACCESOS eliminada.');
EXCEPTION WHEN OTHERS THEN
    IF SQLCODE = -942 THEN
        DBMS_OUTPUT.PUT_LINE('LOG_API_ACCESOS no existe, omitida.');
    ELSE RAISE;
    END IF;
END;
/

-- 4b. API_CLIENTES
BEGIN
    EXECUTE IMMEDIATE 'DROP TABLE API_CLIENTES CASCADE CONSTRAINTS';
    DBMS_OUTPUT.PUT_LINE('API_CLIENTES eliminada.');
EXCEPTION WHEN OTHERS THEN
    IF SQLCODE = -942 THEN
        DBMS_OUTPUT.PUT_LINE('API_CLIENTES no existe, omitida.');
    ELSE RAISE;
    END IF;
END;
/


-- ==========================================================================
-- CORRECCION 5: Eliminar IDX_AUD_FECHA (redundante)
-- IDX_AUD_ENTIDAD_FECHA(ENTIDAD, ID_ENTIDAD, FECHA) cubre las busquedas
-- por entidad+fecha. Para busquedas solo por fecha, este indice era util,
-- pero en la practica la auditoria siempre se consulta por entidad.
-- NOTA: Si se necesitan busquedas globales por rango de fecha, mantenerlo.
-- ==========================================================================

-- DESCOMENTAR SOLO SI SE CONFIRMA QUE NO SE BUSCA POR FECHA GLOBAL:
-- BEGIN
--     EXECUTE IMMEDIATE 'DROP INDEX IDX_AUD_FECHA';
--     DBMS_OUTPUT.PUT_LINE('IDX_AUD_FECHA eliminado (redundante).');
-- EXCEPTION WHEN OTHERS THEN
--     IF SQLCODE = -1418 THEN DBMS_OUTPUT.PUT_LINE('IDX_AUD_FECHA no existe.');
--     ELSE RAISE;
--     END IF;
-- END;
-- /


COMMIT;


-- ==========================================================================
-- VERIFICACION
-- ==========================================================================

-- Verificar precision de BANCOS.COD_BANCO
SELECT column_name, data_type, data_precision, data_scale
FROM user_tab_columns
WHERE table_name = 'BANCOS' AND column_name = 'COD_BANCO';

-- Verificar indice HASH es UNIQUE
SELECT index_name, uniqueness
FROM user_indexes
WHERE table_name = 'LOG_CARGAS' AND index_name IN ('IDX_LOG_HASH', 'UK_LOG_HASH');

-- Verificar que IDX_RETEN_PERIODO ya no existe
SELECT index_name FROM user_indexes WHERE index_name = 'IDX_RETEN_PERIODO';

-- Verificar tablas (deben ser 10, sin API_CLIENTES ni LOG_API_ACCESOS)
SELECT table_name FROM user_tables ORDER BY table_name;

-- Conteo total de indices
SELECT COUNT(*) AS total_indices FROM user_indexes WHERE index_type = 'NORMAL';
