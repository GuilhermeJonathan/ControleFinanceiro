CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;
CREATE TABLE "CartoesCredito" (
    "Id" uuid NOT NULL,
    "Nome" character varying(100) NOT NULL,
    "DiaVencimento" integer,
    "UsuarioId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_CartoesCredito" PRIMARY KEY ("Id")
);

CREATE TABLE "Categorias" (
    "Id" uuid NOT NULL,
    "Nome" character varying(100) NOT NULL,
    "Tipo" integer NOT NULL,
    "UsuarioId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_Categorias" PRIMARY KEY ("Id")
);

CREATE TABLE "HorasTrabalhadas" (
    "Id" uuid NOT NULL,
    "Descricao" character varying(200) NOT NULL,
    "ValorHora" numeric(18,2) NOT NULL,
    "Quantidade" numeric(10,2) NOT NULL,
    "Mes" integer NOT NULL,
    "Ano" integer NOT NULL,
    "UsuarioId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_HorasTrabalhadas" PRIMARY KEY ("Id")
);

CREATE TABLE "ReceitasRecorrentes" (
    "Id" uuid NOT NULL,
    "Nome" character varying(200) NOT NULL,
    "Tipo" integer NOT NULL,
    "Valor" numeric(18,2) NOT NULL,
    "ValorHora" numeric(18,2),
    "QuantidadeHoras" numeric(18,2),
    "Dia" integer NOT NULL,
    "DataInicio" timestamp with time zone NOT NULL,
    "UsuarioId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_ReceitasRecorrentes" PRIMARY KEY ("Id")
);

CREATE TABLE "SaldosContas" (
    "Id" uuid NOT NULL,
    "Banco" character varying(100) NOT NULL,
    "Saldo" numeric(18,2) NOT NULL,
    "Tipo" integer NOT NULL,
    "DataAtualizacao" timestamp with time zone NOT NULL,
    "UsuarioId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_SaldosContas" PRIMARY KEY ("Id")
);

CREATE TABLE "ParcelasCartao" (
    "Id" uuid NOT NULL,
    "CartaoCreditoId" uuid NOT NULL,
    "Descricao" character varying(200) NOT NULL,
    "ValorParcela" numeric(18,2) NOT NULL,
    "ParcelaAtual" integer NOT NULL,
    "TotalParcelas" integer NOT NULL,
    "DataInicio" timestamp with time zone NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_ParcelasCartao" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ParcelasCartao_CartoesCredito_CartaoCreditoId" FOREIGN KEY ("CartaoCreditoId") REFERENCES "CartoesCredito" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Lancamentos" (
    "Id" uuid NOT NULL,
    "Descricao" character varying(200) NOT NULL,
    "Data" timestamp with time zone NOT NULL,
    "Valor" numeric(18,2) NOT NULL,
    "Tipo" integer NOT NULL,
    "Situacao" integer NOT NULL,
    "Mes" integer NOT NULL,
    "Ano" integer NOT NULL,
    "CategoriaId" uuid,
    "CartaoId" uuid,
    "ParcelaAtual" integer,
    "TotalParcelas" integer,
    "GrupoParcelas" uuid,
    "ReceitaRecorrenteId" uuid,
    "IsRecorrente" boolean NOT NULL,
    "ContaBancariaId" uuid,
    "DataPagamento" timestamp with time zone,
    "UsuarioId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_Lancamentos" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Lancamentos_CartoesCredito_CartaoId" FOREIGN KEY ("CartaoId") REFERENCES "CartoesCredito" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Lancamentos_Categorias_CategoriaId" FOREIGN KEY ("CategoriaId") REFERENCES "Categorias" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Lancamentos_ReceitasRecorrentes_ReceitaRecorrenteId" FOREIGN KEY ("ReceitaRecorrenteId") REFERENCES "ReceitasRecorrentes" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Lancamentos_SaldosContas_ContaBancariaId" FOREIGN KEY ("ContaBancariaId") REFERENCES "SaldosContas" ("Id") ON DELETE SET NULL
);

CREATE INDEX "IX_CartoesCredito_UsuarioId" ON "CartoesCredito" ("UsuarioId");

CREATE INDEX "IX_Categorias_UsuarioId" ON "Categorias" ("UsuarioId");

CREATE INDEX "IX_HorasTrabalhadas_UsuarioId" ON "HorasTrabalhadas" ("UsuarioId");

CREATE INDEX "IX_Lancamentos_CartaoId" ON "Lancamentos" ("CartaoId");

CREATE INDEX "IX_Lancamentos_CategoriaId" ON "Lancamentos" ("CategoriaId");

CREATE INDEX "IX_Lancamentos_ContaBancariaId" ON "Lancamentos" ("ContaBancariaId");

CREATE INDEX "IX_Lancamentos_ReceitaRecorrenteId" ON "Lancamentos" ("ReceitaRecorrenteId");

CREATE INDEX "IX_Lancamentos_UsuarioId" ON "Lancamentos" ("UsuarioId");

CREATE INDEX "IX_ParcelasCartao_CartaoCreditoId" ON "ParcelasCartao" ("CartaoCreditoId");

CREATE INDEX "IX_ReceitasRecorrentes_UsuarioId" ON "ReceitasRecorrentes" ("UsuarioId");

CREATE INDEX "IX_SaldosContas_UsuarioId" ON "SaldosContas" ("UsuarioId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260427171115_InitialCreate', '10.0.7');

COMMIT;

