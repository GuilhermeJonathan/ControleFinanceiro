-- =====================================================================
-- MASSA DE VALIDAÇÃO — MÓDULO PATRIMÔNIO (PRODUÇÃO)
-- =====================================================================
-- Gera dados de patrimônio para o cliente e cria o vínculo assessor→cliente.
-- SEGURO: apenas INSERT ... ON CONFLICT DO NOTHING (idempotente, sem DELETE/UPDATE
-- destrutivo). UUIDs fixos com prefixos próprios — pode rodar mais de uma vez.
--
-- PRÉ-REQUISITOS:
--   1. As migrations do patrimônio JÁ aplicadas em produção:
--      AddAtivosPatrimoniais, AddPassivosECashflowAtivos, AddInvestimentos,
--      AddParametros, AddCotacaoMoedaParam, AddSimulacoesPatrimoniais.
--   2. Os dois usuários já existem (IDs abaixo).
--   3. Rodar no MESMO banco onde ficam Users + dados de domínio
--      (se Login e domínio estiverem em bancos separados, veja a nota no fim).
--
-- USUÁRIOS (já existentes):
--   Assessor: 1ce3a268-f081-40a0-88cd-aa1d4deb4091  (assessor@teste.com)
--   Cliente : 1b0707c5-a9bc-45d2-85c1-04d67e4f79df  (guilhermejonathan@hotmail.com)
-- =====================================================================

BEGIN;

-- ---------------------------------------------------------------------
-- 0) (OPCIONAL — REVISE ANTES) Garante que o assessor é UserType Assessor (3),
--    necessário para ele ver os menus de assessor. Descomente se precisar.
--    ⚠️ Altera a tabela Users em produção — só rode se souber que faz sentido.
-- ---------------------------------------------------------------------
-- UPDATE "Users" SET "UserTypeId" = 3
--  WHERE "Id" = '1ce3a268-f081-40a0-88cd-aa1d4deb4091' AND "UserTypeId" <> 3;

-- ---------------------------------------------------------------------
-- 1) Parâmetros (moedas com cotação + tipos). ON CONFLICT protege se já existirem.
-- ---------------------------------------------------------------------
INSERT INTO "MoedasParam" ("Id","Codigo","Nome","CotacaoBRL","Ordem","Ativo","IsSystem") VALUES
  (1,'BRL','Real Brasileiro', 1.00, 1,TRUE,TRUE),
  (2,'USD','Dólar Americano', 5.40, 2,TRUE,TRUE),
  (3,'EUR','Euro',            5.90, 3,TRUE,TRUE),
  (4,'CHF','Franco Suíço',    6.10, 4,TRUE,TRUE),
  (5,'GBP','Libra Esterlina', 6.90, 5,TRUE,TRUE)
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "TiposAtivoParam" ("Id","Nome","Ordem","Ativo","IsSystem") VALUES
  (1,'Imóvel',1,TRUE,TRUE),(2,'Veículo',2,TRUE,TRUE),(3,'Embarcação',3,TRUE,TRUE),
  (4,'Aeronave',4,TRUE,TRUE),(5,'Participação',5,TRUE,TRUE),(6,'Investimento',6,TRUE,TRUE),
  (99,'Outro',99,TRUE,TRUE)
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "TiposInvestimentoParam" ("Id","Nome","Ordem","Ativo","IsSystem") VALUES
  (1,'Ações',1,TRUE,TRUE),(2,'FII',2,TRUE,TRUE),(3,'ETF',3,TRUE,TRUE),
  (4,'Renda Fixa',4,TRUE,TRUE),(5,'Multimercado',5,TRUE,TRUE),(6,'Cripto',6,TRUE,TRUE),
  (7,'Exterior',7,TRUE,TRUE),(99,'Outro',99,TRUE,TRUE)
ON CONFLICT ("Id") DO NOTHING;

-- ---------------------------------------------------------------------
-- 2) Vínculo assessoria (assessor → cliente), já aceito e ativo.
-- ---------------------------------------------------------------------
INSERT INTO "VinculosAssessoria"
  ("Id","AssessorId","ClienteId","CodigoConvite","CriadoEm","AceitoEm","RevogadoEm","NomeCliente","NomeAssessor")
VALUES
  ('c2000000-0000-0000-0000-000000000001',
   '1ce3a268-f081-40a0-88cd-aa1d4deb4091',
   '1b0707c5-a9bc-45d2-85c1-04d67e4f79df',
   'PRODV1',
   now() - interval '15 days', now() - interval '14 days', NULL,
   'Guilherme Rodrigues Silva', 'Assessor')
ON CONFLICT ("Id") DO NOTHING;

-- ---------------------------------------------------------------------
-- 3) Bens (Ativos patrimoniais) do cliente — com fluxo de caixa por bem.
--    Tipo: 1=Imóvel 2=Veículo 5=Participação 6=Investimento
--    Moeda: 1=BRL 2=USD 4=CHF
-- ---------------------------------------------------------------------
INSERT INTO "AtivosPatrimoniais"
  ("Id","UsuarioId","Nome","Tipo","Moeda","ValorAtual","ValorizacaoAnualPct","ReceitaMensal","DespesaMensal","CriadoEm")
VALUES
  ('a2000000-0000-0000-0000-000000000001','1b0707c5-a9bc-45d2-85c1-04d67e4f79df','Apartamento Alto de Pinheiros', 1, 1, 2800000.00,  7.0,  9500.00, 1800.00, now()),
  ('a2000000-0000-0000-0000-000000000002','1b0707c5-a9bc-45d2-85c1-04d67e4f79df','Casa de praia - Guarujá',       1, 1, 1500000.00,  5.0,     0.00, 1200.00, now()),
  ('a2000000-0000-0000-0000-000000000003','1b0707c5-a9bc-45d2-85c1-04d67e4f79df','Land Rover Defender',           2, 1,  620000.00, -8.0,     0.00,  900.00, now()),
  ('a2000000-0000-0000-0000-000000000004','1b0707c5-a9bc-45d2-85c1-04d67e4f79df','Carteira EUA (ETFs)',           6, 2,  250000.00, 11.0,     0.00,    0.00, now()),
  ('a2000000-0000-0000-0000-000000000005','1b0707c5-a9bc-45d2-85c1-04d67e4f79df','Conta Suíça (UBS)',             6, 4,  180000.00,  3.0,     0.00,    0.00, now()),
  ('a2000000-0000-0000-0000-000000000006','1b0707c5-a9bc-45d2-85c1-04d67e4f79df','Participação Holding XYZ',      5, 1, 1200000.00, 15.0, 15000.00,    0.00, now())
ON CONFLICT ("Id") DO NOTHING;

-- ---------------------------------------------------------------------
-- 4) Dívidas (Passivos). Prazo: 1=Curto 2=Longo. Moeda: 1=BRL 3=EUR
--    O financiamento tem juros+prazo (alimenta a projeção); o Lombard é bullet.
-- ---------------------------------------------------------------------
INSERT INTO "PassivosPatrimoniais"
  ("Id","UsuarioId","Nome","Moeda","Valor","Prazo","TaxaJurosAnualPct","PrazoMeses","CriadoEm")
VALUES
  ('d2000000-0000-0000-0000-000000000001','1b0707c5-a9bc-45d2-85c1-04d67e4f79df','Financiamento Apartamento', 1, 900000.00, 2, 10.5, 240, now()),
  ('d2000000-0000-0000-0000-000000000002','1b0707c5-a9bc-45d2-85c1-04d67e4f79df','Lombard loan - Swissquote',  3, 150000.00, 1, NULL, NULL, now())
ON CONFLICT ("Id") DO NOTHING;

-- ---------------------------------------------------------------------
-- 5) Investimentos (módulo separado). Tipo: 1=Ações 3=ETF 4=RendaFixa 6=Cripto
-- ---------------------------------------------------------------------
INSERT INTO "Investimentos"
  ("Id","UsuarioId","Nome","Tipo","Moeda","Corretora","Ticker","ValorAplicado","ValorAtual","RentabilidadeAnualPct","CriadoEm")
VALUES
  ('e2000000-0000-0000-0000-000000000001','1b0707c5-a9bc-45d2-85c1-04d67e4f79df','ITSA4',        1, 1,'XP Investimentos','ITSA4',   80000.00,  96000.00, 15.0, now()),
  ('e2000000-0000-0000-0000-000000000002','1b0707c5-a9bc-45d2-85c1-04d67e4f79df','VOO',          3, 2,'Interactive Brokers','VOO', 120000.00, 152000.00, 20.0, now()),
  ('e2000000-0000-0000-0000-000000000003','1b0707c5-a9bc-45d2-85c1-04d67e4f79df','Bitcoin',      6, 2,'Binance','BTC',              30000.00,  64000.00, 70.0, now()),
  ('e2000000-0000-0000-0000-000000000004','1b0707c5-a9bc-45d2-85c1-04d67e4f79df','LCI Bradesco', 4, 1,'Bradesco',NULL,             150000.00, 168000.00, 11.0, now())
ON CONFLICT ("Id") DO NOTHING;

-- ---------------------------------------------------------------------
-- 6) Simulação de projeção salva (proteção patrimonial) + 1 cenário.
-- ---------------------------------------------------------------------
INSERT INTO "SimulacoesPatrimoniais"
  ("Id","UsuarioId","Nome","Favorita","IdadeAtual","IdadeAlvo","PatrimonioInicial","ModoAutomatico","AporteMensal","TaxaRetornoRealAnualPct","RetiradaMensal","CriadoEm")
VALUES
  ('f2000000-0000-0000-0000-000000000001','1b0707c5-a9bc-45d2-85c1-04d67e4f79df','Independência aos 55', TRUE, 38, 55, 0.00, TRUE, 15000.00, 5.0, 40000.00, now())
ON CONFLICT ("Id") DO NOTHING;

-- Cenário: resgate extraordinário (compra de fazenda) aos 50 anos.
INSERT INTO "CenariosSimulacao" ("SimulacaoId","Nome","Tipo","Valor","IdadeInicio","IdadeFim")
SELECT 'f2000000-0000-0000-0000-000000000001','Compra de fazenda', 2, 2000000.00, 50, NULL
WHERE NOT EXISTS (
  SELECT 1 FROM "CenariosSimulacao"
  WHERE "SimulacaoId" = 'f2000000-0000-0000-0000-000000000001' AND "Nome" = 'Compra de fazenda'
);

COMMIT;

-- =====================================================================
-- Resultado esperado (câmbio estimado USD 5,40 / EUR 5,90 / CHF 6,10):
--   Bens    ≈ R$ 8.075.000  (2.800.000 + 1.500.000 + 620.000 + 250.000*5,40
--                            + 180.000*6,10 + 1.200.000)
--   Dívidas ≈ R$ 1.785.000  (900.000 + 150.000*5,90)
--   Patrimônio líquido ≈ R$ 6.290.000
--   Receita mensal 24.500 · Despesa 3.900 · Saldo 20.600
--
-- NOTA (bancos separados): se em produção a tabela Users fica num banco
-- diferente das tabelas de domínio (Ativos/Passivos/Vínculos), rode as
-- seções 1–6 no banco de DOMÍNIO. Os IDs de usuário continuam valendo.
-- =====================================================================
