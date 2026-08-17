using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations;

/// <summary>
/// Tarefas de documento (assessor pede ao cliente que anexe um doc). SQL idempotente.
/// </summary>
[DbContext(typeof(Persistence.AppDbContext))]
[Migration("20260816020000_AddTarefasDocumento")]
public partial class AddTarefasDocumento : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE TABLE IF NOT EXISTS "TarefasDocumento" (
                "Id" uuid NOT NULL,
                "AssessorId" uuid NOT NULL,
                "ClienteId" uuid NOT NULL,
                "Titulo" character varying(300) NOT NULL,
                "Descricao" character varying(2000) NULL,
                "Alvo" integer NOT NULL,
                "AlvoId" uuid NULL,
                "Status" integer NOT NULL,
                "CriadoEm" timestamp with time zone NOT NULL,
                "ConcluidaEm" timestamp with time zone NULL,
                CONSTRAINT "PK_TarefasDocumento" PRIMARY KEY ("Id")
            );

            CREATE INDEX IF NOT EXISTS "IX_TarefasDocumento_ClienteId" ON "TarefasDocumento" ("ClienteId");
            CREATE INDEX IF NOT EXISTS "IX_TarefasDocumento_AssessorId_ClienteId" ON "TarefasDocumento" ("AssessorId", "ClienteId");
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""DROP TABLE IF EXISTS "TarefasDocumento";""");
    }
}
