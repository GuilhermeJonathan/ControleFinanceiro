using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations;

/// <summary>
/// Vínculo opcional de dívida → ativo patrimonial (alavancagem). SQL idempotente.
/// </summary>
[DbContext(typeof(Persistence.AppDbContext))]
[Migration("20260812010000_AddAtivoVinculadoPassivo")]
public partial class AddAtivoVinculadoPassivo : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            "ALTER TABLE \"PassivosPatrimoniais\" ADD COLUMN IF NOT EXISTS \"AtivoVinculadoId\" uuid;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("ALTER TABLE \"PassivosPatrimoniais\" DROP COLUMN IF EXISTS \"AtivoVinculadoId\";");
    }
}
