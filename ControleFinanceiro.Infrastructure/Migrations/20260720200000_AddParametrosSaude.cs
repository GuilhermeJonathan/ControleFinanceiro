using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations;

/// <summary>Parâmetros do termômetro de saúde por assessor. SQL idempotente.</summary>
[DbContext(typeof(Persistence.AppDbContext))]
[Migration("20260720200000_AddParametrosSaude")]
public partial class AddParametrosSaude : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE TABLE IF NOT EXISTS "ParametrosSaude" (
                "Id" uuid NOT NULL,
                "AssessorId" uuid NOT NULL,
                "ScoreExcelenteMin" integer NOT NULL,
                "ScoreBoaMin" integer NOT NULL,
                "ScoreAtencaoMin" integer NOT NULL,
                "ComprometimentoSaudavelMax" integer NOT NULL,
                "ComprometimentoRazoavelMax" integer NOT NULL,
                "ComprometimentoApertadoMax" integer NOT NULL,
                "ReservaExcelenteMinDias" integer NOT NULL,
                "ReservaBoaMinDias" integer NOT NULL,
                "ReservaCurtaMinDias" integer NOT NULL,
                CONSTRAINT "PK_ParametrosSaude" PRIMARY KEY ("Id")
            );
            """);
        migrationBuilder.Sql("""
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_ParametrosSaude_AssessorId"
                ON "ParametrosSaude" ("AssessorId");
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP TABLE IF EXISTS \"ParametrosSaude\";");
    }
}
