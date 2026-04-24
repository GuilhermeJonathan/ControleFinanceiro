using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCartaoVencimentoETipoReceita : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "QuantidadeHoras",
                table: "ReceitasRecorrentes",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Tipo",
                table: "ReceitasRecorrentes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorHora",
                table: "ReceitasRecorrentes",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuantidadeHoras",
                table: "ReceitasRecorrentes");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "ReceitasRecorrentes");

            migrationBuilder.DropColumn(
                name: "ValorHora",
                table: "ReceitasRecorrentes");
        }
    }
}
