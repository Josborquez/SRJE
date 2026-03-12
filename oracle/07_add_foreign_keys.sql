-- ============================================================================
-- SRJE - Sistema de Retenciones Judiciales de Empleados
-- Script 07: Agregar Foreign Keys faltantes para integridad referencial
-- Base de Datos: Oracle 19c
-- Ejecutar despues de: 01 a 06
-- ============================================================================

-- FK: BENEFICIARIOS.COD_BANCO -> BANCOS.COD_BANCO
ALTER TABLE BENEFICIARIOS ADD CONSTRAINT FK_BENEFICIARIOS_BANCO
    FOREIGN KEY (COD_BANCO) REFERENCES BANCOS(COD_BANCO);

-- FK: DETALLE_PAGO_TEMGE.RUT_BENEFICIARIO -> BENEFICIARIOS.RUT_BENEFICIARIO
-- NOTA: No se agrega FK de RETENIDO_JUDICIAL a BENEFICIARIOS/FUNCIONARIOS
-- porque las retenciones pueden existir antes de que el beneficiario sea creado
-- en el sistema (se crean durante la importacion de remuneraciones).
