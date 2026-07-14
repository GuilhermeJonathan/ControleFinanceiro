-- Seed completo de TiposAtivoParam com icones
INSERT INTO "TiposAtivoParam" ("Id","Nome","Ordem","Ativo","IsSystem","Icone") VALUES
  (1,  'Imovel',       1,  TRUE, TRUE, '🏠'),
  (2,  'Veiculo',      2,  TRUE, TRUE, '🚗'),
  (3,  'Embarcacao',   3,  TRUE, TRUE, '⛵'),
  (4,  'Aeronave',     4,  TRUE, TRUE, '✈'),
  (5,  'Participacao', 5,  TRUE, TRUE, '🏢'),
  (6,  'Investimento', 6,  TRUE, TRUE, '📈'),
  (99, 'Outro',        99, TRUE, TRUE, '◆')
ON CONFLICT ("Id") DO UPDATE
  SET "Nome"  = EXCLUDED."Nome",
      "Icone" = EXCLUDED."Icone",
      "Ordem" = EXCLUDED."Ordem";

-- Seed completo de TiposInvestimentoParam com icones
INSERT INTO "TiposInvestimentoParam" ("Id","Nome","Ordem","Ativo","IsSystem","Icone") VALUES
  (1,  'Acoes',        1,  TRUE, TRUE, '📊'),
  (2,  'FII',          2,  TRUE, TRUE, '🏢'),
  (3,  'ETF',          3,  TRUE, TRUE, '🌍'),
  (4,  'Renda Fixa',   4,  TRUE, TRUE, '💰'),
  (5,  'Multimercado', 5,  TRUE, TRUE, '📦'),
  (6,  'Cripto',       6,  TRUE, TRUE, '🔗'),
  (7,  'Exterior',     7,  TRUE, TRUE, '🌐'),
  (99, 'Outro',        99, TRUE, TRUE, '◆')
ON CONFLICT ("Id") DO UPDATE
  SET "Nome"  = EXCLUDED."Nome",
      "Icone" = EXCLUDED."Icone",
      "Ordem" = EXCLUDED."Ordem";

-- Seed completo de MoedasParam
INSERT INTO "MoedasParam" ("Id","Codigo","Nome","Ordem","Ativo","IsSystem") VALUES
  (1,'BRL','Real Brasileiro',  1,TRUE,TRUE),
  (2,'USD','Dolar Americano',  2,TRUE,TRUE),
  (3,'EUR','Euro',             3,TRUE,TRUE),
  (4,'CHF','Franco Suico',     4,TRUE,TRUE),
  (5,'GBP','Libra Esterlina',  5,TRUE,TRUE)
ON CONFLICT ("Id") DO UPDATE
  SET "Nome"   = EXCLUDED."Nome",
      "Codigo" = EXCLUDED."Codigo",
      "Ordem"  = EXCLUDED."Ordem";
