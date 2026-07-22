using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations;

/// <summary>Subclasse (2º nível dentro do Tipo/classe) opcional em Investimentos. SQL idempotente.</summary>
[DbContext(typeof(Persistence.AppDbContext))]
[Migration("20260725400000_AddSubclasseInvestimento")]
public partial class AddSubclasseInvestimento : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.Sql("ALTER TABLE \"Investimentos\" ADD COLUMN IF NOT EXISTS \"Subclasse\" character varying(60);");

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.Sql("ALTER TABLE \"Investimentos\" DROP COLUMN IF EXISTS \"Subclasse\";");
}
