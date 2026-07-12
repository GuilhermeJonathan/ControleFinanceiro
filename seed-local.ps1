# Seed do ambiente LOCAL (container findog-postgres) para testar a Assessoria.
# Cria: assessor + cliente (senha Senha@123 para ambos), vinculo de assessoria
# ativo, e dados financeiros do cliente (categorias, lancamentos, saldos, meta).
# Idempotente: pode rodar mais de uma vez (ON CONFLICT DO NOTHING).
# Uso: .\seed-local.ps1   (com o container findog-postgres rodando)

$ErrorActionPreference = "Stop"

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
