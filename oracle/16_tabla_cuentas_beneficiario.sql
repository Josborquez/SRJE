-- ============================================================
-- 16_tabla_cuentas_beneficiario.sql
-- Tabla CUENTAS_BENEFICIARIO: soporte multicuenta (1:N)
--
-- IMPORTANTE: Este script es IDEMPOTENTE. Si la tabla ya existe,
-- la elimina y la recrea con los tipos correctos.
-- ============================================================

SET SERVEROUTPUT ON;

-- ==========================================================================
-- PASO 0: Diagnosticar tipos actuales en BD para definir columnas compatibles
-- ==========================================================================

DECLARE
    v_exists NUMBER;
BEGIN
    -- Si la tabla ya existe, eliminarla
    SELECT COUNT(*) INTO v_exists
    FROM user_tables WHERE table_name = 'CUENTAS_BENEFICIARIO';

    IF v_exists = 1 THEN
        EXECUTE IMMEDIATE 'DROP TABLE CUENTAS_BENEFICIARIO CASCADE CONSTRAINTS';
        DBMS_OUTPUT.PUT_LINE('Tabla CUENTAS_BENEFICIARIO existente eliminada.');
    ELSE
        DBMS_OUTPUT.PUT_LINE('Tabla CUENTAS_BENEFICIARIO no existia.');
    END IF;
END;
/

-- ==========================================================================
-- PASO 1: Diagnosticar tipos reales de COD_BANCO y TIPO_CUENTA
-- ==========================================================================

DECLARE
    v_banco_type     VARCHAR2(100);
    v_banco_prec     NUMBER;
    v_tipocta_type   VARCHAR2(100);
    v_tipocta_prec   NUMBER;
    v_rut_type       VARCHAR2(100);
    v_rut_prec       NUMBER;
BEGIN
    -- BANCOS.COD_BANCO (PK referenciada)
    SELECT data_type, NVL(data_precision, 0) INTO v_banco_type, v_banco_prec
    FROM user_tab_columns
    WHERE table_name = 'BANCOS' AND column_name = 'COD_BANCO';
    DBMS_OUTPUT.PUT_LINE('BANCOS.COD_BANCO: ' || v_banco_type || '(' || v_banco_prec || ')');

    -- BENEFICIARIOS.COD_BANCO (para referencia)
    SELECT data_type, NVL(data_precision, 0) INTO v_banco_type, v_banco_prec
    FROM user_tab_columns
    WHERE table_name = 'BENEFICIARIOS' AND column_name = 'COD_BANCO';
    DBMS_OUTPUT.PUT_LINE('BENEFICIARIOS.COD_BANCO: ' || v_banco_type || '(' || v_banco_prec || ')');

    -- BENEFICIARIOS.TIPO_CUENTA
    SELECT data_type, NVL(data_precision, 0) INTO v_tipocta_type, v_tipocta_prec
    FROM user_tab_columns
    WHERE table_name = 'BENEFICIARIOS' AND column_name = 'TIPO_CUENTA';
    DBMS_OUTPUT.PUT_LINE('BENEFICIARIOS.TIPO_CUENTA: ' || v_tipocta_type || '(' || v_tipocta_prec || ')');

    -- BENEFICIARIOS.RUT_BENEFICIARIO (UK referenciada)
    SELECT data_type, NVL(data_precision, 0) INTO v_rut_type, v_rut_prec
    FROM user_tab_columns
    WHERE table_name = 'BENEFICIARIOS' AND column_name = 'RUT_BENEFICIARIO';
    DBMS_OUTPUT.PUT_LINE('BENEFICIARIOS.RUT_BENEFICIARIO: ' || v_rut_type || '(' || v_rut_prec || ')');

    -- TIPOS_CUENTA.COD_TIPO_CUENTA (PK)
    SELECT data_type, NVL(data_precision, 0) INTO v_tipocta_type, v_tipocta_prec
    FROM user_tab_columns
    WHERE table_name = 'TIPOS_CUENTA' AND column_name = 'COD_TIPO_CUENTA';
    DBMS_OUTPUT.PUT_LINE('TIPOS_CUENTA.COD_TIPO_CUENTA: ' || v_tipocta_type || '(' || v_tipocta_prec || ')');
END;
/

-- ==========================================================================
-- PASO 2: Crear tabla con tipos que coincidan EXACTAMENTE con las tablas padre
--
-- Usa subquery en DDL dinamico para leer precision real de cada columna
-- y generar el CREATE TABLE con tipos 100% compatibles.
-- ==========================================================================

DECLARE
    v_banco_prec     NUMBER;
    v_banco_scale    NUMBER;
    v_rut_prec       NUMBER;
    v_rut_scale      NUMBER;
    v_tipocta_prec   NUMBER;
    v_tipocta_scale  NUMBER;
    v_ddl            VARCHAR2(4000);
BEGIN
    -- Leer precision real de BANCOS.COD_BANCO
    SELECT NVL(data_precision, 38), NVL(data_scale, 0) INTO v_banco_prec, v_banco_scale
    FROM user_tab_columns
    WHERE table_name = 'BANCOS' AND column_name = 'COD_BANCO';

    -- Leer precision real de BENEFICIARIOS.RUT_BENEFICIARIO
    SELECT NVL(data_precision, 38), NVL(data_scale, 0) INTO v_rut_prec, v_rut_scale
    FROM user_tab_columns
    WHERE table_name = 'BENEFICIARIOS' AND column_name = 'RUT_BENEFICIARIO';

    -- Leer precision real de BENEFICIARIOS.TIPO_CUENTA
    SELECT NVL(data_precision, 38), NVL(data_scale, 0) INTO v_tipocta_prec, v_tipocta_scale
    FROM user_tab_columns
    WHERE table_name = 'BENEFICIARIOS' AND column_name = 'TIPO_CUENTA';

    v_ddl := 'CREATE TABLE CUENTAS_BENEFICIARIO ('
        || ' ID               NUMBER(18)       GENERATED ALWAYS AS IDENTITY,'
        || ' RUT_BENEFICIARIO NUMBER(' || v_rut_prec || ',' || v_rut_scale || ') NOT NULL,'
        || ' COD_BANCO        NUMBER(' || v_banco_prec || ',' || v_banco_scale || ') NOT NULL,'
        || ' TIPO_CUENTA      NUMBER(' || v_tipocta_prec || ',' || v_tipocta_scale || ') NOT NULL,'
        || ' NUMERO_CUENTA    NVARCHAR2(15)    NOT NULL,'
        || ' ALIAS            NVARCHAR2(60),'
        || ' ORDEN            NUMBER           DEFAULT 1,'
        || ' ESTADO           NCHAR(1)         DEFAULT ''A'','
        || ' FECHA_CREACION   DATE             DEFAULT SYSDATE,'
        || ' USUARIO_CREACION NVARCHAR2(50),'
        || ' CONSTRAINT PK_CUENTAS_BENEFICIARIO PRIMARY KEY (ID)'
        || ')';

    DBMS_OUTPUT.PUT_LINE('DDL: ' || v_ddl);
    EXECUTE IMMEDIATE v_ddl;
    DBMS_OUTPUT.PUT_LINE('Tabla CUENTAS_BENEFICIARIO creada con tipos compatibles.');
END;
/

-- ==========================================================================
-- PASO 3: Asegurar que PKs padre esten habilitadas antes de crear FKs
-- ==========================================================================

DECLARE
    v_status VARCHAR2(30);
BEGIN
    -- Verificar PK_BANCOS
    SELECT status INTO v_status FROM user_constraints
    WHERE table_name = 'BANCOS' AND constraint_type = 'P';

    IF v_status = 'DISABLED' THEN
        EXECUTE IMMEDIATE 'ALTER TABLE BANCOS ENABLE CONSTRAINT PK_BANCOS';
        DBMS_OUTPUT.PUT_LINE('PK_BANCOS estaba DISABLED -> habilitada.');
    ELSE
        DBMS_OUTPUT.PUT_LINE('PK_BANCOS ya esta ENABLED.');
    END IF;
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        DBMS_OUTPUT.PUT_LINE('ADVERTENCIA: No se encontro PK en BANCOS.');
END;
/

-- ==========================================================================
-- PASO 4: Foreign keys
-- ==========================================================================

ALTER TABLE CUENTAS_BENEFICIARIO ADD CONSTRAINT FK_CUENTABENEF_BENEFICIARIO
    FOREIGN KEY (RUT_BENEFICIARIO) REFERENCES BENEFICIARIOS(RUT_BENEFICIARIO);

ALTER TABLE CUENTAS_BENEFICIARIO ADD CONSTRAINT FK_CUENTABENEF_BANCO
    FOREIGN KEY (COD_BANCO) REFERENCES BANCOS(COD_BANCO);

-- ==========================================================================
-- PASO 5: Indices
-- ==========================================================================

CREATE UNIQUE INDEX UQ_CUENTABENEF_CUENTA
    ON CUENTAS_BENEFICIARIO (RUT_BENEFICIARIO, COD_BANCO, TIPO_CUENTA, NUMERO_CUENTA);

CREATE INDEX IX_CUENTABENEF_RUT
    ON CUENTAS_BENEFICIARIO (RUT_BENEFICIARIO);

-- ==========================================================================
-- PASO 6: Seed — cuenta principal desde BENEFICIARIOS (orden=0)
-- ==========================================================================

INSERT INTO CUENTAS_BENEFICIARIO (RUT_BENEFICIARIO, COD_BANCO, TIPO_CUENTA, NUMERO_CUENTA, ALIAS, ORDEN, USUARIO_CREACION)
SELECT
    B.RUT_BENEFICIARIO,
    B.COD_BANCO,
    B.TIPO_CUENTA,
    CASE WHEN B.COD_BANCO = 12 THEN B.CTA_ESTADO ELSE B.CTA_OT_BANCO END,
    'Cuenta principal',
    0,
    'MIGRACION'
FROM BENEFICIARIOS B
WHERE B.COD_BANCO IS NOT NULL
  AND B.TIPO_CUENTA IS NOT NULL
  AND (B.CTA_ESTADO IS NOT NULL OR B.CTA_OT_BANCO IS NOT NULL);

-- ==========================================================================
-- PASO 7: Seed — cuentas adicionales desde RETENIDO_JUDICIAL
-- ==========================================================================

INSERT INTO CUENTAS_BENEFICIARIO (RUT_BENEFICIARIO, COD_BANCO, TIPO_CUENTA, NUMERO_CUENTA, ALIAS, ORDEN, USUARIO_CREACION)
SELECT DISTINCT
    R.RUT_BENEFICIARIO,
    R.COD_BANCO,
    NVL(R.TIPO_CUENTA, 1),
    CASE WHEN R.COD_BANCO = 12 THEN R.CTA_ESTADO ELSE R.CTA_OT_BANCO END,
    'Desde retenciones',
    1,
    'MIGRACION'
FROM RETENIDO_JUDICIAL R
WHERE R.COD_BANCO IS NOT NULL
  AND (R.CTA_ESTADO IS NOT NULL OR R.CTA_OT_BANCO IS NOT NULL)
  AND NOT EXISTS (
      SELECT 1 FROM CUENTAS_BENEFICIARIO CB
      WHERE CB.RUT_BENEFICIARIO = R.RUT_BENEFICIARIO
        AND CB.COD_BANCO = R.COD_BANCO
        AND CB.TIPO_CUENTA = NVL(R.TIPO_CUENTA, 1)
        AND CB.NUMERO_CUENTA = CASE WHEN R.COD_BANCO = 12 THEN R.CTA_ESTADO ELSE R.CTA_OT_BANCO END
  );

COMMIT;

-- ==========================================================================
-- VERIFICACION
-- ==========================================================================

SELECT column_name, data_type, data_precision, data_scale, nullable
FROM user_tab_columns
WHERE table_name = 'CUENTAS_BENEFICIARIO'
ORDER BY column_id;

SELECT constraint_name, constraint_type, status
FROM user_constraints
WHERE table_name = 'CUENTAS_BENEFICIARIO';

SELECT COUNT(*) AS total_cuentas FROM CUENTAS_BENEFICIARIO;
