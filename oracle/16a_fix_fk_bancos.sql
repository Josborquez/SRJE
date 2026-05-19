-- ============================================================
-- 16a_fix_fk_bancos.sql
-- FIX: Habilitar PK_BANCOS (esta DISABLED) y crear FK pendiente
--
-- Ejecutar sobre la BD actual donde CUENTAS_BENEFICIARIO ya
-- existe con datos pero FK_CUENTABENEF_BANCO no se pudo crear.
-- ============================================================

SET SERVEROUTPUT ON;

-- Paso 1: Habilitar PK_BANCOS
ALTER TABLE BANCOS ENABLE CONSTRAINT PK_BANCOS;

-- Paso 2: Crear FK pendiente
ALTER TABLE CUENTAS_BENEFICIARIO ADD CONSTRAINT FK_CUENTABENEF_BANCO
    FOREIGN KEY (COD_BANCO) REFERENCES BANCOS(COD_BANCO);

-- Verificacion
SELECT constraint_name, constraint_type, status
FROM user_constraints
WHERE table_name IN ('BANCOS', 'CUENTAS_BENEFICIARIO')
  AND constraint_type IN ('P', 'R')
ORDER BY table_name, constraint_type;
