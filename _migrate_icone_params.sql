-- Adiciona coluna Icone nas tabelas de parametros (nullable, max 10 chars para emoji)
ALTER TABLE "TiposAtivoParam"        ADD COLUMN IF NOT EXISTS "Icone" character varying(10) NULL;
ALTER TABLE "TiposInvestimentoParam" ADD COLUMN IF NOT EXISTS "Icone" character varying(10) NULL;

-- Atualiza icones dos itens existentes (espelha os emojis do app)
UPDATE "TiposAtivoParam" SET "Icone" = CASE "Id"
  WHEN 1  THEN '🏠'
  WHEN 2  THEN '🚗'
  WHEN 3  THEN '⛵'
  WHEN 4  THEN '✈️'
  WHEN 5  THEN '🏢'
  WHEN 6  THEN '📈'
  WHEN 99 THEN '◆'
  ELSE NULL END;

UPDATE "TiposInvestimentoParam" SET "Icone" = CASE "Id"
  WHEN 1  THEN '📊'
  WHEN 2  THEN '🏢'
  WHEN 3  THEN '🌍'
  WHEN 4  THEN '💰'
  WHEN 5  THEN '📦'
  WHEN 6  THEN '🔗'
  WHEN 7  THEN '🌐'
  WHEN 99 THEN '◆'
  ELSE NULL END;
