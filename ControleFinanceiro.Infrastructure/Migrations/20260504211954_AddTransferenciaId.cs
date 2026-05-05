using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTransferenciaId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TransferenciaId",
                table: "Lancamentos",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lancamentos_TransferenciaId",
                table: "Lancamentos",
                column: "TransferenciaId",
                filter: "\"TransferenciaId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Lancamentos_TransferenciaId",
                table: "Lancamentos");

            migrationBuilder.DropColumn(
                name: "TransferenciaId",
                table: "Lancamentos");
        }
    }
}
