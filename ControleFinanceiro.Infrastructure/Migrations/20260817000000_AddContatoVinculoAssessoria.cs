using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations;

/// <summary>
/// Dados de contato que o assessor mantém do cliente: Telefone (WhatsApp) e Observacoes
/// (nota interna). Não tocam nos dados de login do cliente. SQL idempotente.
/// </summary>
[DbContext(typeof(Persistence.AppDbContext))]
[Migration("20260817000000_AddContatoVinculoAssessoria")]
public partial class AddContatoVinculoAssessoria : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            ALTER TABLE "VinculosAssessoria" ADD COLUMN IF NOT EXISTS "Telefone" character varying(40);
            ALTER TABLE "VinculosAssessoria" ADD COLUMN IF NOT EXISTS "Observacoes" character varying(2000);
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            ALTER TABLE "VinculosAssessoria" DROP COLUMN IF EXISTS "Telefone";
            ALTER TABLE "VinculosAssessoria" DROP COLUMN IF EXISTS "Observacoes";
            """);
    }
}
