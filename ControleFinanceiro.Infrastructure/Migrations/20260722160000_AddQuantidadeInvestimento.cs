using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations;

/// <summary>Quantidade de cotas/ações no Investimento (carteira: ValorAtual = Quantidade × preço). SQL idempotente.</summary>
[DbContext(typeof(Persistence.AppDbContext))]
[Migration("20260722160000_AddQuantidadeInvestimento")]
public partial class AddQuantidadeInvestimento : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.Sql(
            "ALTER TABLE \"Investimentos\" ADD COLUMN IF NOT EXISTS \"Quantidade\" numeric(18,6);");

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.Sql("ALTER TABLE \"Investimentos\" DROP COLUMN IF EXISTS \"Quantidade\";");
}
