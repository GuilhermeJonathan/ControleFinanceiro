CREATE TABLE IF NOT EXISTS "Investimentos" (
    "Id"                    uuid                        NOT NULL,
    "UsuarioId"             uuid                        NOT NULL,
    "Nome"                  character varying(200)      NOT NULL,
    "Tipo"                  integer                     NOT NULL,
    "Moeda"                 integer                     NOT NULL,
    "Corretora"             character varying(100)      NULL,
    "Ticker"                character varying(20)       NULL,
    "ValorAplicado"         numeric(18,2)               NOT NULL,
    "ValorAtual"            numeric(18,2)               NOT NULL,
    "RentabilidadeAnualPct" numeric(9,4)                NULL,
    "CriadoEm"              timestamp with time zone    NOT NULL,
    "AtualizadoEm"          timestamp with time zone    NULL,
    CONSTRAINT "PK_Investimentos" PRIMARY KEY ("Id")
);

CREATE INDEX IF NOT EXISTS "IX_Investimentos_UsuarioId"
    ON "Investimentos" ("UsuarioId");

-- seed demo
INSERT INTO "Investimentos" ("Id","UsuarioId","Nome","Tipo","Moeda","Corretora","Ticker","ValorAplicado","ValorAtual","RentabilidadeAnualPct","CriadoEm") VALUES
  ('b1000000-0000-0000-0000-000000000001','bbbbbbbb-0000-0000-0000-000000000002','ITSA4',         1, 1,'XP Investimentos','ITSA4',   50000.00,  58000.00, 15.5, now()),
  ('b1000000-0000-0000-0000-000000000002','bbbbbbbb-0000-0000-0000-000000000002','HGLG11',        2, 1,'XP Investimentos','HGLG11',  30000.00,  32500.00,  8.2, now()),
  ('b1000000-0000-0000-0000-000000000003','bbbbbbbb-0000-0000-0000-000000000002','VOO',           3, 2,'Interactive Brokers','VOO',  80000.00, 102000.00, 22.0, now()),
  ('b1000000-0000-0000-0000-000000000004','bbbbbbbb-0000-0000-0000-000000000002','LCI Bradesco',  4, 1,'Bradesco',         NULL,    100000.00, 112000.00, 11.5, now()),
  ('b1000000-0000-0000-0000-000000000005','bbbbbbbb-0000-0000-0000-000000000002','Bitcoin',       6, 2,'Binance',           'BTC',   20000.00,  35000.00, 65.0, now())
ON CONFLICT ("Id") DO NOTHI
