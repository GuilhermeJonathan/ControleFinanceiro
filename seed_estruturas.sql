-- ─────────────────────────────────────────────────────────────────────────────
-- SEED: Estruturas (family office) para demonstração
-- Usuário: 1b0707c5-a9bc-45d2-85c1-04d67e4f79df
--
-- IMPORTANTE: se uma execução anterior falhou, sua sessão pode estar numa
-- transação abortada (erro 25P02). Rode  ROLLBACK;  antes de rodar este script.
--
-- Usa UUIDs fixos em tudo (sem gen_random_uuid → sem depender de pgcrypto).
-- Reexecução: dá conflito de PK (guarda contra duplicar). Para reinserir do zero,
-- use o bloco de limpeza comentado abaixo.
--
-- Enums:  TipoEstrutura 1=Trust 2=HoldingPatrimonial 3=HoldingParticipacoes 4=Offshore 5=EmpresaOperacional 6=PPLI 99=Outro
--         TipoRelacao   1=PropriedadeDireta 2=BeneficioTrust
--         TipoAtivo     1=Imovel 2=Veiculo 3=Embarcacao 4=Aeronave 5=Participacao 6=Investimento 99=Outro
--         TipoInvest    1=Acoes 2=FII 3=ETF 4=RendaFixa 5=Multimercado 6=Cripto 7=Exterior 99=Outro
--         Moeda         1=BRL 2=USD 3=EUR 4=CHF 5=GBP
-- ─────────────────────────────────────────────────────────────────────────────

BEGIN;

-- ── Schema (idempotente) — cria tabelas/colunas caso a migration ainda não
--    tenha rodado em prod. Igual à migration AddEstruturas; se já existir, no-op.
CREATE TABLE IF NOT EXISTS "Estruturas" (
    "Id"            uuid NOT NULL,
    "UsuarioId"     uuid NOT NULL,
    "Nome"          character varying(200) NOT NULL,
    "Tipo"          integer NOT NULL,
    "Jurisdicao"    character varying(120),
    "ConstituidaEm" timestamp with time zone,
    "Observacoes"   character varying(2000),
    "CriadoEm"      timestamp with time zone NOT NULL,
    "AtualizadoEm"  timestamp with time zone,
    CONSTRAINT "PK_Estruturas" PRIMARY KEY ("Id")
);
CREATE INDEX IF NOT EXISTS "IX_Estruturas_UsuarioId" ON "Estruturas" ("UsuarioId");

CREATE TABLE IF NOT EXISTS "ParticipacoesEstrutura" (
    "Id"                     uuid NOT NULL,
    "UsuarioId"              uuid NOT NULL,
    "EstruturaPaiId"         uuid,
    "EstruturaFilhaId"       uuid NOT NULL,
    "PercentualParticipacao" numeric(9,4) NOT NULL,
    "TipoRelacao"            integer NOT NULL,
    "CriadoEm"               timestamp with time zone NOT NULL,
    CONSTRAINT "PK_ParticipacoesEstrutura" PRIMARY KEY ("Id")
);
CREATE INDEX IF NOT EXISTS "IX_ParticipacoesEstrutura_UsuarioId" ON "ParticipacoesEstrutura" ("UsuarioId");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_ParticipacoesEstrutura_UsuarioId_EstruturaPaiId_EstruturaFilhaId"
    ON "ParticipacoesEstrutura" ("UsuarioId", "EstruturaPaiId", "EstruturaFilhaId");

ALTER TABLE "AtivosPatrimoniais" ADD COLUMN IF NOT EXISTS "EstruturaId" uuid;
ALTER TABLE "Investimentos"      ADD COLUMN IF NOT EXISTS "EstruturaId" uuid;

-- ── (opcional) limpeza — descomente para reinserir do zero ───────────────────
-- UPDATE "AtivosPatrimoniais" SET "EstruturaId" = NULL WHERE "UsuarioId" = '1b0707c5-a9bc-45d2-85c1-04d67e4f79df';
-- UPDATE "Investimentos"      SET "EstruturaId" = NULL WHERE "UsuarioId" = '1b0707c5-a9bc-45d2-85c1-04d67e4f79df';
-- DELETE FROM "AtivosPatrimoniais" WHERE "Id"::text LIKE '2a0000%';
-- DELETE FROM "Investimentos"      WHERE "Id"::text LIKE '3a0000%';
-- DELETE FROM "ParticipacoesEstrutura" WHERE "UsuarioId" = '1b0707c5-a9bc-45d2-85c1-04d67e4f79df';
-- DELETE FROM "Estruturas"             WHERE "UsuarioId" = '1b0707c5-a9bc-45d2-85c1-04d67e4f79df';

-- ── Estruturas ───────────────────────────────────────────────────────────────
INSERT INTO "Estruturas" ("Id","UsuarioId","Nome","Tipo","Jurisdicao","ConstituidaEm","Observacoes","CriadoEm","AtualizadoEm") VALUES
 ('e0000001-0000-0000-0000-000000000001','1b0707c5-a9bc-45d2-85c1-04d67e4f79df','Trust Internacional',      1,'Zurique · Suíça',            '2022-03-01T00:00:00Z','Trust discricionário — topo da estrutura sucessória.', now(), NULL),
 ('e0000002-0000-0000-0000-000000000002','1b0707c5-a9bc-45d2-85c1-04d67e4f79df','Holding Patrimonial',       2,'Brasil · SP',                '2021-06-01T00:00:00Z','Detém o portfólio imobiliário da família.',            now(), NULL),
 ('e0000003-0000-0000-0000-000000000003','1b0707c5-a9bc-45d2-85c1-04d67e4f79df','Holding de Participações',  3,'Brasil · SP',                '2021-06-01T00:00:00Z','Participações nas empresas operacionais.',             now(), NULL),
 ('e0000004-0000-0000-0000-000000000004','1b0707c5-a9bc-45d2-85c1-04d67e4f79df','BVI Holding Co.',           4,'Ilhas Virgens Britânicas',   '2023-01-15T00:00:00Z','Holding offshore de investimentos.',                   now(), NULL),
 ('e0000005-0000-0000-0000-000000000005','1b0707c5-a9bc-45d2-85c1-04d67e4f79df','Cayman Investment Ltd.',    4,'Cayman',                     '2023-02-10T00:00:00Z','Veículo de investimento internacional.',               now(), NULL),
 ('e0000006-0000-0000-0000-000000000006','1b0707c5-a9bc-45d2-85c1-04d67e4f79df','Bahamas Asset Mgmt.',       4,'Bahamas',                    '2023-05-20T00:00:00Z','Gestão de ativos e PPLI.',                             now(), NULL);

-- ── Participações (grafo). EstruturaPaiId NULL = Família/cliente (raiz) ───────
INSERT INTO "ParticipacoesEstrutura" ("Id","UsuarioId","EstruturaPaiId","EstruturaFilhaId","PercentualParticipacao","TipoRelacao","CriadoEm") VALUES
 ('1a000001-0000-0000-0000-000000000001','1b0707c5-a9bc-45d2-85c1-04d67e4f79df', NULL,                                  'e0000001-0000-0000-0000-000000000001', 100.0000, 2, now()),
 ('1a000002-0000-0000-0000-000000000002','1b0707c5-a9bc-45d2-85c1-04d67e4f79df','e0000001-0000-0000-0000-000000000001','e0000002-0000-0000-0000-000000000002', 100.0000, 1, now()),
 ('1a000003-0000-0000-0000-000000000003','1b0707c5-a9bc-45d2-85c1-04d67e4f79df','e0000001-0000-0000-0000-000000000001','e0000003-0000-0000-0000-000000000003', 100.0000, 1, now()),
 ('1a000004-0000-0000-0000-000000000004','1b0707c5-a9bc-45d2-85c1-04d67e4f79df','e0000001-0000-0000-0000-000000000001','e0000004-0000-0000-0000-000000000004', 100.0000, 1, now()),
 ('1a000005-0000-0000-0000-000000000005','1b0707c5-a9bc-45d2-85c1-04d67e4f79df','e0000001-0000-0000-0000-000000000001','e0000005-0000-0000-0000-000000000005', 100.0000, 1, now()),
 ('1a000006-0000-0000-0000-000000000006','1b0707c5-a9bc-45d2-85c1-04d67e4f79df','e0000001-0000-0000-0000-000000000001','e0000006-0000-0000-0000-000000000006', 100.0000, 1, now());

-- ── Ativos (imóveis / participações / bens) ──────────────────────────────────
INSERT INTO "AtivosPatrimoniais"
 ("Id","UsuarioId","Nome","Tipo","Moeda","ValorAtual","ValorizacaoAnualPct","ReceitaMensal","DespesaMensal","CriadoEm","AtualizadoEm","EstruturaId") VALUES
 ('2a000001-0000-0000-0000-000000000001','1b0707c5-a9bc-45d2-85c1-04d67e4f79df','Edifício Corporate SP',   1,1, 12000000.00, 6.0, 90000.00, 12000.00, now(), NULL,'e0000002-0000-0000-0000-000000000002'),
 ('2a000002-0000-0000-0000-000000000002','1b0707c5-a9bc-45d2-85c1-04d67e4f79df','Fazenda Boa Vista',       1,1,  8500000.00, 4.0, 15000.00,  8000.00, now(), NULL,'e0000002-0000-0000-0000-000000000002'),
 ('2a000003-0000-0000-0000-000000000003','1b0707c5-a9bc-45d2-85c1-04d67e4f79df','Galpão Logístico',        1,1,  6000000.00, 5.0, 45000.00,  5000.00, now(), NULL,'e0000002-0000-0000-0000-000000000002'),
 ('2a000004-0000-0000-0000-000000000004','1b0707c5-a9bc-45d2-85c1-04d67e4f79df','Cobertura Jardins',       1,1,  5000000.00, 5.5,     0.00,  3000.00, now(), NULL,'e0000002-0000-0000-0000-000000000002'),
 ('2a000005-0000-0000-0000-000000000005','1b0707c5-a9bc-45d2-85c1-04d67e4f79df','Participação Empresa Op. A',5,1, 9500000.00, 8.0, 60000.00, 0.00, now(), NULL,'e0000003-0000-0000-0000-000000000003'),
 ('2a000006-0000-0000-0000-000000000006','1b0707c5-a9bc-45d2-85c1-04d67e4f79df','Participação Empresa Op. B',5,1, 9500000.00, 7.0, 55000.00, 0.00, now(), NULL,'e0000003-0000-0000-0000-000000000003'),
 ('2a000007-0000-0000-0000-000000000007','1b0707c5-a9bc-45d2-85c1-04d67e4f79df','Imóvel Miami',            1,2,  2000000.00, 4.0, 8000.00, 1500.00, now(), NULL,'e0000006-0000-0000-0000-000000000006'),
 ('2a000008-0000-0000-0000-000000000008','1b0707c5-a9bc-45d2-85c1-04d67e4f79df','Private Placement Life Insurance (PPLI)', 99,2, 3000000.00, 5.0, 0.00, 0.00, now(), NULL,'e0000006-0000-0000-0000-000000000006'),
 ('2a000009-0000-0000-0000-000000000009','1b0707c5-a9bc-45d2-85c1-04d67e4f79df','Residência da Família',   1,1,  4500000.00, 5.0,     0.00, 2500.00, now(), NULL, NULL),
 ('2a00000a-0000-0000-0000-00000000000a','1b0707c5-a9bc-45d2-85c1-04d67e4f79df','Veículo (blindado)',      2,1,   350000.00,-15.0,    0.00,  800.00, now(), NULL, NULL);

-- ── Investimentos (carteiras / portfólios) ───────────────────────────────────
INSERT INTO "Investimentos"
 ("Id","UsuarioId","Nome","Tipo","Moeda","Corretora","Ticker","ValorAplicado","ValorAtual","RentabilidadeAnualPct","CriadoEm","AtualizadoEm","ValorAtualizadoEm","Quantidade","EstruturaId") VALUES
 ('3a000001-0000-0000-0000-000000000001','1b0707c5-a9bc-45d2-85c1-04d67e4f79df','Custódia Julius Baer', 7,2,'Julius Baer',NULL,1000000.00,1250000.00, 9.0, now(), NULL, NULL, NULL,'e0000005-0000-0000-0000-000000000005'),
 ('3a000002-0000-0000-0000-000000000002','1b0707c5-a9bc-45d2-85c1-04d67e4f79df','Conta Cash UBS',       4,4,'UBS',        NULL, 700000.00, 750000.00, 3.5, now(), NULL, NULL, NULL,'e0000005-0000-0000-0000-000000000005'),
 ('3a000003-0000-0000-0000-000000000003','1b0707c5-a9bc-45d2-85c1-04d67e4f79df','Global Bonds Fund',    7,2,'Pershing',   NULL,4000000.00,4100000.00, 5.0, now(), NULL, NULL, NULL,'e0000004-0000-0000-0000-000000000004'),
 ('3a000004-0000-0000-0000-000000000004','1b0707c5-a9bc-45d2-85c1-04d67e4f79df','Carteira de Ações BR', 1,1,'XP',        NULL, 500000.00, 620000.00,12.0, now(), NULL, NULL, NULL, NULL),
 ('3a000005-0000-0000-0000-000000000005','1b0707c5-a9bc-45d2-85c1-04d67e4f79df','Tesouro Direto',       4,1,'XP',        NULL, 300000.00, 330000.00,10.5, now(), NULL, NULL, NULL, NULL);

COMMIT;
