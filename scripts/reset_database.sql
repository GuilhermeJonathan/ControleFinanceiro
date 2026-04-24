-- ============================================================
-- reset_database.sql
-- Limpa todos os dados do banco, respeitando as FK constraints.
-- Mantém a estrutura (tabelas, índices, migrations intactos).
-- ============================================================

USE ControleFinanceiro;   -- << ajuste o nome do banco se necessário
GO

BEGIN TRANSACTION;

-- 1. Tabelas dependentes primeiro (FK para outras)
DELETE FROM Lancamentos;
DELETE FROM ParcelasCartao;
DELETE FROM SaldosContas;
DELETE FROM HorasTrabalhadas;

-- 2. Tabelas referenciadas por Lancamentos
DELETE FROM CartoesCredito;
DELETE FROM ReceitasRecorrentes;
DELETE FROM Categorias;

-- Reseta identity seeds (opcional – remove se quiser manter IDs sequenciais)
-- As PKs são GUID, então não há IDENTITY para resetar.

COMMIT TRANSACTION;

PRINT 'Base limpa com sucesso.';
GO
