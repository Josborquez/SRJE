-- ============================================================
-- 18_tabla_log_accesos.sql
-- Tabla LOG_ACCESOS: historial de login/logout de usuarios
-- (login exitoso, login fallido y logout, con IP y fecha)
-- ============================================================

SET SERVEROUTPUT ON;

DECLARE
    v_exists NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_exists
    FROM user_tables WHERE table_name = 'LOG_ACCESOS';

    IF v_exists = 0 THEN
        EXECUTE IMMEDIATE '
            CREATE TABLE LOG_ACCESOS (
                ID      NUMBER(18)    GENERATED ALWAYS AS IDENTITY,
                USUARIO VARCHAR2(50)  NOT NULL,
                EVENTO  VARCHAR2(20)  NOT NULL,
                IP      VARCHAR2(50),
                FECHA   DATE          DEFAULT SYSDATE NOT NULL,
                CONSTRAINT PK_LOG_ACCESOS PRIMARY KEY (ID),
                CONSTRAINT CK_LOG_ACCESOS_EVENTO CHECK (EVENTO IN (''login_ok'', ''login_fail'', ''logout''))
            )';
        EXECUTE IMMEDIATE '
            CREATE INDEX IX_LOG_ACCESOS_USUARIO_FECHA ON LOG_ACCESOS (USUARIO, FECHA)';
        DBMS_OUTPUT.PUT_LINE('Tabla LOG_ACCESOS creada.');
    ELSE
        DBMS_OUTPUT.PUT_LINE('Tabla LOG_ACCESOS ya existe. Sin cambios.');
    END IF;
END;
/
    