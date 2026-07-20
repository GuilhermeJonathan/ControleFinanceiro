using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations;

/// <summary>Histórico de cotações de moedas — registrado pelo job diário de câmbio. SQL idempotente.</summary>
[DbContext(typeof(Persistence.AppDbContext))]
[Migration("20260721000000_AddCotacaoHistorico")]
public partial class AddCotacaoHistorico : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE TABLE IF NOT EXISTS "CotacoesHistorico" (
                "Id"          serial NOT NULL,
                "MoedaCodigo" character varying(10) NOT NULL,
                "CotacaoBRL"  numeric(18,6) NOT NULL,
                "Fonte"       character varying(50) NOT NULL,
                "DataHora"    timestamp with time zone NOT NULL,
                CONSTRAINT "PK_CotacoesHistorico" PRIMARY KEY ("Id")
            );
            """);
        migrationBuilder.Sql("""
            CREATE INDEX IF NOT EXISTS "IX_CotacoesHistorico_MoedaCodigo_DataHora"
                ON "CotacoesHistorico" ("MoedaCodigo", "DataHora");
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""DROP TABLE IF EXISTS "CotacoesHistorico";""");
    }
}
