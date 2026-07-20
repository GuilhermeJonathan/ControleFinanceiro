using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations;

/// <summary>Permite vários planos por cliente: troca o índice único de UsuarioId por não-único. Idempotente.</summary>
[DbContext(typeof(Persistence.AppDbContext))]
[Migration("20260720100000_PlanoAcaoMultiplos")]
public partial class PlanoAcaoMultiplos : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_PlanosAcao_UsuarioId\";");
        migrationBuilder.Sql("""
            CREATE INDEX IF NOT EXISTS "IX_PlanosAcao_UsuarioId"
                ON "PlanosAcao" ("UsuarioId");
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_PlanosAcao_UsuarioId\";");
        migrationBuilder.Sql("""
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_PlanosAcao_UsuarioId"
                ON "PlanosAcao" ("UsuarioId");
            """);
    }
}
