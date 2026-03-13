-- ============================================================================
-- SRJE - Sistema de Retenciones Judiciales de Empleados
-- Script 08: Tabla TIPOS_CUENTA (catalogo de tipos de cuenta bancaria)
-- Base de Datos: Oracle 19c
-- Ejecutar despues de: 01, 02, 03, 04
-- ============================================================================

CREATE TABLE TIPOS_CUENTA (
    COD_TIPO_CUENTA   NUMBER(10)     NOT NULL,
    DESCRIPCION       VARCHAR2(100)  NOT NULL,
    ACTIVO            CHAR(1)        DEFAULT 'S' NOT NULL,
    CONSTRAINT PK_TIPOS_CUENTA PRIMARY KEY (COD_TIPO_CUENTA),
    CONSTRAINT CK_TIPOS_CUENTA_ACTIVO CHECK (ACTIVO IN ('S', 'N'))
);

COMMENT ON TABLE TIPOS_CUENTA IS 'Catalogo de tipos de cuenta bancaria';
COMMENT ON COLUMN TIPOS_CUENTA.COD_TIPO_CUENTA IS 'Codigo del tipo de cuenta';
COMMENT ON COLUMN TIPOS_CUENTA.DESCRIPCION IS 'Descripcion del tipo de cuenta';
COMMENT ON COLUMN TIPOS_CUENTA.ACTIVO IS 'S=Activo, N=Inactivo';

-- Datos iniciales
INSERT INTO TIPOS_CUENTA (COD_TIPO_CUENTA, DESCRIPCION) VALUES (1, 'Cuenta Corriente');
INSERT INTO TIPOS_CUENTA (COD_TIPO_CUENTA, DESCRIPCION) VALUES (2, 'Cuenta de Ahorro / CuentaRUT');
INSERT INTO TIPOS_CUENTA (COD_TIPO_CUENTA, DESCRIPCION) VALUES (3, 'Cuenta Vista');

COMMIT;
