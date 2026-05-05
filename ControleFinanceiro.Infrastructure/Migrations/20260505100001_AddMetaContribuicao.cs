using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMetaContribuicao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ContribuicaoMensalValor",
                table: "Metas",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ContribuicaoDia",
                table: "Metas",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimaContribuicaoEm",
                table: "Metas",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContribuicaoMensalValor",
                table: "Metas");

            migrationBuilder.DropColumn(
                name: "ContribuicaoDia",
                table: "Metas");

            migrationBuilder.DropColumn(
                name: "UltimaContribuicaoEm",
                table: "Metas");
        }
    }
}
