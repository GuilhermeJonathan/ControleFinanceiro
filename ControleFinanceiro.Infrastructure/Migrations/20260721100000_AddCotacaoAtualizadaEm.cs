using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations;

/// <summary>Adiciona CotacaoAtualizadaEm em MoedasParam — registra quando o job atualizou a cotação. Idempotente.</summary>
[DbContext(typeof(Persistence.AppDbContext))]
[Migration("20260721100000_AddCotacaoAtualizadaEm")]
public partial class AddCotacaoAtualizadaEm : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            "ALTER TABLE \"MoedasParam\" ADD COLUMN IF NOT EXISTS \"CotacaoAtualizadaEm\" timestamp with time zone;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            "ALTER TABLE \"MoedasParam\" DROP COLUMN IF EXISTS \"CotacaoAtualizadaEm\";");
    }
}
