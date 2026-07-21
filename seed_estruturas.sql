-- ─────────────────────────────────────────────────────────────────────────────
-- SEED: Estruturas (family office) para demonstração
-- Usuário: a0b4e55f-ca8e-4e1c-9ae6-14c1a6c52290
--
-- Cria: Trust (Suíça) → Holdings BR + Offshore (BVI/Cayman/Bahamas),
--        imóveis na Holding Patrimonial, participações na Holding de Participações,
--        portfólios nas offshore, + alguns bens/investimentos em pessoa física.
--
-- Os VALORES das estruturas são DERIVADOS (não existe coluna de valor): vêm dos
-- ativos/investimentos abaixo com EstruturaId apontando para elas.
--
-- Enums:  TipoEstrutura 1=Trust 2=HoldingPatrimonial 3=HoldingParticipacoes 4=Offshore 5=EmpresaOperacional 6=PPLI 99=Outro
--         TipoRelacao   1=PropriedadeDireta 2=BeneficioTrust
--         TipoAtivo     1=Imovel 2=Veiculo 3=Embarcacao 4=Aeronave 5=Participacao 6=Investimento 99=Outro
--         TipoInvest    1=Acoes 2=FII 3=ETF 4=RendaFixa 5=Multimercado 6=Cripto 7=Exterior 99=Outro
--         Moeda         1=BRL 2=USD 3=EUR 4=CHF 5=GBP
--
-- Rodar novamente: as estruturas têm UUID fixo (dá conflito de PK = guarda);
-- os ativos/investimentos usam gen_random_uuid() e SERIAM duplicados. Use o
-- bloco de limpeza abaixo se quiser reinserir do zero.
-- ─────────────────────────────────────────────────────────────────────────────

BEGIN;

-- ── (opcional) limpeza — descomente para reinserir do zero ───────────────────
-- DELETE FROM "AtivosPatrimoniais"     WHERE "UsuarioId" = 'a0b4e55f-ca8e-4e1c-9ae6-14c1a6c52290' AND "EstruturaId" IS NOT NULL;
-- UPDATE "AtivosPatrimoniais" SET "EstruturaId" = NULL WHERE "UsuarioId" = 'a0b4e55f-ca8e-4e1c-9ae6-14c1a6c52290';
-- UPDATE "Investimentos"      SET "EstruturaId" = NULL WHERE "UsuarioId" = 'a0b4e55f-ca8e-4e1c-9ae6-14c1a6c52290';
-- DELETE FROM "ParticipacoesEstrutura" WHERE "UsuarioId" = 'a0b4e55f-ca8e-4e1c-9ae6-14c1a6c52290';
-- DELETE FROM "Estruturas"             WHERE "UsuarioId" = 'a0b4e55f-ca8e-4e1c-9ae6-14c1a6c52290';

-- ── Estruturas ───────────────────────────────────────────────────────────────
INSERT INTO "Estruturas" ("Id","UsuarioId","Nome","Tipo","Jurisdicao","ConstituidaEm","Observacoes","CriadoEm","AtualizadoEm") VALUES
 ('e0000001-0000-0000-0000-000000000001','a0b4e55f-ca8e-4e1c-9ae6-14c1a6c52290','Trust Internacional',      1,'Zurique · Suíça',            '2022-03-01T00:00:00Z','Trust discricionário — topo da estrutura sucessória.', now(), NULL),
 ('e0000002-0000-0000-0000-000000000002','a0b4e55f-ca8e-4e1c-9ae6-14c1a6c52290','Holding Patrimonial',       2,'Brasil · SP',                '2021-06-01T00:00:00Z','Detém o portfólio imobiliário da família.',            now(), NULL),
 ('e0000003-0000-0000-0000-000000000003','a0b4e55f-ca8e-4e1c-9ae6-14c1a6c52290','Holding de Participações',  3,'Brasil · SP',                '2021-06-01T00:00:00Z','Participações nas empresas operacionais.',             now(), NULL),
 ('e0000004-0000-0000-0000-000000000004','a0b4e55f-ca8e-4e1c-9ae6-14c1a6c52290','BVI Holding Co.',           4,'Ilhas Virgens Britânicas',   '2023-01-15T00:00:00Z','Holding offshore de investimentos.',                   now(), NULL),
 ('e0000005-0000-0000-0000-000000000005','a0b4e55f-ca8e-4e1c-9ae6-14c1a6c52290','Cayman Investment Ltd.',    4,'Cayman',                     '2023-02-10T00:00:00Z','Veículo de investimento internacional.',               now(), NULL),
 ('e0000006-0000-0000-0000-000000000006','a0b4e55f-ca8e-4e1c-9ae6-14c1a6c52290','Bahamas Asset Mgmt.',       4,'Bahamas',                    '2023-05-20T00:00:00Z','Gestão de ativos e PPLI.',                             now(), NULL);

-- ── Participações (grafo). EstruturaPaiId NULL = Família/cliente (raiz) ───────
INSERT INTO "ParticipacoesEstrutura" ("Id","UsuarioId","EstruturaPaiId","EstruturaFilhaId","PercentualParticipacao","TipoRelacao","CriadoEm") VALUES
 (gen_random_uuid(),'a0b4e55f-ca8e-4e1c-9ae6-14c1a6c52290', NULL,                                  'e0000001-0000-0000-0000-000000000001', 100.0000, 2, now()), -- Família → Trust (benefício)
 (gen_random_uuid(),'a0b4e55f-ca8e-4e1c-9ae6-14c1a6c52290','e0000001-0000-0000-0000-000000000001','e0000002-0000-0000-0000-000000000002', 100.0000, 1, now()), -- Trust → Holding Patrimonial
 (gen_random_uuid(),'a0b4e55f-ca8e-4e1c-9ae6-14c1a6c52290','e0000001-0000-0000-0000-000000000001','e0000003-0000-0000-0000-000000000003', 100.0000, 1, now()), -- Trust → Holding Participações
 (gen_random_uuid(),'a0b4e55f-ca8e-4e1c-9ae6-14c1a6c52290','e0000001-0000-0000-0000-000000000001','e0000004-0000-0000-0000-000000000004', 100.0000, 1, now()), -- Trust → BVI
 (gen_random_uuid(),'a0b4e55f-ca8e-4e1c-9ae6-14c1a6c52290','e0000001-0000-0000-0000-000000000001','e0000005-0000-0000-0000-000000000005', 100.0000, 1, now()), -- Trust → Cayman
 (gen_random_uuid(),'a0b4e55f-ca8e-4e1c-9ae6-14c1a6c52290','e0000001-0000-0000-0000-000000000001','e0000006-0000-0000-0000-000000000006', 100.0000, 1, now()); -- Trust → Bahamas

-- ── Ativos (imóveis / participações / bens) ──────────────────────────────────
-- Colunas: Id,UsuarioId,Nome,Tipo,Moeda,ValorAtual,ValorizacaoAnualPct,ReceitaMensal,DespesaMensal,CriadoEm,AtualizadoEm,EstruturaId
INSERT INTO "AtivosPatrimoniais"
 ("Id","UsuarioId","Nome","Tipo","Moeda","ValorAtual","ValorizacaoAnualPct","ReceitaMensal","DespesaMensal","CriadoEm","AtualizadoEm","EstruturaId") VALUES
 -- Holding Patrimonial (imóveis, BRL)
 (gen_random_uuid(),'a0b4e55f-ca8e-4e1c-9ae6-14c1a6c52290','Edifício Corporate SP',   1,1, 12000000.00, 6.0, 90000.00, 12000.00, now(), NULL,'e0000002-0000-0000-0000-000000000002'),
 (gen_random_uuid(),'a0b4e55f-ca8e-4e1c-9ae6-14c1a6c52290','Fazenda Boa Vista',       1,1,  8500000.00, 4.0, 15000.00,  8000.00, now(), NULL,'e0000002-0000-0000-0000-000000000002'),
 (gen_random_uuid(),'a0b4e55f-ca8e-4e1c-9ae6-14c1a6c52290','Galpão Logístico',        1,1,  6000000.00, 5.0, 45000.00,  5000.00, now(), NULL,'e0000002-0000-0000-0000-000000000002'),
 (gen_random_uuid(),'a0b4e55f-ca8e-4e1c-9ae6-14c1a6c52290','Cobertura Jardins',       1,1,  5000000.00, 5.5,     0.00,  3000.00, now(), NULL,'e0000002-0000-0000-0000-000000000002'),
 -- Holding de Participações (participações em empresas, BRL)
 (gen_random_uuid(),'a0b4e55f-ca8e-4e1c-9ae6-14c1a6c52290','Participação Empresa Op. A',5,1, 9500000.00, 8.0, 60000.00, 0.00, now(), NULL,'e0000003-0000-0000-0000-000000000003'),
 (gen_random_uuid(),'a0b4e55f-ca8e-4e1c-9ae6-14c1a6c52290','Participação Empresa Op. B',5,1, 9500000.00, 7.0, 55000.00, 0.00, now(), NULL,'e0000003-0000-0000-0000-000000000003'),
 -- Bahamas (imóvel internacional + PPLI, USD)
 (gen_random_uuid(),'a0b4e55f-ca8e-4e1c-9ae6-14c1a6c52290','Imóvel Miami',            1,2,  2000000.00, 4.0, 8000.00, 1500.00, now(), NULL,'e0000006-0000-0000-0000-000000000006'),
 (gen_random_uuid(),'a0b4e55f-ca8e-4e1c-9ae6-14c1a6c52290','Private Placement Life Insurance (PPLI)', 99,2, 3000000.00, 5.0, 0.00, 0.00, now(), NULL,'e0000006-0000-0000-0000-000000000006'),
 -- Pessoa física (sem estrutura)
 (gen_random_uuid(),'a0b4e55f-ca8e-4e1c-9ae6-14c1a6c52290','Residência da Família',   1,1,  4500000.00, 5.0,     0.00, 2500.00, now(), NULL, NULL),
 (gen_random_uuid(),'a0b4e55f-ca8e-4e1c-9ae6-14c1a6c52290','Veículo (blindado)',      2,1,   350000.00,-15.0,    0.00,  800.00, now(), NULL, NULL);

-- ── Investimentos (carteiras / portfólios) ───────────────────────────────────
-- Colunas: Id,UsuarioId,Nome,Tipo,Moeda,Corretora,Ticker,ValorAplicado,ValorAtual,RentabilidadeAnualPct,CriadoEm,AtualizadoEm,ValorAtualizadoEm,Quantidade,EstruturaId
INSERT INTO "Investimentos"
 ("Id","UsuarioId","Nome","Tipo","Moeda","Corretora","Ticker","ValorAplicado","ValorAtual","RentabilidadeAnualPct","CriadoEm","AtualizadoEm","ValorAtualizadoEm","Quantidade","EstruturaId") VALUES
 -- Cayman (portfólio Suíça)
 (gen_random_uuid(),'a0b4e55f-ca8e-4e1c-9ae6-14c1a6c52290','Custódia Julius Baer', 7,2,'Julius Baer',NULL,1000000.00,1250000.00, 9.0, now(), NULL, NULL, NULL,'e0000005-0000-0000-0000-000000000005'),
 (gen_random_uuid(),'a0b4e55f-ca8e-4e1c-9ae6-14c1a6c52290','Conta Cash UBS',       4,4,'UBS',        NULL, 700000.00, 750000.00, 3.5, now(), NULL, NULL, NULL,'e0000005-0000-0000-0000-000000000005'),
 -- BVI (fundo de bonds)
 (gen_random_uuid(),'a0b4e55f-ca8e-4e1c-9ae6-14c1a6c52290','Global Bonds Fund',    7,2,'Pershing',   NULL,4000000.00,4100000.00, 5.0, now(), NULL, NULL, NULL,'e0000004-0000-0000-0000-000000000004'),
 -- Pessoa física
 (gen_random_uuid(),'a0b4e55f-ca8e-4e1c-9ae6-14c1a6c52290','Carteira de Ações BR', 1,1,'XP',        NULL, 500000.00, 620000.00,12.0, now(), NULL, NULL, NULL, NULL),
 (gen_random_uuid(),'a0b4e55f-ca8e-4e1c-9ae6-14c1a6c52290','Tesouro Direto',       4,1,'XP',        NULL, 300000.00, 330000.00,10.5, now(), NULL, NULL, NULL, NULL);

COMMIT;

-- ── Conferência rápida (opcional) ─────────────────────────────────────────────
-- SELECT "Nome","Tipo","Jurisdicao" FROM "Estruturas" WHERE "UsuarioId"='a0b4e55f-ca8e-4e1c-9ae6-14c1a6c52290';
-- SELECT e."Nome", COUNT(a.*) AS ativos, COALESCE(SUM(a."ValorAtual"),0) AS valor_direto_brl_ou_moeda
--   FROM "Estruturas" e LEFT JOIN "AtivosPatrimoniais" a ON a."EstruturaId" = e."Id"
--  WHERE e."UsuarioId"='a0b4e55f-ca8e-4e1c-9ae6-14c1a6c52290' GROUP BY e."Nome";
