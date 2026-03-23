-- ============================================================================
-- 10: Agregar campos bancarios a RETENIDO_JUDICIAL
-- Permite asociar cuenta bancaria a cada retencion individual,
-- soportando beneficiarios con retenciones pagadas a cuentas distintas.
-- Los campos son nullable: si son NULL, se usa la cuenta del beneficiario.
-- ============================================================================

ALTER TABLE RETENIDO_JUDICIAL ADD (
    COD_BANCO      NUMBER(18,0)    NULL,
    TIPO_CUENTA    NUMBER(18,0)    NULL,
    CTA_ESTADO     NVARCHAR2(15)   NULL,
    CTA_OT_BANCO   NVARCHAR2(15)   NULL
);

COMMENT ON COLUMN RETENIDO_JUDICIAL.COD_BANCO IS 'Codigo banco para pago de esta retencion. NULL = usar cuenta del beneficiario';
COMMENT ON COLUMN RETENIDO_JUDICIAL.TIPO_CUENTA IS 'Tipo cuenta para pago de esta retencion. NULL = usar cuenta del beneficiario';
COMMENT ON COLUMN RETENIDO_JUDICIAL.CTA_ESTADO IS 'Cuenta Banco Estado para esta retencion. NULL = usar cuenta del beneficiario';
COMMENT ON COLUMN RETENIDO_JUDICIAL.CTA_OT_BANCO IS 'Cuenta otro banco para esta retencion. NULL = usar cuenta del beneficiario';

-- Backfill: copiar datos bancarios del beneficiario a retenciones existentes
UPDATE RETENIDO_JUDICIAL r
SET (COD_BANCO, TIPO_CUENTA, CTA_ESTADO, CTA_OT_BANCO) = (
    SELECT b.COD_BANCO, b.TIPO_CUENTA, b.CTA_ESTADO, b.CTA_OT_BANCO
    FROM BENEFICIARIOS b
    WHERE b.RUT_BENEFICIARIO = r.RUT_BENEFICIARIO
)
WHERE r.COD_BANCO IS NULL
  AND EXISTS (
    SELECT 1 FROM BENEFICIARIOS b
    WHERE b.RUT_BENEFICIARIO = r.RUT_BENEFICIARIO
);

COMMIT;
