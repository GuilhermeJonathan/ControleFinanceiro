using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations;

/// <summary>
/// Whitelabel: override de tipos por assessoria. Adiciona AssessorId (null = global) nos
/// tipos de ativo/investimento e cria a tabela ParametrosOcultos (defaults ocultados por
/// assessoria). SQL idempotente.
/// </summary>
[DbContext(typeof(Persistence.AppDbContext))]
[Migration("20260722100000_AddParametroOverridePorAssessoria")]
public partial class AddParametroOverridePorAssessoria : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            "ALTER TABLE \"TiposAtivoParam\" ADD COLUMN IF NOT EXISTS \"AssessorId\" uuid;");
        migrationBuilder.Sql(
            "ALTER TABLE \"TiposInvestimentoParam\" ADD COLUMN IF NOT EXISTS \"AssessorId\" uuid;");
        migrationBuilder.Sql("""
            CREATE INDEX IF NOT EXISTS "IX_TiposAtivoParam_AssessorId"
                ON "TiposAtivoParam" ("AssessorId");
            """);
        migrationBuilder.Sql("""
            CREATE INDEX IF NOT EXISTS "IX_TiposInvestimentoParam_AssessorId"
                ON "TiposInvestimentoParam" ("AssessorId");
            """);
        migrationBuilder.Sql("""
            CREATE TABLE IF NOT EXISTS "ParametrosOcultos" (
                "Id"          serial NOT NULL,
                "AssessorId"  uuid NOT NULL,
                "Tipo"        integer NOT NULL,
                "ParametroId" integer NOT NULL,
                CONSTRAINT "PK_ParametrosOcultos" PRIMARY KEY ("Id")
            );
            """);
        migrationBuilder.Sql("""
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_ParametrosOcultos_AssessorId_Tipo_ParametroId"
                ON "ParametrosOcultos" ("AssessorId", "Tipo", "ParametroId");
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""DROP TABLE IF EXISTS "ParametrosOcultos";""");
        migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_TiposAtivoParam_AssessorId\";");
        migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_TiposInvestimentoParam_AssessorId\";");
        migrationBuilder.Sql("ALTER TABLE \"TiposAtivoParam\" DROP COLUMN IF EXISTS \"AssessorId\";");
        migrationBuilder.Sql("ALTER TABLE \"TiposInvestimentoParam\" DROP COLUMN IF EXISTS \"AssessorId\";");
    }
}
