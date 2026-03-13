-- ============================================================================
-- SRJE - Sistema de Retenciones Judiciales de Empleados
-- Script 09: Corregir tipos de cuenta para compatibilidad con archivo TEMGE
-- Base de Datos: Oracle 19c
-- Ejecutar despues de: 08
--
-- Problema: El archivo TEMGE usa codigos de tipo de cuenta (01, 02, 22, 30)
-- que no coinciden con los valores del CHECK constraint de BENEFICIARIOS (1, 2, 3).
-- Solucion: Agregar los codigos reales del TEMGE al catalogo y eliminar el CHECK
-- rigido, delegando la validacion a la tabla de catalogo.
-- ============================================================================

-- 1. Agregar codigos de cuenta que usa el archivo TEMGE
INSERT INTO TIPOS_CUENTA (COD_TIPO_CUENTA, DESCRIPCION)
    SELECT 22, 'Cuenta Vista' FROM DUAL
    WHERE NOT EXISTS (SELECT 1 FROM TIPOS_CUENTA WHERE COD_TIPO_CUENTA = 22);

INSERT INTO TIPOS_CUENTA (COD_TIPO_CUENTA, DESCRIPCION)
    SELECT 30, 'Cuenta Chequera Electronica' FROM DUAL
    WHERE NOT EXISTS (SELECT 1 FROM TIPOS_CUENTA WHERE COD_TIPO_CUENTA = 30);

-- 2. Eliminar CHECK constraint rigido de BENEFICIARIOS
--    (la validacion ahora la hace la tabla TIPOS_CUENTA como catalogo)
ALTER TABLE BENEFICIARIOS DROP CONSTRAINT CK_BENEFICIARIOS_TIPO_CTA;

COMMIT;

-- Verificacion
SELECT COD_TIPO_CUENTA, DESCRIPCION, ACTIVO FROM TIPOS_CUENTA ORDER BY COD_TIPO_CUENTA;
