using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations;

/// <summary>
/// Override de moedas por assessoria: AssessorId (null = global) + índices únicos parciais
/// (global único por código; custom único por assessor+código). SQL idempotente.
/// </summary>
[DbContext(typeof(Persistence.AppDbContext))]
[Migration("20260722190000_AddMoedaOverridePorAssessoria")]
public partial class AddMoedaOverridePorAssessoria : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("ALTER TABLE \"MoedasParam\" ADD COLUMN IF NOT EXISTS \"AssessorId\" uuid;");
        migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_MoedasParam_Codigo\";");
        migrationBuilder.Sql("""
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_MoedasParam_Codigo"
                ON "MoedasParam" ("Codigo") WHERE "AssessorId" IS NULL;
            """);
        migrationBuilder.Sql("""
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_MoedasParam_AssessorId_Codigo"
                ON "MoedasParam" ("AssessorId", "Codigo") WHERE "AssessorId" IS NOT NULL;
            """);
        migrationBuilder.Sql("""
            CREATE INDEX IF NOT EXISTS "IX_MoedasParam_AssessorId"
                ON "MoedasParam" ("AssessorId");
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_MoedasParam_AssessorId\";");
        migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_MoedasParam_AssessorId_Codigo\";");
        migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_MoedasParam_Codigo\";");
        migrationBuilder.Sql("""CREATE UNIQUE INDEX IF NOT EXISTS "IX_MoedasParam_Codigo" ON "MoedasParam" ("Codigo");""");
        migrationBuilder.Sql("ALTER TABLE \"MoedasParam\" DROP COLUMN IF EXISTS \"AssessorId\";");
    }
}
