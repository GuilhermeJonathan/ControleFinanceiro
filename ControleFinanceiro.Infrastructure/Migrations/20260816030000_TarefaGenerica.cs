using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations;

/// <summary>
/// Generaliza TarefasDocumento: vira tarefa genérica (assessor→cliente). Remove o alvo
/// obrigatório de documento e adiciona AtalhoRota (deep-link opcional). SQL idempotente.
/// </summary>
[DbContext(typeof(Persistence.AppDbContext))]
[Migration("20260816030000_TarefaGenerica")]
public partial class TarefaGenerica : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            ALTER TABLE "TarefasDocumento" ADD COLUMN IF NOT EXISTS "AtalhoRota" character varying(60);
            ALTER TABLE "TarefasDocumento" DROP COLUMN IF EXISTS "Alvo";
            ALTER TABLE "TarefasDocumento" DROP COLUMN IF EXISTS "AlvoId";
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            ALTER TABLE "TarefasDocumento" ADD COLUMN IF NOT EXISTS "Alvo" integer NOT NULL DEFAULT 1;
            ALTER TABLE "TarefasDocumento" ADD COLUMN IF NOT EXISTS "AlvoId" uuid;
            ALTER TABLE "TarefasDocumento" DROP COLUMN IF EXISTS "AtalhoRota";
            """);
    }
}
