-- ============================================================================
-- SRJE - Sistema de Retenciones Judiciales de Empleados
-- Script 01: Tablas Principales
-- Base de Datos: Oracle 19c
-- Ejecutar como: DBA o usuario propietario del esquema SRJE
-- ============================================================================

-- --------------------------------------------------------------------------
-- 1. BENEFICIARIOS — Ficha del beneficiario de retencion judicial
-- --------------------------------------------------------------------------
CREATE TABLE BENEFICIARIOS (
    ID                    NUMBER(18)          GENERATED ALWAYS AS IDENTITY,
    RUT_BENEFICIARIO      NUMBER(18,0)        NOT NULL,
    DV_BENEFICIARIO       NCHAR(1)            NOT NULL,
    NOMBRE_BENEFICIARIO   NVARCHAR2(39)       NOT NULL,   -- 39 chars para archivo TEMGE
    FECHA_NACIMIENTO      DATE                NULL,
    SEXO                  NCHAR(1)            NULL,        -- M=Masculino, F=Femenino
    ESTADO_CIVIL          NVARCHAR2(20)       NULL,        -- Soltero/Casado/Viudo/Divorciado
    DOMICILIO             NVARCHAR2(100)      NULL,
    COMUNA                NVARCHAR2(50)       NULL,
    TELEFONO              NVARCHAR2(20)       NULL,
    CTA_OT_BANCO          NVARCHAR2(15)       NULL,        -- Cuenta en otro banco (max 15 chars)
    TIPO_CUENTA           NUMBER(18,0)        NULL,        -- 01=Cta.Cte, 02=Ahorro, 03=Vista
    COD_BANCO             NUMBER(18,0)        NULL,        -- 012=BancoEstado
    CTA_ESTADO            NCHAR(11)           NULL,        -- Cuenta BancoEstado (11 chars)
    SUCURSAL              NVARCHAR2(60)       NULL,
    ESTADO                NCHAR(1)            DEFAULT 'A', -- A=Activo, I=Inactivo, S=Suspendido
    FECHA_CREACION        DATE                DEFAULT SYSDATE,
    FECHA_MODIFICACION    DATE                NULL,
    USUARIO_CREACION      NVARCHAR2(50)       NULL,
    CONSTRAINT PK_BENEFICIARIOS PRIMARY KEY (ID),
    CONSTRAINT UK_BENEFICIARIOS_RUT UNIQUE (RUT_BENEFICIARIO),
    CONSTRAINT CK_BENEFICIARIOS_SEXO CHECK (SEXO IN ('M', 'F')),
    CONSTRAINT CK_BENEFICIARIOS_ESTADO CHECK (ESTADO IN ('A', 'I', 'S')),
    CONSTRAINT CK_BENEFICIARIOS_TIPO_CTA CHECK (TIPO_CUENTA IN (1, 2, 3))
);

COMMENT ON TABLE BENEFICIARIOS IS 'Ficha del beneficiario de retencion judicial';
COMMENT ON COLUMN BENEFICIARIOS.NOMBRE_BENEFICIARIO IS 'Nombre completo (39 chars para archivo TEMGE)';
COMMENT ON COLUMN BENEFICIARIOS.CTA_OT_BANCO IS 'Numero de cuenta en otro banco (15 chars max)';
COMMENT ON COLUMN BENEFICIARIOS.CTA_ESTADO IS 'Numero cuenta BancoEstado (11 chars, LPAD con ceros)';

-- --------------------------------------------------------------------------
-- 2. FUNCIONARIOS — Funcionarios publicos afectados por retenciones
-- --------------------------------------------------------------------------
CREATE TABLE FUNCIONARIOS (
    ID                    NUMBER(18)          GENERATED ALWAYS AS IDENTITY,
    RUT_FUNCIONARIO       NUMBER(18,0)        NOT NULL,
    DV_FUNCIONARIO        NCHAR(1)            NOT NULL,
    APELLIDO_PATERNO      NVARCHAR2(20)       NULL,        -- 20 chars (archivo remuneraciones)
    APELLIDO_MATERNO      NVARCHAR2(20)       NULL,        -- 20 chars
    NOMBRES               NVARCHAR2(30)       NULL,        -- 30 chars
    ID_SISTEMA            NVARCHAR2(8)        NULL,        -- ID numerico en sistema RRHH
    ACTIVO                NCHAR(1)            DEFAULT 'S', -- S=Activo, N=Inactivo
    CONSTRAINT PK_FUNCIONARIOS PRIMARY KEY (ID),
    CONSTRAINT UK_FUNCIONARIOS_RUT UNIQUE (RUT_FUNCIONARIO),
    CONSTRAINT CK_FUNCIONARIOS_ACTIVO CHECK (ACTIVO IN ('S', 'N'))
);

COMMENT ON TABLE FUNCIONARIOS IS 'Funcionarios publicos con retenciones judiciales';

-- --------------------------------------------------------------------------
-- 3. RETENIDO_JUDICIAL — Retenciones vigentes (funcionario -> beneficiario)
-- --------------------------------------------------------------------------
CREATE TABLE RETENIDO_JUDICIAL (
    ID                    NUMBER(18)          GENERATED ALWAYS AS IDENTITY,
    ID_RETENCION          NUMBER(18,0)        NOT NULL,    -- ID externo sistema remuneraciones
    RUT_TITULAR           NUMBER(18,0)        NOT NULL,    -- RUT funcionario retenido
    DV_TITULAR            NCHAR(1)            NOT NULL,
    RUT_BENEFICIARIO      NUMBER(18,0)        NOT NULL,    -- RUT beneficiario
    DV_BENEFICIARIO       NCHAR(1)            NOT NULL,
    MONTO                 NUMBER(18,2)        NOT NULL,    -- Monto en pesos CLP
    COD_RETENCION         NVARCHAR2(11)       NULL,        -- Codigo tipo retencion (ej: DURETENF)
    TIPO_PAGO             NVARCHAR2(17)       NULL,        -- COMPENSACION o PERMANENTE SR
    ESTADO                NCHAR(1)            DEFAULT 'A', -- A=Activo, I=Inactivo
    FECHA_VIGENCIA        DATE                NULL,
    PERIODO_PROCESO       NCHAR(6)            NULL,        -- Periodo AAAAMM
    CONSTRAINT PK_RETENIDO_JUDICIAL PRIMARY KEY (ID),
    CONSTRAINT CK_RETENIDO_ESTADO CHECK (ESTADO IN ('A', 'I'))
);

COMMENT ON TABLE RETENIDO_JUDICIAL IS 'Retenciones judiciales vigentes';
COMMENT ON COLUMN RETENIDO_JUDICIAL.PERIODO_PROCESO IS 'Periodo AAAAMM del proceso de remuneraciones';

-- --------------------------------------------------------------------------
-- 4. HISTORIAL_PAGOS_TEMGE — Registro maestro de archivos TEMGE generados
-- --------------------------------------------------------------------------
CREATE TABLE HISTORIAL_PAGOS_TEMGE (
    ID                    NUMBER(18)          GENERATED ALWAYS AS IDENTITY,
    FECHA_PROCESO         DATE                NOT NULL,
    HORA_PROCESO          NCHAR(6)            NULL,        -- HHMMSS
    COD_EMPRESA           NVARCHAR2(21)       NULL,        -- 06110104519640100572
    MONTO_TOTAL           NUMBER(18,2)        NULL,
    CANTIDAD_REGISTROS    NUMBER(10,0)        NULL,
    NOMBRE_ARCHIVO        NVARCHAR2(200)      NULL,
    ESTADO                NCHAR(1)            DEFAULT 'G', -- G=Generado, E=Enviado, C=Conciliado
    USUARIO_GENERA        NVARCHAR2(50)       NULL,
    CONSTRAINT PK_HISTORIAL_PAGOS_TEMGE PRIMARY KEY (ID),
    CONSTRAINT CK_HISTORIAL_ESTADO CHECK (ESTADO IN ('G', 'E', 'C'))
);

COMMENT ON TABLE HISTORIAL_PAGOS_TEMGE IS 'Registro maestro de archivos TEMGE generados';

-- --------------------------------------------------------------------------
-- 5. DETALLE_PAGO_TEMGE — Lineas individuales del archivo TEMGE
-- --------------------------------------------------------------------------
CREATE TABLE DETALLE_PAGO_TEMGE (
    ID                    NUMBER(18)          GENERATED ALWAYS AS IDENTITY,
    ID_HISTORIAL          NUMBER(18,0)        NOT NULL,
    ID_RETENIDO_JUDICIAL  NUMBER(18,0)        NULL,
    RUT_BENEFICIARIO      NUMBER(18,0)        NOT NULL,
    MONTO_PAGADO          NUMBER(18,2)        NULL,
    COD_BANCO             NUMBER(18,0)        NULL,
    TIPO_CUENTA           NUMBER(18,0)        NULL,
    ESTADO_LINEA          NCHAR(1)            DEFAULT 'P', -- P=Procesado, E=Excluido
    MOTIVO_EXCLUSION      NVARCHAR2(200)      NULL,
    CONSTRAINT PK_DETALLE_PAGO_TEMGE PRIMARY KEY (ID),
    CONSTRAINT FK_DETALLE_HISTORIAL FOREIGN KEY (ID_HISTORIAL) REFERENCES HISTORIAL_PAGOS_TEMGE(ID),
    CONSTRAINT CK_DETALLE_ESTADO CHECK (ESTADO_LINEA IN ('P', 'E'))
);

COMMENT ON TABLE DETALLE_PAGO_TEMGE IS 'Detalle linea por linea del archivo TEMGE generado';

-- --------------------------------------------------------------------------
-- 6. BANCOS — Catalogo de bancos (extensible)
-- --------------------------------------------------------------------------
CREATE TABLE BANCOS (
    COD_BANCO             NUMBER(18,0)        NOT NULL,
    NOMBRE_BANCO          NVARCHAR2(100)      NOT NULL,
    USA_CTA_OT_BANCO      NCHAR(1)            DEFAULT 'S', -- S=Usa CTA_OT_BANCO, N=Usa CTA_ESTADO
    ACTIVO                NCHAR(1)            DEFAULT 'S',
    CONSTRAINT PK_BANCOS PRIMARY KEY (COD_BANCO)
);

COMMENT ON TABLE BANCOS IS 'Catalogo de bancos para generacion TEMGE';

-- --------------------------------------------------------------------------
-- 7. TIPOS_RETENCION — Catalogo de codigos de retencion
-- --------------------------------------------------------------------------
CREATE TABLE TIPOS_RETENCION (
    COD_RETENCION         NVARCHAR2(11)       NOT NULL,
    DESCRIPCION           NVARCHAR2(100)      NOT NULL,
    MONEDA                NVARCHAR2(10)       DEFAULT 'CLP',
    ACTIVO                NCHAR(1)            DEFAULT 'S',
    CONSTRAINT PK_TIPOS_RETENCION PRIMARY KEY (COD_RETENCION)
);

COMMENT ON TABLE TIPOS_RETENCION IS 'Catalogo de codigos de retencion judicial';
