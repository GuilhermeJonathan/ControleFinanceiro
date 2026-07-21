using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations;

/// <summary>
/// F1 Estruturas: tabelas Estruturas + ParticipacoesEstrutura (grafo) e coluna EstruturaId
/// em AtivosPatrimoniais/Investimentos (ativo pendurado numa estrutura). SQL idempotente.
/// </summary>
[DbContext(typeof(Persistence.AppDbContext))]
[Migration("20260723100000_AddEstruturas")]
public partial class AddEstruturas : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
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
            """);
        migrationBuilder.Sql("""
            CREATE INDEX IF NOT EXISTS "IX_Estruturas_UsuarioId" ON "Estruturas" ("UsuarioId");
            """);
        migrationBuilder.Sql("""
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
            """);
        migrationBuilder.Sql("""
            CREATE INDEX IF NOT EXISTS "IX_ParticipacoesEstrutura_UsuarioId"
                ON "ParticipacoesEstrutura" ("UsuarioId");
            """);
        migrationBuilder.Sql("""
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_ParticipacoesEstrutura_UsuarioId_EstruturaPaiId_EstruturaFilhaId"
                ON "ParticipacoesEstrutura" ("UsuarioId", "EstruturaPaiId", "EstruturaFilhaId");
            """);
        migrationBuilder.Sql(
            "ALTER TABLE \"AtivosPatrimoniais\" ADD COLUMN IF NOT EXISTS \"EstruturaId\" uuid;");
        migrationBuilder.Sql(
            "ALTER TABLE \"Investimentos\" ADD COLUMN IF NOT EXISTS \"EstruturaId\" uuid;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("ALTER TABLE \"Investimentos\" DROP COLUMN IF EXISTS \"EstruturaId\";");
        migrationBuilder.Sql("ALTER TABLE \"AtivosPatrimoniais\" DROP COLUMN IF EXISTS \"EstruturaId\";");
        migrationBuilder.Sql("""DROP TABLE IF EXISTS "ParticipacoesEstrutura";""");
        migrationBuilder.Sql("""DROP TABLE IF EXISTS "Estruturas";""");
    }
}
