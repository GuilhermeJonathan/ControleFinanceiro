using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations;

/// <summary>
/// Adiciona a coluna ExpiraEm aos vínculos de assessoria e corretor — validade do
/// convite pendente. SQL idempotente para tolerar drift de schema entre ambientes.
/// </summary>
[DbContext(typeof(Persistence.AppDbContext))]
[Migration("20260718100000_AddExpiraEmVinculos")]
public partial class AddExpiraEmVinculos : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            "ALTER TABLE \"VinculosAssessoria\" ADD COLUMN IF NOT EXISTS \"ExpiraEm\" timestamp with time zone;");
        migrationBuilder.Sql(
            "ALTER TABLE \"VinculosCorretor\" ADD COLUMN IF NOT EXISTS \"ExpiraEm\" timestamp with time zone;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("ALTER TABLE \"VinculosAssessoria\" DROP COLUMN IF EXISTS \"ExpiraEm\";");
        migrationBuilder.Sql("ALTER TABLE \"VinculosCorretor\" DROP COLUMN IF EXISTS \"ExpiraEm\";");
    }
}
