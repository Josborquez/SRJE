-- ============================================================================
-- SRJE - Sistema de Retenciones Judiciales de Empleados
-- Script 06: Ampliar columna CTA_ESTADO de NCHAR(11) a NVARCHAR2(15)
-- Motivo: Cuentas BancoEstado pueden tener hasta 15 caracteres
-- Error: ORA-12899 al insertar valores mayores a 11 caracteres
-- ============================================================================

ALTER TABLE BENEFICIARIOS MODIFY CTA_ESTADO NVARCHAR2(15);

COMMENT ON COLUMN BENEFICIARIOS.CTA_ESTADO IS 'Numero cuenta BancoEstado (hasta 15 chars)';
