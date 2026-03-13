-- ============================================================
-- SRJE - Script de limpieza total de datos transaccionales
-- Fecha: 2026-03-13
-- Motivo: Bug en parser de remuneraciones (RUT Funcionario y
--         Beneficiario estaban invertidos en posiciones 0-9 y 10-19).
--         Se limpia todo para reimportar desde cero con el parser corregido.
--
-- TABLAS QUE SE LIMPIAN (datos transaccionales):
--   - LOG_CARGA_DETALLE, LOG_CARGAS (auditoría de importaciones)
--   - DETALLE_PAGO_TEMGE, HISTORIAL_PAGOS_TEMGE (pagos generados)
--   - AUDITORIA_CAMBIOS (auditoría de cambios manuales)
--   - RETENIDO_JUDICIAL (retenciones judiciales)
--   - FUNCIONARIOS (creados por importación)
--   - BENEFICIARIOS (creados por importación)
--
-- TABLAS QUE NO SE TOCAN (catálogos/datos maestros):
--   - BANCOS
--   - TIPOS_RETENCION
--   - TIPOS_CUENTA
-- ============================================================

-- Desactivar restricciones temporalmente no es necesario
-- si respetamos el orden de FK (hijos primero, padres después).

-- 1. Detalle de logs (hijo de LOG_CARGAS)
DELETE FROM LOG_CARGA_DETALLE;

-- 2. Cabecera de logs
DELETE FROM LOG_CARGAS;

-- 3. Detalle de pagos TEMGE (hijo de HISTORIAL_PAGOS_TEMGE)
DELETE FROM DETALLE_PAGO_TEMGE;

-- 4. Cabecera de pagos TEMGE
DELETE FROM HISTORIAL_PAGOS_TEMGE;

-- 5. Auditoría de cambios manuales
DELETE FROM AUDITORIA_CAMBIOS;

-- 6. Retenciones judiciales
DELETE FROM RETENIDO_JUDICIAL;

-- 7. Funcionarios
DELETE FROM FUNCIONARIOS;

-- 8. Beneficiarios
DELETE FROM BENEFICIARIOS;

COMMIT;

-- Verificación: todas las tablas deben quedar en 0
SELECT 'BENEFICIARIOS' AS TABLA, COUNT(*) AS REGISTROS FROM BENEFICIARIOS
UNION ALL SELECT 'FUNCIONARIOS', COUNT(*) FROM FUNCIONARIOS
UNION ALL SELECT 'RETENIDO_JUDICIAL', COUNT(*) FROM RETENIDO_JUDICIAL
UNION ALL SELECT 'HISTORIAL_PAGOS_TEMGE', COUNT(*) FROM HISTORIAL_PAGOS_TEMGE
UNION ALL SELECT 'DETALLE_PAGO_TEMGE', COUNT(*) FROM DETALLE_PAGO_TEMGE
UNION ALL SELECT 'LOG_CARGAS', COUNT(*) FROM LOG_CARGAS
UNION ALL SELECT 'LOG_CARGA_DETALLE', COUNT(*) FROM LOG_CARGA_DETALLE
UNION ALL SELECT 'AUDITORIA_CAMBIOS', COUNT(*) FROM AUDITORIA_CAMBIOS;
