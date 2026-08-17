using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations;

/// <summary>
/// Vínculo direto de bem → membro da família (Beneficiário) quando não há estrutura.
/// Adiciona "BeneficiarioId" (nullable, sem FK — mesmo padrão do EstruturaId) em
/// AtivosPatrimoniais, Investimentos e ContasFinanceiras. SQL idempotente.
/// </summary>
[DbContext(typeof(Persistence.AppDbContext))]
[Migration("20260816000000_AddBeneficiarioIdItens")]
public partial class AddBeneficiarioIdItens : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            ALTER TABLE "AtivosPatrimoniais" ADD COLUMN IF NOT EXISTS "BeneficiarioId" uuid;
            ALTER TABLE "Investimentos"      ADD COLUMN IF NOT EXISTS "BeneficiarioId" uuid;
            ALTER TABLE "ContasFinanceiras"  ADD COLUMN IF NOT EXISTS "BeneficiarioId" uuid;

            CREATE INDEX IF NOT EXISTS "IX_AtivosPatrimoniais_BeneficiarioId" ON "AtivosPatrimoniais" ("BeneficiarioId");
            CREATE INDEX IF NOT EXISTS "IX_Investimentos_BeneficiarioId"      ON "Investimentos" ("BeneficiarioId");
            CREATE INDEX IF NOT EXISTS "IX_ContasFinanceiras_BeneficiarioId"  ON "ContasFinanceiras" ("BeneficiarioId");
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP INDEX IF EXISTS "IX_AtivosPatrimoniais_BeneficiarioId";
            DROP INDEX IF EXISTS "IX_Investimentos_BeneficiarioId";
            DROP INDEX IF EXISTS "IX_ContasFinanceiras_BeneficiarioId";

            ALTER TABLE "AtivosPatrimoniais" DROP COLUMN IF EXISTS "BeneficiarioId";
            ALTER TABLE "Investimentos"      DROP COLUMN IF EXISTS "BeneficiarioId";
            ALTER TABLE "ContasFinanceiras"  DROP COLUMN IF EXISTS "BeneficiarioId";
            """);
    }
}
