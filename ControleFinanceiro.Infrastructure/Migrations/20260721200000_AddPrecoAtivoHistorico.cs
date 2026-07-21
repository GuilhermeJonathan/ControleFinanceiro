using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations;

/// <summary>Histórico de preços de ativos + coluna ValorAtualizadoEm em Investimentos. SQL idempotente.</summary>
[DbContext(typeof(Persistence.AppDbContext))]
[Migration("20260721200000_AddPrecoAtivoHistorico")]
public partial class AddPrecoAtivoHistorico : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE TABLE IF NOT EXISTS "PrecosAtivoHistorico" (
                "Id"       serial NOT NULL,
                "Ticker"   character varying(20) NOT NULL,
                "Preco"    numeric(18,6) NOT NULL,
                "Fonte"    character varying(50) NOT NULL,
                "DataHora" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_PrecosAtivoHistorico" PRIMARY KEY ("Id")
            );
            """);
        migrationBuilder.Sql("""
            CREATE INDEX IF NOT EXISTS "IX_PrecosAtivoHistorico_Ticker_DataHora"
                ON "PrecosAtivoHistorico" ("Ticker", "DataHora");
            """);
        migrationBuilder.Sql(
            "ALTER TABLE \"Investimentos\" ADD COLUMN IF NOT EXISTS \"ValorAtualizadoEm\" timestamp with time zone;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""DROP TABLE IF EXISTS "PrecosAtivoHistorico";""");
        migrationBuilder.Sql("ALTER TABLE \"Investimentos\" DROP COLUMN IF EXISTS \"ValorAtualizadoEm\";");
    }
}
