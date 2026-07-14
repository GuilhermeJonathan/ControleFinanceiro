using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations;

/// <summary>
/// Cotação (em BRL) por moeda, editável pelo assessor em Cadastros → Moedas.
/// A consolidação patrimonial passa a usar esse valor no lugar do câmbio chumbado.
/// Semeia as moedas existentes com as taxas que eram fixas no código.
/// </summary>
[DbContext(typeof(Persistence.AppDbContext))]
[Migration("20260716020000_AddCotacaoMoedaParam")]
public partial class AddCotacaoMoedaParam : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<decimal>(
            name: "CotacaoBRL",
            table: "MoedasParam",
            type: "numeric(18,6)",
            precision: 18, scale: 6,
            nullable: false,
            defaultValue: 1m);

        // Semeia com as taxas que eram o FxStub (evita zerar patrimônio em moeda estrangeira)
        migrationBuilder.Sql(@"
            UPDATE ""MoedasParam"" SET ""CotacaoBRL"" = 1.00     WHERE ""Codigo"" = 'BRL';
            UPDATE ""MoedasParam"" SET ""CotacaoBRL"" = 5.40     WHERE ""Codigo"" = 'USD';
            UPDATE ""MoedasParam"" SET ""CotacaoBRL"" = 5.90     WHERE ""Codigo"" = 'EUR';
            UPDATE ""MoedasParam"" SET ""CotacaoBRL"" = 6.10     WHERE ""Codigo"" = 'CHF';
            UPDATE ""MoedasParam"" SET ""CotacaoBRL"" = 6.90     WHERE ""Codigo"" = 'GBP';
        ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "CotacaoBRL", table: "MoedasParam");
    }
}
