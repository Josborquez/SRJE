-- ============================================================================
-- SRJE - Sistema de Retenciones Judiciales de Empleados
-- Script 04: Datos de Catalogo (Bancos + Tipos de Retencion)
-- Base de Datos: Oracle 19c
-- Ejecutar despues de: 01, 02, 03
-- ============================================================================

-- --------------------------------------------------------------------------
-- BANCOS — Catalogo de bancos chilenos
-- Fuente: Documento SRJE v3 + archivo TEMGE real (banco 504)
-- --------------------------------------------------------------------------
INSERT INTO BANCOS (COD_BANCO, NOMBRE_BANCO, USA_CTA_OT_BANCO, ACTIVO) VALUES (1, 'Banco de Chile', 'S', 'S');
INSERT INTO BANCOS (COD_BANCO, NOMBRE_BANCO, USA_CTA_OT_BANCO, ACTIVO) VALUES (9, 'Banco Internacional', 'S', 'S');
INSERT INTO BANCOS (COD_BANCO, NOMBRE_BANCO, USA_CTA_OT_BANCO, ACTIVO) VALUES (12, 'Banco Estado', 'N', 'S');
INSERT INTO BANCOS (COD_BANCO, NOMBRE_BANCO, USA_CTA_OT_BANCO, ACTIVO) VALUES (14, 'Scotiabank', 'S', 'S');
INSERT INTO BANCOS (COD_BANCO, NOMBRE_BANCO, USA_CTA_OT_BANCO, ACTIVO) VALUES (16, 'Banco de Credito e Inversiones (BCI)', 'S', 'S');
INSERT INTO BANCOS (COD_BANCO, NOMBRE_BANCO, USA_CTA_OT_BANCO, ACTIVO) VALUES (27, 'Corpbanca (Itau)', 'S', 'S');
INSERT INTO BANCOS (COD_BANCO, NOMBRE_BANCO, USA_CTA_OT_BANCO, ACTIVO) VALUES (28, 'Banco BICE', 'S', 'S');
INSERT INTO BANCOS (COD_BANCO, NOMBRE_BANCO, USA_CTA_OT_BANCO, ACTIVO) VALUES (37, 'Santander', 'S', 'S');
INSERT INTO BANCOS (COD_BANCO, NOMBRE_BANCO, USA_CTA_OT_BANCO, ACTIVO) VALUES (49, 'Itau', 'S', 'S');
INSERT INTO BANCOS (COD_BANCO, NOMBRE_BANCO, USA_CTA_OT_BANCO, ACTIVO) VALUES (51, 'Falabella', 'S', 'S');
INSERT INTO BANCOS (COD_BANCO, NOMBRE_BANCO, USA_CTA_OT_BANCO, ACTIVO) VALUES (55, 'Ripley', 'S', 'S');
INSERT INTO BANCOS (COD_BANCO, NOMBRE_BANCO, USA_CTA_OT_BANCO, ACTIVO) VALUES (60, 'Banco Security', 'S', 'S');
INSERT INTO BANCOS (COD_BANCO, NOMBRE_BANCO, USA_CTA_OT_BANCO, ACTIVO) VALUES (504, 'Banco Consorcio', 'S', 'S');

-- --------------------------------------------------------------------------
-- TIPOS_RETENCION — Codigos de retencion judicial
-- Fuente: Documento SRJE v3 + archivo ENVIO_REMUNERACIONES real
-- --------------------------------------------------------------------------
INSERT INTO TIPOS_RETENCION (COD_RETENCION, DESCRIPCION, MONEDA) VALUES ('DURETENF', 'Retencion en Pesos ($)', 'CLP');
INSERT INTO TIPOS_RETENCION (COD_RETENCION, DESCRIPCION, MONEDA) VALUES ('DURETENM', 'Retencion en Moneda', 'USD');
INSERT INTO TIPOS_RETENCION (COD_RETENCION, DESCRIPCION, MONEDA) VALUES ('DURETENUF', 'Retencion en UF', 'UF');
INSERT INTO TIPOS_RETENCION (COD_RETENCION, DESCRIPCION, MONEDA) VALUES ('DURETENUTM', 'Retencion en UTM', 'UTM');
INSERT INTO TIPOS_RETENCION (COD_RETENCION, DESCRIPCION, MONEDA) VALUES ('DURETENN', 'Retencion Normal', 'CLP');
INSERT INTO TIPOS_RETENCION (COD_RETENCION, DESCRIPCION, MONEDA) VALUES ('DURETENR', 'Retencion Reajustable', 'CLP');
INSERT INTO TIPOS_RETENCION (COD_RETENCION, DESCRIPCION, MONEDA) VALUES ('DURETENV', 'Retencion Variable', 'CLP');
INSERT INTO TIPOS_RETENCION (COD_RETENCION, DESCRIPCION, MONEDA) VALUES ('DURETENS2', 'Retencion Sueldo Escala 2', 'CLP');
INSERT INTO TIPOS_RETENCION (COD_RETENCION, DESCRIPCION, MONEDA) VALUES ('DURETENS3', 'Retencion Sueldo Escala 3', 'CLP');
INSERT INTO TIPOS_RETENCION (COD_RETENCION, DESCRIPCION, MONEDA) VALUES ('DURETENS4', 'Retencion Sueldo Escala 4', 'CLP');
INSERT INTO TIPOS_RETENCION (COD_RETENCION, DESCRIPCION, MONEDA) VALUES ('DURETENS9', 'Retencion Sueldo Escala 9', 'CLP');
INSERT INTO TIPOS_RETENCION (COD_RETENCION, DESCRIPCION, MONEDA) VALUES ('DURETENS10', 'Retencion Sueldo Escala 10', 'CLP');
INSERT INTO TIPOS_RETENCION (COD_RETENCION, DESCRIPCION, MONEDA) VALUES ('DURETENS11', 'Retencion Sueldo Escala 11', 'CLP');

COMMIT;
