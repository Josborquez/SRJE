-- ============================================================================
-- SRJE - Sistema de Retenciones Judiciales de Empleados
-- Script 02: Tablas de Auditoria y Logs
-- Base de Datos: Oracle 19c
-- Ejecutar despues de: 01_tablas_principales.sql
-- ============================================================================

-- --------------------------------------------------------------------------
-- 1. LOG_CARGAS — Registro maestro de cada proceso de importacion/generacion
-- --------------------------------------------------------------------------
CREATE TABLE LOG_CARGAS (
    ID                    NUMBER(18)          GENERATED ALWAYS AS IDENTITY,
    TIPO_CARGA            NVARCHAR2(30)       NOT NULL,    -- REMUNERACIONES | TEMGE_ENTRADA | TEMGE_SALIDA | NUEVAS_CUENTAS
    NOMBRE_ARCHIVO        NVARCHAR2(260)      NULL,
    HASH_ARCHIVO          NVARCHAR2(64)       NULL,        -- SHA-256 para detectar duplicados
    TAMANIO_BYTES         NUMBER(18,0)        NULL,
    PERIODO_PROCESO       NCHAR(6)            NULL,        -- AAAAMM
    FECHA_INICIO          TIMESTAMP           DEFAULT SYSTIMESTAMP,
    FECHA_FIN             TIMESTAMP           NULL,
    DURACION_MS           NUMBER(10,0)        NULL,
    ESTADO                NCHAR(1)            DEFAULT 'P', -- P=Previsualizado, C=Confirmado, E=Error, A=Anulado
    TOTAL_LINEAS          NUMBER(10,0)        NULL,
    REGISTROS_INSERTADOS  NUMBER(10,0)        NULL,
    REGISTROS_ACTUALIZADOS NUMBER(10,0)       NULL,
    REGISTROS_EXCLUIDOS   NUMBER(10,0)        NULL,
    REGISTROS_ERROR       NUMBER(10,0)        NULL,
    MONTO_TOTAL           NUMBER(18,2)        NULL,
    USUARIO               NVARCHAR2(50)       NULL,
    IP_USUARIO            NVARCHAR2(50)       NULL,
    OBSERVACIONES         NVARCHAR2(2000)     NULL,
    PUEDE_REVERTIR        NCHAR(1)            DEFAULT 'S', -- S=Permite reversion, N=No reversible
    CONSTRAINT PK_LOG_CARGAS PRIMARY KEY (ID),
    CONSTRAINT CK_LOG_CARGAS_TIPO CHECK (TIPO_CARGA IN ('REMUNERACIONES', 'TEMGE_ENTRADA', 'TEMGE_SALIDA', 'NUEVAS_CUENTAS')),
    CONSTRAINT CK_LOG_CARGAS_ESTADO CHECK (ESTADO IN ('P', 'C', 'E', 'A')),
    CONSTRAINT CK_LOG_CARGAS_REVERTIR CHECK (PUEDE_REVERTIR IN ('S', 'N'))
);

COMMENT ON TABLE LOG_CARGAS IS 'Registro maestro de cada proceso de importacion o generacion';
COMMENT ON COLUMN LOG_CARGAS.HASH_ARCHIVO IS 'SHA-256 del archivo para detectar duplicados';

-- --------------------------------------------------------------------------
-- 2. LOG_CARGA_DETALLE — Detalle linea por linea de cada carga
-- --------------------------------------------------------------------------
CREATE TABLE LOG_CARGA_DETALLE (
    ID                    NUMBER(18)          GENERATED ALWAYS AS IDENTITY,
    ID_CARGA              NUMBER(18,0)        NOT NULL,
    NUMERO_LINEA          NUMBER(10,0)        NULL,
    RUT_REFERENCIA        NVARCHAR2(15)       NULL,
    ACCION                NVARCHAR2(15)       NULL,        -- INSERTAR | ACTUALIZAR | EXCLUIR | ERROR
    ESTADO                NCHAR(2)            NULL,        -- OK | W=Advertencia | E=Error
    DATOS_ORIGINALES      NCLOB               NULL,        -- JSON datos del archivo
    DATOS_ANTERIORES      NCLOB               NULL,        -- JSON valores en BD antes
    DATOS_NUEVOS          NCLOB               NULL,        -- JSON valores escritos en BD
    MENSAJES              NVARCHAR2(2000)     NULL,
    CONSTRAINT PK_LOG_CARGA_DETALLE PRIMARY KEY (ID),
    CONSTRAINT FK_LOG_CARGA_DETALLE FOREIGN KEY (ID_CARGA) REFERENCES LOG_CARGAS(ID)
);

COMMENT ON TABLE LOG_CARGA_DETALLE IS 'Detalle linea por linea de cada carga procesada';

-- --------------------------------------------------------------------------
-- 3. AUDITORIA_CAMBIOS — Cambios manuales en fichas
-- --------------------------------------------------------------------------
CREATE TABLE AUDITORIA_CAMBIOS (
    ID                    NUMBER(18)          GENERATED ALWAYS AS IDENTITY,
    ENTIDAD               NVARCHAR2(50)       NOT NULL,    -- BENEFICIARIO | RETENCION | CUENTA_BANCARIA
    ID_ENTIDAD            NUMBER(18,0)        NOT NULL,
    RUT_AFECTADO          NVARCHAR2(15)       NULL,
    ACCION                NVARCHAR2(20)       NOT NULL,    -- INSERT | UPDATE | DELETE | INACTIVAR
    CAMPO_MODIFICADO      NVARCHAR2(60)       NULL,
    VALOR_ANTERIOR        NVARCHAR2(500)      NULL,
    VALOR_NUEVO           NVARCHAR2(500)      NULL,
    USUARIO               NVARCHAR2(50)       NOT NULL,
    FECHA                 TIMESTAMP           DEFAULT SYSTIMESTAMP,
    IP                    NVARCHAR2(50)       NULL,
    MOTIVO                NVARCHAR2(500)      NULL,        -- Obligatorio en cambios criticos
    CONSTRAINT PK_AUDITORIA_CAMBIOS PRIMARY KEY (ID)
);

COMMENT ON TABLE AUDITORIA_CAMBIOS IS 'Log de cambios manuales en fichas de beneficiarios';

-- --------------------------------------------------------------------------
-- NOTA: Las tablas API_CLIENTES y LOG_API_ACCESOS fueron eliminadas del script
-- de creacion ya que no tienen implementacion en el backend.
-- Se reimplementaran cuando se defina el modulo de autenticacion/API publica.
-- --------------------------------------------------------------------------
