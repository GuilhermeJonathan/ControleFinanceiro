using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations;

/// <summary>
/// Flag "sucessão resolvida" nas contas (relevante em contas internacionais). SQL idempotente.
/// </summary>
[DbContext(typeof(Persistence.AppDbContext))]
[Migration("20260815000000_AddSucessaoResolvidaConta")]
public partial class AddSucessaoResolvidaConta : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            "ALTER TABLE \"ContasFinanceiras\" ADD COLUMN IF NOT EXISTS \"SucessaoResolvida\" boolean NOT NULL DEFAULT false;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("ALTER TABLE \"ContasFinanceiras\" DROP COLUMN IF EXISTS \"SucessaoResolvida\";");
    }
}
