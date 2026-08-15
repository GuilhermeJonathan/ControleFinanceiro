using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations;

/// <summary>
/// Flag Nacional×Exterior no Tipo de investimento. Tipos marcados "Exterior" usam
/// cotação global (Yahoo); nacionais usam brapi (B3). SQL idempotente.
/// </summary>
[DbContext(typeof(Persistence.AppDbContext))]
[Migration("20260812000000_AddExteriorTipoInvestimento")]
public partial class AddExteriorTipoInvestimento : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            "ALTER TABLE \"TiposInvestimentoParam\" ADD COLUMN IF NOT EXISTS \"Exterior\" boolean NOT NULL DEFAULT false;");

        // Marca o tipo de sistema "Exterior" (seed) como exterior.
        migrationBuilder.Sql(
            "UPDATE \"TiposInvestimentoParam\" SET \"Exterior\" = true WHERE lower(\"Nome\") = 'exterior';");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("ALTER TABLE \"TiposInvestimentoParam\" DROP COLUMN IF EXISTS \"Exterior\";");
    }
}
