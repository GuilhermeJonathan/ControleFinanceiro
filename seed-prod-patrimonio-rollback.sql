-- =====================================================================
-- ROLLBACK DA MASSA DE VALIDAÇÃO — MÓDULO PATRIMÔNIO (PRODUÇÃO)
-- =====================================================================
-- Remove APENAS as linhas inseridas por seed-prod-patrimonio.sql (UUIDs fixos).
-- NÃO mexe em parâmetros de sistema (MoedasParam/TiposAtivoParam/TiposInvestimentoParam)
-- nem na tabela Users. Seguro e específico.
-- =====================================================================

BEGIN;

-- 1) Cenários da simulação (filhos) → depois a simulação
DELETE FROM "CenariosSimulacao"
 WHERE "SimulacaoId" = 'f2000000-0000-0000-0000-000000000001';

DELETE FROM "SimulacoesPatrimoniais"
 WHERE "Id" = 'f2000000-0000-0000-0000-000000000001';

-- 2) Investimentos (só se a tabela existir)
DO $$
BEGIN
  IF to_regclass('public."Investimentos"') IS NOT NULL THEN
    DELETE FROM "Investimentos" WHERE "Id" IN (
      'e2000000-0000-0000-0000-000000000001',
      'e2000000-0000-0000-0000-000000000002',
      'e2000000-0000-0000-0000-000000000003',
      'e2000000-0000-0000-0000-000000000004'
    );
  END IF;
END $$;

-- 3) Dívidas / passivos
DELETE FROM "PassivosPatrimoniais" WHERE "Id" IN (
  'd2000000-0000-0000-0000-000000000001',
  'd2000000-0000-0000-0000-000000000002'
);

-- 4) Bens / ativos
DELETE FROM "AtivosPatrimoniais" WHERE "Id" IN (
  'a2000000-0000-0000-0000-000000000001',
  'a2000000-0000-0000-0000-000000000002',
  'a2000000-0000-0000-0000-000000000003',
  'a2000000-0000-0000-0000-000000000004',
  'a2000000-0000-0000-0000-000000000005',
  'a2000000-0000-0000-0000-000000000006'
);

-- 5) Vínculo de assessoria
DELETE FROM "VinculosAssessoria"
 WHERE "Id" = 'c2000000-0000-0000-0000-000000000001';

COMMIT;

-- =====================================================================
-- Observações:
--  • Os parâmetros (moedas/tipos) NÃO são removidos de propósito — são
--    compartilhados e podem estar em uso por outros usuários.
--  • Se você descomentou o UPDATE de UserTypeId na seção 0 do seed, ele
--    NÃO é revertido aqui (reverta manualmente se necessário).
-- =====================================================================
