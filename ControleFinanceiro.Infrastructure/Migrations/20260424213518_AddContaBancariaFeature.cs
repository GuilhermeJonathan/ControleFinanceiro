using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContaBancariaFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SaldosContas_Banco",
                table: "SaldosContas");

            migrationBuilder.AddColumn<int>(
                name: "Tipo",
                table: "SaldosContas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ContaBancariaId",
                table: "Lancamentos",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lancamentos_ContaBancariaId",
                table: "Lancamentos",
                column: "ContaBancariaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lancamentos_SaldosContas_ContaBancariaId",
                table: "Lancamentos",
                column: "ContaBancariaId",
                principalTable: "SaldosContas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lancamentos_SaldosContas_ContaBancariaId",
                table: "Lancamentos");

            migrationBuilder.DropIndex(
                name: "IX_Lancamentos_ContaBancariaId",
                table: "Lancamentos");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "SaldosContas");

            migrationBuilder.DropColumn(
                name: "ContaBancariaId",
                table: "Lancamentos");

            migrationBuilder.CreateIndex(
                name: "IX_SaldosContas_Banco",
                table: "SaldosContas",
                column: "Banco",
                unique: true);
        }
    }
}
