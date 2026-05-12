-- ============================================================================
-- SRJE - Sistema de Retenciones Judiciales de Empleados
-- Script 15: Eliminar columnas RUT_FUNCIONARIO y DV_FUNCIONARIO de BENEFICIARIOS
-- Base de Datos: Oracle 19c
--
-- MOTIVO: Columnas desnormalizadas (violan 3NF). La relacion beneficiario-funcionario
--         se deriva de RETENIDO_JUDICIAL (RUT_TITULAR = funcionario, RUT_BENEFICIARIO = beneficiario).
--         Un beneficiario puede tener retenciones de MULTIPLES funcionarios, pero
--         estas columnas solo almacenaban UNO, causando datos incorrectos.
--
-- PATRON: Mismo que script 14 (eliminacion de NOMBRE_FUNCIONARIO).
--
-- Script IDEMPOTENTE.
-- ============================================================================

SET SERVEROUTPUT ON;

BEGIN
    -- Eliminar RUT_FUNCIONARIO si existe
    DECLARE
        v_exists NUMBER;
    BEGIN
        SELECT COUNT(*) INTO v_exists
        FROM user_tab_columns
        WHERE table_name = 'BENEFICIARIOS' AND column_name = 'RUT_FUNCIONARIO';

        IF v_exists = 1 THEN
            EXECUTE IMMEDIATE 'ALTER TABLE BENEFICIARIOS DROP COLUMN RUT_FUNCIONARIO';
            DBMS_OUTPUT.PUT_LINE('BENEFICIARIOS.RUT_FUNCIONARIO eliminada.');
        ELSE
            DBMS_OUTPUT.PUT_LINE('BENEFICIARIOS.RUT_FUNCIONARIO ya no existe, omitida.');
        END IF;
    END;

    -- Eliminar DV_FUNCIONARIO si existe
    DECLARE
        v_exists NUMBER;
    BEGIN
        SELECT COUNT(*) INTO v_exists
        FROM user_tab_columns
        WHERE table_name = 'BENEFICIARIOS' AND column_name = 'DV_FUNCIONARIO';

        IF v_exists = 1 THEN
            EXECUTE IMMEDIATE 'ALTER TABLE BENEFICIARIOS DROP COLUMN DV_FUNCIONARIO';
            DBMS_OUTPUT.PUT_LINE('BENEFICIARIOS.DV_FUNCIONARIO eliminada.');
        ELSE
            DBMS_OUTPUT.PUT_LINE('BENEFICIARIOS.DV_FUNCIONARIO ya no existe, omitida.');
        END IF;
    END;
END;
/

-- Verificacion
SELECT column_name, data_type, nullable
FROM user_tab_columns
WHERE table_name = 'BENEFICIARIOS'
ORDER BY column_id;
