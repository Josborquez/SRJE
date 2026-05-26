-- ============================================================
-- 16b_fix_seed_cuentas.sql
-- FIX: Insertar cuentas faltantes que el seed original omitio
-- por el bug del CASE sin COALESCE.
--
-- Ejecutar sobre la BD actual (CUENTAS_BENEFICIARIO ya existe).
-- ============================================================

SET SERVEROUTPUT ON;

-- Insertar cuentas principales faltantes desde BENEFICIARIOS
INSERT INTO CUENTAS_BENEFICIARIO (RUT_BENEFICIARIO, COD_BANCO, TIPO_CUENTA, NUMERO_CUENTA, ALIAS, ORDEN, USUARIO_CREACION)
SELECT
    B.RUT_BENEFICIARIO,
    B.COD_BANCO,
    B.TIPO_CUENTA,
    COALESCE(
        CASE WHEN B.COD_BANCO = 12 THEN B.CTA_ESTADO ELSE B.CTA_OT_BANCO END,
        B.CTA_ESTADO,
        B.CTA_OT_BANCO
    ),
    'Cuenta principal',
    0,
    'MIGRACION'
FROM BENEFICIARIOS B
WHERE B.COD_BANCO IS NOT NULL
  AND B.TIPO_CUENTA IS NOT NULL
  AND (B.CTA_ESTADO IS NOT NULL OR B.CTA_OT_BANCO IS NOT NULL)
  AND NOT EXISTS (
      SELECT 1 FROM CUENTAS_BENEFICIARIO CB
      WHERE CB.RUT_BENEFICIARIO = B.RUT_BENEFICIARIO
  );

DECLARE
    v_count NUMBER;
BEGIN
    v_count := SQL%ROWCOUNT;
    DBMS_OUTPUT.PUT_LINE('Cuentas principales faltantes insertadas: ' || v_count);
END;
/

-- Insertar cuentas adicionales faltantes desde RETENIDO_JUDICIAL
INSERT INTO CUENTAS_BENEFICIARIO (RUT_BENEFICIARIO, COD_BANCO, TIPO_CUENTA, NUMERO_CUENTA, ALIAS, ORDEN, USUARIO_CREACION)
SELECT DISTINCT
    R.RUT_BENEFICIARIO,
    R.COD_BANCO,
    NVL(R.TIPO_CUENTA, 1),
    COALESCE(
        CASE WHEN R.COD_BANCO = 12 THEN R.CTA_ESTADO ELSE R.CTA_OT_BANCO END,
        R.CTA_ESTADO,
        R.CTA_OT_BANCO
    ),
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
        AND CB.NUMERO_CUENTA = COALESCE(
            CASE WHEN R.COD_BANCO = 12 THEN R.CTA_ESTADO ELSE R.CTA_OT_BANCO END,
            R.CTA_ESTADO,
            R.CTA_OT_BANCO
        )
  );

DECLARE
    v_count NUMBER;
BEGIN
    v_count := SQL%ROWCOUNT;
    DBMS_OUTPUT.PUT_LINE('Cuentas retenciones faltantes insertadas: ' || v_count);
END;
/

COMMIT;

-- Verificacion
SELECT COUNT(*) AS total_cuentas FROM CUENTAS_BENEFICIARIO;
