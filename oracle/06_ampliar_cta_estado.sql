-- ============================================================================
-- SRJE - Sistema de Retenciones Judiciales de Empleados
-- Script 06: Asegurar columna CTA_ESTADO como NVARCHAR2(15)
-- Motivo: Cuentas BancoEstado pueden tener hasta 15 caracteres
-- NOTA: El script 01 ya define CTA_ESTADO como NVARCHAR2(15).
--       Este ALTER es idempotente y solo es necesario si se ejecuto
--       una version anterior del script 01 que definia NCHAR(11).
-- ============================================================================

ALTER TABLE BENEFICIARIOS MODIFY CTA_ESTADO NVARCHAR2(15);

COMMENT ON COLUMN BENEFICIARIOS.CTA_ESTADO IS 'Numero cuenta BancoEstado (hasta 15 chars)';
