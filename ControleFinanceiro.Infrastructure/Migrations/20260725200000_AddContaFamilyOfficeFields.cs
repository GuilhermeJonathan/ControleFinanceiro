using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations;

/// <summary>
/// Campos family-office opcionais em ContasFinanceiras: valor de portfólio, crédito lombardo
/// (limite/utilizado) e status. SQL idempotente.
/// </summary>
[DbContext(typeof(Persistence.AppDbContext))]
[Migration("20260725200000_AddContaFamilyOfficeFields")]
public partial class AddContaFamilyOfficeFields : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("ALTER TABLE \"ContasFinanceiras\" ADD COLUMN IF NOT EXISTS \"ValorPortfolio\" numeric(18,2);");
        migrationBuilder.Sql("ALTER TABLE \"ContasFinanceiras\" ADD COLUMN IF NOT EXISTS \"LombardLimite\" numeric(18,2);");
        migrationBuilder.Sql("ALTER TABLE \"ContasFinanceiras\" ADD COLUMN IF NOT EXISTS \"LombardUtilizado\" numeric(18,2);");
        migrationBuilder.Sql("ALTER TABLE \"ContasFinanceiras\" ADD COLUMN IF NOT EXISTS \"Status\" character varying(60);");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("ALTER TABLE \"ContasFinanceiras\" DROP COLUMN IF EXISTS \"ValorPortfolio\";");
        migrationBuilder.Sql("ALTER TABLE \"ContasFinanceiras\" DROP COLUMN IF EXISTS \"LombardLimite\";");
        migrationBuilder.Sql("ALTER TABLE \"ContasFinanceiras\" DROP COLUMN IF EXISTS \"LombardUtilizado\";");
        migrationBuilder.Sql("ALTER TABLE \"ContasFinanceiras\" DROP COLUMN IF EXISTS \"Status\";");
    }
}
