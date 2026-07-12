using ControleFinanceiro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations
{
    /// <summary>
    /// SQL idempotente + atributos [DbContext]/[Migration] adicionados em 12/07/2026:
    /// esta migration foi criada sem Designer (aplicada manualmente em prod) e o EF
    /// não a descobria. IF NOT EXISTS garante que funcione tanto em banco novo
    /// quanto em prod, onde as colunas já existem.
    /// </summary>
    [DbContext(typeof(AppDbContext))]
    [Migration("20260505100001_AddMetaContribuicao")]
    public partial class AddMetaContribuicao : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Metas" ADD COLUMN IF NOT EXISTS "ContribuicaoMensalValor" numeric(18,2);
                ALTER TABLE "Metas" ADD COLUMN IF NOT EXISTS "ContribuicaoDia" integer;
                ALTER TABLE "Metas" ADD COLUMN IF NOT EXISTS "UltimaContribuicaoEm" timestamp with time zone;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Metas" DROP COLUMN IF EXISTS "ContribuicaoMensalValor";
                ALTER TABLE "Metas" DROP COLUMN IF EXISTS "ContribuicaoDia";
                ALTER TABLE "Metas" DROP COLUMN IF EXISTS "UltimaContribuicaoEm";
                """);
        }
    }
}
