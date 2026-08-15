using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations;

/// <summary>Slug/rota do login whitelabel na consultoria (único quando preenchido). SQL idempotente.</summary>
[DbContext(typeof(Persistence.AppDbContext))]
[Migration("20260815010000_AddSlugConsultoria")]
public partial class AddSlugConsultoria : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            "ALTER TABLE \"ConsultoriaConfigs\" ADD COLUMN IF NOT EXISTS \"Slug\" character varying(60);");
        // Único apenas entre os slugs preenchidos (índice parcial).
        migrationBuilder.Sql(
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_ConsultoriaConfigs_Slug\" ON \"ConsultoriaConfigs\" (\"Slug\") WHERE \"Slug\" IS NOT NULL;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_ConsultoriaConfigs_Slug\";");
        migrationBuilder.Sql("ALTER TABLE \"ConsultoriaConfigs\" DROP COLUMN IF EXISTS \"Slug\";");
    }
}
