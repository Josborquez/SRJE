-- ============================================================================
-- SRJE - Sistema de Retenciones Judiciales de Empleados
-- Script 05: Agregar columnas de Funcionario a tabla BENEFICIARIOS
-- Base de Datos: Oracle 19c
-- ============================================================================

-- Agregar columnas de datos del funcionario asociado al beneficiario
ALTER TABLE BENEFICIARIOS ADD (
    RUT_FUNCIONARIO       NUMBER(18,0)        NULL,
    DV_FUNCIONARIO        NCHAR(1)            NULL,
    NOMBRE_FUNCIONARIO    NVARCHAR2(100)      NULL
);

COMMENT ON COLUMN BENEFICIARIOS.RUT_FUNCIONARIO IS 'RUT del funcionario asociado al beneficiario';
COMMENT ON COLUMN BENEFICIARIOS.DV_FUNCIONARIO IS 'Digito verificador del RUT del funcionario';
COMMENT ON COLUMN BENEFICIARIOS.NOMBRE_FUNCIONARIO IS 'Nombre completo del funcionario asociado';

-- Indice para busquedas por funcionario
CREATE INDEX IDX_BENEFICIARIOS_RUT_FUNC ON BENEFICIARIOS (RUT_FUNCIONARIO);
