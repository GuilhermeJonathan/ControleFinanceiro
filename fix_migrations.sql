-- ============================================================
-- Fix migrations – executar no SSMS contra o banco ControleFinanceiro
-- ============================================================

-- 1) Remove o registro fantasma que ficou gravado sem criar as colunas
DELETE FROM [__EFMigrationsHistory]
WHERE MigrationId = '20260424174053_AddTipoReceitaRecorrente';

-- 2) Adiciona as colunas que faltam em ReceitasRecorrentes
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE Name = 'Tipo' AND Object_ID = OBJECT_ID('ReceitasRecorrentes')
)
    ALTER TABLE ReceitasRecorrentes ADD Tipo int NOT NULL DEFAULT 1;

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE Name = 'ValorHora' AND Object_ID = OBJECT_ID('ReceitasRecorrentes')
)
    ALTER TABLE ReceitasRecorrentes ADD ValorHora decimal(18,2) NULL;

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE Name = 'QuantidadeHoras' AND Object_ID = OBJECT_ID('ReceitasRecorrentes')
)
    ALTER TABLE ReceitasRecorrentes ADD QuantidadeHoras decimal(18,2) NULL;

-- 3) Garante que DiaVencimento existe em CartoesCredito
--    (foi aplicado por migration que depois foi removida dos arquivos)
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE Name = 'DiaVencimento' AND Object_ID = OBJECT_ID('CartoesCredito')
)
    ALTER TABLE CartoesCredito ADD DiaVencimento int NULL;

-- 4) Marca a migration pendente como aplicada
IF NOT EXISTS (
    SELECT 1 FROM [__EFMigrationsHistory]
    WHERE MigrationId = '20260424174125_AddCartaoVencimentoETipoReceita'
)
    INSERT INTO [__EFMigrationsHistory] (MigrationId, ProductVersion)
    VALUES ('20260424174125_AddCartaoVencimentoETipoReceita', '10.0.7');

-- Verificação final
SELECT MigrationId FROM [__EFMigrationsHistory] ORDER BY MigrationId;
SELECT
    COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME IN ('ReceitasRecorrentes', 'CartoesCredito')
ORDER BY TABLE_NAME, COLUMN_NAME;
