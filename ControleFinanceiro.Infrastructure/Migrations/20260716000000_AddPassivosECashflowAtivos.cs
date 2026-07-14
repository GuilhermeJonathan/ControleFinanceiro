using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations;

/// <summary>
/// Balanço patrimonial: adiciona a tabela de dívidas/passivos e o fluxo de caixa
/// por bem (ReceitaMensal/DespesaMensal) para consolidar Bens − Dívidas = Patrimônio Líquido.
/// </summary>
[DbContext(typeof(Persistence.AppDbContext))]
[Migration("20260716000000_AddPassivosECashflowAtivos")]
public partial class AddPassivosECashflowAtivos : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Fluxo de caixa por bem (aluguel, dividendos, condomínio, manutenção…)
        migrationBuilder.AddColumn<decimal>(
            name: "ReceitaMensal",
            table: "AtivosPatrimoniais",
            type: "numeric(18,2)",
            precision: 18, scale: 2,
            nullable: false,
            defaultValue: 0m);

        migrationBuilder.AddColumn<decimal>(
            name: "DespesaMensal",
            table: "AtivosPatrimoniais",
            type: "numeric(18,2)",
            precision: 18, scale: 2,
            nullable: false,
            defaultValue: 0m);

        // Dívidas / passivos patrimoniais
        migrationBuilder.CreateTable(
            name: "PassivosPatrimoniais",
            columns: table => new
            {
                Id                = table.Column<Guid>(type: "uuid", nullable: false),
                UsuarioId         = table.Column<Guid>(type: "uuid", nullable: false),
                Nome              = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Moeda             = table.Column<int>(type: "integer", nullable: false),
                Valor             = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                Prazo             = table.Column<int>(type: "integer", nullable: false),
                TaxaJurosAnualPct = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: true),
                PrazoMeses        = table.Column<int>(type: "integer", nullable: true),
                CriadoEm          = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                AtualizadoEm      = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PassivosPatrimoniais", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_PassivosPatrimoniais_UsuarioId",
            table: "PassivosPatrimoniais",
            column: "UsuarioId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "PassivosPatrimoniais");
        migrationBuilder.DropColumn(name: "ReceitaMensal", table: "AtivosPatrimoniais");
        migrationBuilder.DropColumn(name: "DespesaMensal", table: "AtivosPatrimoniais");
    }
}
