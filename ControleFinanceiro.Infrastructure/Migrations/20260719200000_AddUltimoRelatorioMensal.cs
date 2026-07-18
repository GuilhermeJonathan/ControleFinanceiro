using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations;

/// <summary>Controle do envio do relatório/resumo mensal por e-mail. SQL idempotente.</summary>
[DbContext(typeof(Persistence.AppDbContext))]
[Migration("20260719200000_AddUltimoRelatorioMensal")]
public partial class AddUltimoRelatorioMensal : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            "ALTER TABLE \"VinculosAssessoria\" ADD COLUMN IF NOT EXISTS \"UltimoRelatorioMensalEm\" timestamp with time zone;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("ALTER TABLE \"VinculosAssessoria\" DROP COLUMN IF EXISTS \"UltimoRelatorioMensalEm\";");
    }
}
