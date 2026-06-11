-- ============================================================
-- 17_tabla_usuarios_sistema.sql
-- Tabla USUARIOS_SISTEMA: autenticacion local con BCrypt
-- (reemplaza usuarios hardcodeados de DevAuthService)
--
-- IMPORTANTE:
--  - Los PASSWORD_HASH son BCrypt; NUNCA guardar contraseñas en claro
--    ni hashes reales en este repositorio.
--  - El usuario admin inicial se crea automaticamente al arrancar la
--    aplicacion con Auth:Provider=Database y la variable de entorno
--    SRJE_ADMIN_PASSWORD definida (solo si no existe).
--  - Para insertar usuarios manualmente, generar el hash en C# con:
--      BCrypt.Net.BCrypt.HashPassword("la_contraseña")
-- ============================================================

SET SERVEROUTPUT ON;

DECLARE
    v_exists NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_exists
    FROM user_tables WHERE table_name = 'USUARIOS_SISTEMA';

    IF v_exists = 0 THEN
        EXECUTE IMMEDIATE '
            CREATE TABLE USUARIOS_SISTEMA (
                USUARIO         VARCHAR2(50)   NOT NULL,
                PASSWORD_HASH   VARCHAR2(128)  NOT NULL,
                NOMBRE_COMPLETO VARCHAR2(100)  NOT NULL,
                ROL             VARCHAR2(20)   NOT NULL,
                ESTADO          VARCHAR2(1)    DEFAULT ''A'' NOT NULL,
                FECHA_CREACION  DATE           DEFAULT SYSDATE,
                CONSTRAINT PK_USUARIOS_SISTEMA PRIMARY KEY (USUARIO),
                CONSTRAINT CK_USUARIOS_SISTEMA_ROL CHECK (ROL IN (''admin'', ''operador'', ''consulta'')),
                CONSTRAINT CK_USUARIOS_SISTEMA_ESTADO CHECK (ESTADO IN (''A'', ''I'')),
                CONSTRAINT CK_USUARIOS_SISTEMA_LOWER CHECK (USUARIO = LOWER(USUARIO))
            )';
        DBMS_OUTPUT.PUT_LINE('Tabla USUARIOS_SISTEMA creada.');
    ELSE
        DBMS_OUTPUT.PUT_LINE('Tabla USUARIOS_SISTEMA ya existe. Sin cambios.');
    END IF;
END;
/

-- Constraint de usuario en minusculas (idempotente, para tablas ya creadas
-- con una version anterior de este script)
DECLARE
    v_exists NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_exists
    FROM user_constraints
    WHERE constraint_name = 'CK_USUARIOS_SISTEMA_LOWER';

    IF v_exists = 0 THEN
        EXECUTE IMMEDIATE '
            ALTER TABLE USUARIOS_SISTEMA ADD CONSTRAINT CK_USUARIOS_SISTEMA_LOWER
            CHECK (USUARIO = LOWER(USUARIO))';
        DBMS_OUTPUT.PUT_LINE('Constraint CK_USUARIOS_SISTEMA_LOWER agregada.');
    ELSE
        DBMS_OUTPUT.PUT_LINE('Constraint CK_USUARIOS_SISTEMA_LOWER ya existe. Sin cambios.');
    END IF;
END;
/

-- Ejemplo de insercion manual (reemplazar <HASH_BCRYPT> por hash real,
-- generado fuera del repo):
--
-- INSERT INTO USUARIOS_SISTEMA (USUARIO, PASSWORD_HASH, NOMBRE_COMPLETO, ROL)
-- VALUES ('operador1', '<HASH_BCRYPT>', 'Operador SRJE', 'operador');
-- COMMIT;
