-- ─────────────────────────────────────────────────────────────────────────────
-- SEED: Contas financeiras (bancária / investimento-custódia / internacional)
-- Usuário: a0b4e55f-ca8e-4e1c-9ae6-14c1a6c52290
--
-- IMPORTANTE
--  • Se uma execução anterior abortou, rode  ROLLBACK;  antes (erro 25P02).
--  • Reexecutar dá conflito de PK (proteção anti-duplicar). Para reinserir do zero,
--    descomente o bloco de LIMPEZA abaixo.
--  • Todas as contas ficam na PESSOA FÍSICA (EstruturaId nulo) → self-contained,
--    não depende de estruturas existirem para este usuário. Para ligar uma conta a
--    uma holding/offshore, preencha "EstruturaId" com o Id da estrutura do usuário.
--
-- Enums:
--   TipoContaFinanceira  1=Corrente  2=InvestimentoCustodia  3=Internacional  99=Outro
--   Moeda                1=BRL  2=USD  3=EUR  4=CHF  5=GBP
--
-- Regra de valor (na tela):
--   • Corrente/Internacional/Outro → usa o "Saldo" (convertido p/ BRL pelo câmbio do tenant).
--   • InvestimentoCustodia         → valor DERIVADO da soma dos investimentos ligados
--     (Investimentos.ContaId = Id da conta). O "Saldo" é ignorado nesse tipo.
-- ─────────────────────────────────────────────────────────────────────────────

BEGIN;

-- ── Schema idempotente (caso a migration ainda não tenha rodado em prod) ──────
CREATE TABLE IF NOT EXISTS "ContasFinanceiras" (
    "Id"            uuid NOT NULL,
    "UsuarioId"     uuid NOT NULL,
    "Nome"          character varying(200) NOT NULL,
    "Tipo"          integer NOT NULL,
    "Instituicao"   character varying(120),
    "Pais"          character varying(80),
    "Moeda"         integer NOT NULL,
    "Saldo"         numeric(18,2) NOT NULL,
    "Identificador" character varying(120),
    "EstruturaId"   uuid,
    "CriadoEm"      timestamp with time zone NOT NULL,
    "AtualizadoEm"  timestamp with time zone,
    CONSTRAINT "PK_ContasFinanceiras" PRIMARY KEY ("Id")
);
CREATE INDEX IF NOT EXISTS "IX_ContasFinanceiras_UsuarioId" ON "ContasFinanceiras" ("UsuarioId");
ALTER TABLE "Investimentos" ADD COLUMN IF NOT EXISTS "ContaId" uuid;

-- ── (opcional) LIMPEZA — descomente para reinserir do zero ────────────────────
-- UPDATE "Investimentos" SET "ContaId" = NULL WHERE "UsuarioId" = 'a0b4e55f-ca8e-4e1c-9ae6-14c1a6c52290';
-- DELETE FROM "ContasFinanceiras" WHERE "UsuarioId" = 'a0b4e55f-ca8e-4e1c-9ae6-14c1a6c52290';

-- ── Contas ────────────────────────────────────────────────────────────────────
INSERT INTO "ContasFinanceiras"
 ("Id","UsuarioId","Nome","Tipo","Instituicao","Pais","Moeda","Saldo","Identificador","EstruturaId","CriadoEm","AtualizadoEm") VALUES
 -- Nacionais (Brasil)
 ('c4000001-0000-0000-0000-000000000001','a0b4e55f-ca8e-4e1c-9ae6-14c1a6c52290','Conta Corrente Itaú',      1,'Itaú Unibanco','Brasil',1,  850000.00,'Ag 0001 / CC 12345-6', NULL, now(), NULL),
 ('c4000002-0000-0000-0000-000000000002','a0b4e55f-ca8e-4e1c-9ae6-14c1a6c52290','Conta Bradesco',           1,'Bradesco','Brasil',    1,  120000.00,'Ag 1234 / CC 98765-0', NULL, now(), NULL),
 ('c4000003-0000-0000-0000-000000000003','a0b4e55f-ca8e-4e1c-9ae6-14c1a6c52290','Custódia XP',              2,'XP Investimentos','Brasil',1,      0.00,'Conta 9988-7',         NULL, now(), NULL),
 -- Internacionais
 ('c4000004-0000-0000-0000-000000000004','a0b4e55f-ca8e-4e1c-9ae6-14c1a6c52290','UBS (Suíça)',              3,'UBS','Suíça',          4,  500000.00,'CH93 0076 2011 6238', NULL, now(), NULL),
 ('c4000005-0000-0000-0000-000000000005','a0b4e55f-ca8e-4e1c-9ae6-14c1a6c52290','Swissquote',               2,'Swissquote','Suíça',   2,      0.00,'SQ-556677',           NULL, now(), NULL),
 ('c4000006-0000-0000-0000-000000000006','a0b4e55f-ca8e-4e1c-9ae6-14c1a6c52290','J.P. Morgan Private',      3,'J.P. Morgan','EUA',     2, 1200000.00,'JPM-33221',           NULL, now(), NULL);

-- ── (opcional) Ligar investimentos existentes deste usuário a uma conta de custódia ──
--   Descomente e ajuste os tickers/ids para a custódia mostrar valor derivado:
-- UPDATE "Investimentos" SET "ContaId" = 'c4000003-0000-0000-0000-000000000003'
--   WHERE "UsuarioId" = 'a0b4e55f-ca8e-4e1c-9ae6-14c1a6c52290' AND "Moeda" = 1;   -- ex.: carteira BR → Custódia XP
-- UPDATE "Investimentos" SET "ContaId" = 'c4000005-0000-0000-0000-000000000005'
--   WHERE "UsuarioId" = 'a0b4e55f-ca8e-4e1c-9ae6-14c1a6c52290' AND "Moeda" <> 1;  -- ex.: exterior → Swissquote

COMMIT;
