using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations;

/// <summary>Indicadores de sucessão (governança/conformidade) por cliente. SQL idempotente.</summary>
[DbContext(typeof(Persistence.AppDbContext))]
[Migration("20260725300000_AddIndicadoresSucessao")]
public partial class AddIndicadoresSucessao : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE TABLE IF NOT EXISTS "IndicadoresSucessao" (
                "Id"                uuid NOT NULL,
                "UsuarioId"         uuid NOT NULL,
                "GovernancaScore"   integer,
                "ConformidadeScore" integer,
                "CriadoEm"          timestamp with time zone NOT NULL,
                "AtualizadoEm"      timestamp with time zone,
                CONSTRAINT "PK_IndicadoresSucessao" PRIMARY KEY ("Id")
            );
            """);
        migrationBuilder.Sql("""
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_IndicadoresSucessao_UsuarioId" ON "IndicadoresSucessao" ("UsuarioId");
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""DROP TABLE IF EXISTS "IndicadoresSucessao";""");
    }
}
