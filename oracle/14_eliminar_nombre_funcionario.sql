-- ============================================================================
-- SRJE - Sistema de Retenciones Judiciales de Empleados
-- Script 14: Eliminar columna NOMBRE_FUNCIONARIO de BENEFICIARIOS
-- Base de Datos: Oracle 19c
--
-- MOTIVO: Columna desnormalizada (viola 3NF). El nombre del funcionario
--         se obtiene siempre desde la tabla FUNCIONARIOS via JOIN.
--         El backend fue actualizado para derivar este dato dinamicamente.
--
-- COLUMNAS QUE SE MANTIENEN:
--   RUT_FUNCIONARIO  — FK a FUNCIONARIOS, necesaria para el flujo de importacion
--   DV_FUNCIONARIO   — Necesario para formatear RUT en exports
--
-- Script IDEMPOTENTE.
-- ============================================================================

SET SERVEROUTPUT ON;

BEGIN
    -- Verificar si la columna existe antes de eliminar
    DECLARE
        v_exists NUMBER;
    BEGIN
        SELECT COUNT(*) INTO v_exists
        FROM user_tab_columns
        WHERE table_name = 'BENEFICIARIOS' AND column_name = 'NOMBRE_FUNCIONARIO';

        IF v_exists = 1 THEN
            EXECUTE IMMEDIATE 'ALTER TABLE BENEFICIARIOS DROP COLUMN NOMBRE_FUNCIONARIO';
            DBMS_OUTPUT.PUT_LINE('BENEFICIARIOS.NOMBRE_FUNCIONARIO eliminada.');
        ELSE
            DBMS_OUTPUT.PUT_LINE('BENEFICIARIOS.NOMBRE_FUNCIONARIO ya no existe, omitida.');
        END IF;
    END;
END;
/

-- Verificacion
SELECT column_name, data_type, nullable
FROM user_tab_columns
WHERE table_name = 'BENEFICIARIOS'
ORDER BY column_id;
