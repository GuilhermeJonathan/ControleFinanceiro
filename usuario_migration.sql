-- Add UsuarioId to all financial tables
-- Run this in DBeaver against your Supabase PostgreSQL

ALTER TABLE "Lancamentos" ADD COLUMN IF NOT EXISTS "UsuarioId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
ALTER TABLE "CartoesCredito" ADD COLUMN IF NOT EXISTS "UsuarioId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
ALTER TABLE "SaldosContas" ADD COLUMN IF NOT EXISTS "UsuarioId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
ALTER TABLE "Categorias" ADD COLUMN IF NOT EXISTS "UsuarioId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
ALTER TABLE "ReceitasRecorrentes" ADD COLUMN IF NOT EXISTS "UsuarioId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
ALTER TABLE "HorasTrabalhadas" ADD COLUMN IF NOT EXISTS "UsuarioId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

CREATE INDEX IF NOT EXISTS "IX_Lancamentos_UsuarioId" ON "Lancamentos" ("UsuarioId");
CREATE INDEX IF NOT EXISTS "IX_CartoesCredito_UsuarioId" ON "CartoesCredito" ("UsuarioId");
CREATE INDEX IF NOT EXISTS "IX_SaldosContas_UsuarioId" ON "SaldosContas" ("UsuarioId");
CREATE INDEX IF NOT EXISTS "IX_Categorias_UsuarioId" ON "Categorias" ("UsuarioId");
CREATE INDEX IF NOT EXISTS "IX_ReceitasRecorrentes_UsuarioId" ON "ReceitasRecorrentes" ("UsuarioId");
CREATE INDEX IF NOT EXISTS "IX_HorasTrabalhadas_UsuarioId" ON "HorasTrabalhadas" ("UsuarioId");
