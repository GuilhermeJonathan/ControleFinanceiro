using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations;

/// <summary>Alocação-alvo (% por classe de investimento) por usuário. SQL idempotente.</summary>
[DbContext(typeof(Persistence.AppDbContext))]
[Migration("20260719100000_AddAlocacaoAlvo")]
public partial class AddAlocacaoAlvo : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE TABLE IF NOT EXISTS "AlocacoesAlvo" (
                "Id" uuid NOT NULL,
                "UsuarioId" uuid NOT NULL,
                "Tipo" integer NOT NULL,
                "PercentualAlvo" numeric(5,2) NOT NULL,
                CONSTRAINT "PK_AlocacoesAlvo" PRIMARY KEY ("Id")
            );
            """);
        migrationBuilder.Sql("""
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_AlocacoesAlvo_UsuarioId_Tipo"
                ON "AlocacoesAlvo" ("UsuarioId", "Tipo");
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP TABLE IF EXISTS \"AlocacoesAlvo\";");
    }
}
