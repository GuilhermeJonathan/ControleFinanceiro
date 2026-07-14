# Seed do ambiente LOCAL (container findog-postgres) para testar a Assessoria.
# Cria: assessor + cliente (senha Senha@123 para ambos), vinculo de assessoria
# ativo, e dados financeiros do cliente (categorias, lancamentos, saldos, meta).
# Idempotente: pode rodar mais de uma vez (ON CONFLICT DO NOTHING).
# Uso: .\seed-local.ps1   (com o container findog-postgres rodando)

# NOTICE do Postgres vai pro stderr; não deixar isso abortar o script
$ErrorActionPreference = "Continue"

$ASSESSOR_ID = "aaaaaaaa-0000-0000-0000-000000000001"
$CLIENTE_ID  = "bbbbbbbb-0000-0000-0000-000000000002"

$loginSql = @"
CREATE EXTENSION IF NOT EXISTS pgcrypto;

INSERT INTO "Users" ("Id","Name","Email","Document","PasswordHash","UserTypeId",
    "IsActive","IsBlocked","IsPaying","TrialD1EmailSent","TrialD7EmailSent",
    "PodeVerImoveis","ReengagementEmailSent","PlanType","PlanExpiresAt","CreatedAt")
VALUES
  ('$ASSESSOR_ID','Assessor Teste','assessor@local.test','11111111111',
   crypt('Senha@123', gen_salt('bf', 11)), 3,
   TRUE, FALSE, TRUE, FALSE, FALSE, FALSE, FALSE, 2, now() + interval '1 year', now()),
  ('$CLIENTE_ID','Cliente Teste','cliente@local.test','22222222222',
   crypt('Senha@123', gen_salt('bf', 11)), 2,
   TRUE, FALSE, TRUE, FALSE, FALSE, FALSE, FALSE, 2, now() + interval '1 year', now())
ON CONFLICT ("Id") DO NOTHING;
"@

$mainSql = @"
INSERT INTO "VinculosAssessoria"
    ("Id","AssessorId","ClienteId","CodigoConvite","CriadoEm","AceitoEm","NomeCliente","NomeAssessor")
VALUES ('cccccccc-0000-0000-0000-000000000003','$ASSESSOR_ID','$CLIENTE_ID','SEED01',
        now() - interval '10 days', now() - interval '9 days','Cliente Teste','Assessor Teste')
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "Categorias" ("Id","Nome","Tipo","UsuarioId","LimiteMensal","CreatedAt") VALUES
  ('dddddddd-0000-0000-0000-000000000001','Salario',    1,'$CLIENTE_ID',NULL,now()),
  ('dddddddd-0000-0000-0000-000000000002','Mercado',    2,'$CLIENTE_ID', 800,now()),
  ('dddddddd-0000-0000-0000-000000000003','Transporte', 2,'$CLIENTE_ID', 400,now()),
  ('dddddddd-0000-0000-0000-000000000004','Lazer',      2,'$CLIENTE_ID', 300,now()),
  ('dddddddd-0000-0000-0000-000000000005','Moradia',    2,'$CLIENTE_ID',NULL,now())
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "Lancamentos" ("Id","Descricao","Data","Valor","Tipo","Situacao","Mes","Ano",
    "CategoriaId","IsRecorrente","UsuarioId","CreatedAt") VALUES
  ('eeeeeeee-0000-0000-0000-000000000001','Salario',
    date_trunc('month', now()) + interval '4 days', 5200.00, 1, 1,
    EXTRACT(MONTH FROM now())::int, EXTRACT(YEAR FROM now())::int,
    'dddddddd-0000-0000-0000-000000000001', FALSE,'$CLIENTE_ID', now()),
  ('eeeeeeee-0000-0000-0000-000000000002','Supermercado',
    date_trunc('month', now()) + interval '6 days', 650.00, 2, 2,
    EXTRACT(MONTH FROM now())::int, EXTRACT(YEAR FROM now())::int,
    'dddddddd-0000-0000-0000-000000000002', FALSE,'$CLIENTE_ID', now()),
  ('eeeeeeee-0000-0000-0000-000000000003','Feira',
    date_trunc('month', now()) + interval '8 days', 320.00, 2, 2,
    EXTRACT(MONTH FROM now())::int, EXTRACT(YEAR FROM now())::int,
    'dddddddd-0000-0000-0000-000000000002', FALSE,'$CLIENTE_ID', now()),
  ('eeeeeeee-0000-0000-0000-000000000004','Combustivel',
    date_trunc('month', now()) + interval '5 days', 380.00, 2, 2,
    EXTRACT(MONTH FROM now())::int, EXTRACT(YEAR FROM now())::int,
    'dddddddd-0000-0000-0000-000000000003', FALSE,'$CLIENTE_ID', now()),
  ('eeeeeeee-0000-0000-0000-000000000005','Cinema e jantar',
    date_trunc('month', now()) + interval '9 days', 240.00, 2, 2,
    EXTRACT(MONTH FROM now())::int, EXTRACT(YEAR FROM now())::int,
    'dddddddd-0000-0000-0000-000000000004', FALSE,'$CLIENTE_ID', now()),
  ('eeeeeeee-0000-0000-0000-000000000006','Aluguel',
    date_trunc('month', now()) + interval '4 days', 1800.00, 2, 2,
    EXTRACT(MONTH FROM now())::int, EXTRACT(YEAR FROM now())::int,
    'dddddddd-0000-0000-0000-000000000005', FALSE,'$CLIENTE_ID', now())
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "SaldosContas" ("Id","Banco","Saldo","Tipo","DataAtualizacao","UsuarioId","CreatedAt")
VALUES ('ffffffff-0000-0000-0000-000000000001','Nubank', 7500.00, 1, now(),'$CLIENTE_ID', now())
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "Metas" ("Id","UsuarioId","Titulo","ValorMeta","ValorAtual","Status","CriadoEm","CreatedAt")
VALUES ('99999999-0000-0000-0000-000000000001','$CLIENTE_ID','Reserva de emergencia', 15000.00, 7500.00, 1, now(), now())
ON CONFLICT ("Id") DO NOTHING;

-- Ativos patrimoniais demo (modulo B2B). Tipo: 1=Imovel 2=Veiculo 3=Embarcacao 6=Investimento
-- Moeda: 1=BRL 2=USD 3=EUR 4=CHF
INSERT INTO "AtivosPatrimoniais" ("Id","UsuarioId","Nome","Tipo","Moeda","ValorAtual","ValorizacaoAnualPct","CriadoEm") VALUES
  ('a1000000-0000-0000-0000-000000000001','$CLIENTE_ID','Apartamento Jardins SP', 1, 1, 2350000.00,  8.0, now()),
  ('a1000000-0000-0000-0000-000000000002','$CLIENTE_ID','Carteira EUA (ETFs)',    6, 2,  180000.00, 12.0, now()),
  ('a1000000-0000-0000-0000-000000000003','$CLIENTE_ID','Conta Suica (UBS)',      6, 4,  120000.00,  3.5, now()),
  ('a1000000-0000-0000-0000-000000000004','$CLIENTE_ID','Porsche 911',            2, 1,  850000.00, -6.0, now()),
  ('a1000000-0000-0000-0000-000000000005','$CLIENTE_ID','Fundo Europa',           6, 3,   95000.00,  5.0, now())
ON CONFLICT ("Id") DO NOTHING;

-- Investimentos demo
INSERT INTO "Investimentos" ("Id","UsuarioId","Nome","Tipo","Moeda","Corretora","Ticker","ValorAplicado","ValorAtual","RentabilidadeAnualPct","CriadoEm") VALUES
  ('b1000000-0000-0000-0000-000000000001','$CLIENTE_ID','ITSA4',         1, 1,'XP Investimentos','ITSA4',   50000.00,  58000.00, 15.5, now()),
  ('b1000000-0000-0000-0000-000000000002','$CLIENTE_ID','HGLG11',        2, 1,'XP Investimentos','HGLG11',  30000.00,  32500.00,  8.2, now()),
  ('b1000000-0000-0000-0000-000000000003','$CLIENTE_ID','VOO',           3, 2,'Interactive Brokers','VOO',  80000.00, 102000.00, 22.0, now()),
  ('b1000000-0000-0000-0000-000000000004','$CLIENTE_ID','LCI Bradesco',  4, 1,'Bradesco',         NULL,    100000.00, 112000.00, 11.5, now()),
  ('b1000000-0000-0000-0000-000000000005','$CLIENTE_ID','Bitcoin',       6, 2,'Binance',           'BTC',   20000.00,  35000.00, 65.0, now())
ON CONFLICT ("Id") DO NOTHING;

-- ─────────────────────────────────────────────────────────────────────────
-- Parâmetros configuráveis pelo assessor
-- TipoAtivo  : espelho do enum TipoAtivo (isSystem=TRUE = não excluível)
-- TipoInvest : espelho do enum TipoInvestimento
-- Moeda      : espelho do enum MoedaPatrimonio
-- ─────────────────────────────────────────────────────────────────────────
INSERT INTO "TiposAtivoParam" ("Id","Nome","Ordem","Ativo","IsSystem") VALUES
  (1, 'Imóvel',       1, TRUE, TRUE),
  (2, 'Veículo',      2, TRUE, TRUE),
  (3, 'Embarcação',   3, TRUE, TRUE),
  (4, 'Aeronave',     4, TRUE, TRUE),
  (5, 'Participação', 5, TRUE, TRUE),
  (6, 'Investimento', 6, TRUE, TRUE),
  (99,'Outro',        99,TRUE, TRUE)
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "TiposInvestimentoParam" ("Id","Nome","Ordem","Ativo","IsSystem") VALUES
  (1, 'Ações',        1, TRUE, TRUE),
  (2, 'FII',          2, TRUE, TRUE),
  (3, 'ETF',          3, TRUE, TRUE),
  (4, 'Renda Fixa',   4, TRUE, TRUE),
  (5, 'Multimercado', 5, TRUE, TRUE),
  (6, 'Cripto',       6, TRUE, TRUE),
  (7, 'Exterior',     7, TRUE, TRUE),
  (99,'Outro',        99,TRUE, TRUE)
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "MoedasParam" ("Id","Codigo","Nome","Ordem","Ativo","IsSystem") VALUES
  (1,'BRL','Real Brasileiro',   1,TRUE,TRUE),
  (2,'USD','Dólar Americano',   2,TRUE,TRUE),
  (3,'EUR','Euro',              3,TRUE,TRUE),
  (4,'CHF','Franco Suíço',      4,TRUE,TRUE),
  (5,'GBP','Libra Esterlina',   5,TRUE,TRUE)
ON CONFLICT ("Id") DO NOTHING;
"@

Write-Host "Populando findog..." -ForegroundColor Cyan
$loginSql | docker exec -i findog-postgres psql -U postgres -d findog -v ON_ERROR_STOP=1
Write-Host "Populando findog..." -ForegroundColor Cyan
$mainSql | docker exec -i findog-postgres psql -U postgres -d findog -v ON_ERROR_STOP=1

Write-Host ""
Write-Host "Seed concluido! Logins (senha: Senha@123):" -ForegroundColor Green
Write-Host "  Assessor: assessor@local.test  (menu 'Meus Clientes')"
Write-Host "  Cliente:  cliente@local.test   (dados financeiros no mes corrente)"
Write-Host "  Vinculo de assessoria ja ativo entre os dois."
