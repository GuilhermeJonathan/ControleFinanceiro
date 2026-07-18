using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations;

/// <summary>Foto mensal do patrimônio por usuário — base do gráfico de evolução. SQL idempotente.</summary>
[DbContext(typeof(Persistence.AppDbContext))]
[Migration("20260719000000_AddPatrimonioSnapshot")]
public partial class AddPatrimonioSnapshot : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE TABLE IF NOT EXISTS "PatrimonioSnapshots" (
                "Id" uuid NOT NULL,
                "UsuarioId" uuid NOT NULL,
                "Ano" integer NOT NULL,
                "Mes" integer NOT NULL,
                "PatrimonioLiquidoBRL" numeric(18,2) NOT NULL,
                "TotalBensBRL" numeric(18,2) NOT NULL,
                "TotalDividasBRL" numeric(18,2) NOT NULL,
                "AtualizadoEm" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_PatrimonioSnapshots" PRIMARY KEY ("Id")
            );
            """);
        migrationBuilder.Sql("""
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_PatrimonioSnapshots_UsuarioId_Ano_Mes"
                ON "PatrimonioSnapshots" ("UsuarioId", "Ano", "Mes");
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP TABLE IF EXISTS \"PatrimonioSnapshots\";");
    }
}
