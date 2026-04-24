using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCartaoParcelasToLancamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CartaoId",
                table: "Lancamentos",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GrupoParcelas",
                table: "Lancamentos",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParcelaAtual",
                table: "Lancamentos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalParcelas",
                table: "Lancamentos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lancamentos_CartaoId",
                table: "Lancamentos",
                column: "CartaoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lancamentos_CartoesCredito_CartaoId",
                table: "Lancamentos",
                column: "CartaoId",
                principalTable: "CartoesCredito",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lancamentos_CartoesCredito_CartaoId",
                table: "Lancamentos");

            migrationBuilder.DropIndex(
                name: "IX_Lancamentos_CartaoId",
                table: "Lancamentos");

            migrationBuilder.DropColumn(
                name: "CartaoId",
                table: "Lancamentos");

            migrationBuilder.DropColumn(
                name: "GrupoParcelas",
                table: "Lancamentos");

            migrationBuilder.DropColumn(
                name: "ParcelaAtual",
                table: "Lancamentos");

            migrationBuilder.DropColumn(
                name: "TotalParcelas",
                table: "Lancamentos");
        }
    }
}
