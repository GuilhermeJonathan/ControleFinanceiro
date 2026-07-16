using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations;

/// <summary>
/// Adiciona a coluna EmailConvidado aos vínculos de assessoria e corretor,
/// usada quando o convite é enviado por e-mail (pré-preenche o aceite).
/// SQL idempotente para tolerar drift de schema entre ambientes.
/// </summary>
[DbContext(typeof(Persistence.AppDbContext))]
[Migration("20260718000000_AddEmailConvidadoVinculos")]
public partial class AddEmailConvidadoVinculos : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            "ALTER TABLE \"VinculosAssessoria\" ADD COLUMN IF NOT EXISTS \"EmailConvidado\" character varying(200);");
        migrationBuilder.Sql(
            "ALTER TABLE \"VinculosCorretor\" ADD COLUMN IF NOT EXISTS \"EmailConvidado\" character varying(200);");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("ALTER TABLE \"VinculosAssessoria\" DROP COLUMN IF EXISTS \"EmailConvidado\";");
        migrationBuilder.Sql("ALTER TABLE \"VinculosCorretor\" DROP COLUMN IF EXISTS \"EmailConvidado\";");
    }
}
