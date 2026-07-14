using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations;

/// <summary>
/// Proteção patrimonial: simulações de projeção de longo prazo e seus cenários
/// (aportes/resgates extraordinários). Cenarios é owned (tabela CenariosSimulacao).
/// </summary>
[DbContext(typeof(Persistence.AppDbContext))]
[Migration("20260716010000_AddSimulacoesPatrimoniais")]
public partial class AddSimulacoesPatrimoniais : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "SimulacoesPatrimoniais",
            columns: table => new
            {
                Id                      = table.Column<Guid>(type: "uuid", nullable: false),
                UsuarioId               = table.Column<Guid>(type: "uuid", nullable: false),
                Nome                    = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Favorita                = table.Column<bool>(type: "boolean", nullable: false),
                IdadeAtual              = table.Column<int>(type: "integer", nullable: false),
                IdadeAlvo               = table.Column<int>(type: "integer", nullable: false),
                PatrimonioInicial       = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                ModoAutomatico          = table.Column<bool>(type: "boolean", nullable: false),
                AporteMensal            = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                TaxaRetornoRealAnualPct = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: false),
                RetiradaMensal          = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                CriadoEm                = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                AtualizadoEm            = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SimulacoesPatrimoniais", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_SimulacoesPatrimoniais_UsuarioId",
            table: "SimulacoesPatrimoniais",
            column: "UsuarioId");

        migrationBuilder.CreateTable(
            name: "CenariosSimulacao",
            columns: table => new
            {
                Id          = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                SimulacaoId = table.Column<Guid>(type: "uuid", nullable: false),
                Nome        = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Tipo        = table.Column<int>(type: "integer", nullable: false),
                Valor       = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                IdadeInicio = table.Column<int>(type: "integer", nullable: false),
                IdadeFim    = table.Column<int>(type: "integer", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CenariosSimulacao", x => x.Id);
                table.ForeignKey(
                    name: "FK_CenariosSimulacao_SimulacoesPatrimoniais_SimulacaoId",
                    column: x => x.SimulacaoId,
                    principalTable: "SimulacoesPatrimoniais",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_CenariosSimulacao_SimulacaoId",
            table: "CenariosSimulacao",
            column: "SimulacaoId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "CenariosSimulacao");
        migrationBuilder.DropTable(name: "SimulacoesPatrimoniais");
    }
}
