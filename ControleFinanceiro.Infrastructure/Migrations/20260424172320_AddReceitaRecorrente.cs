using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReceitaRecorrente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ReceitaRecorrenteId",
                table: "Lancamentos",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ReceitasRecorrentes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Dia = table.Column<int>(type: "int", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceitasRecorrentes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lancamentos_ReceitaRecorrenteId",
                table: "Lancamentos",
                column: "ReceitaRecorrenteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lancamentos_ReceitasRecorrentes_ReceitaRecorrenteId",
                table: "Lancamentos",
                column: "ReceitaRecorrenteId",
                principalTable: "ReceitasRecorrentes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lancamentos_ReceitasRecorrentes_ReceitaRecorrenteId",
                table: "Lancamentos");

            migrationBuilder.DropTable(
                name: "ReceitasRecorrentes");

            migrationBuilder.DropIndex(
                name: "IX_Lancamentos_ReceitaRecorrenteId",
                table: "Lancamentos");

            migrationBuilder.DropColumn(
                name: "ReceitaRecorrenteId",
                table: "Lancamentos");
        }
    }
}
