using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVinculosAssessoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VinculosAssessoria",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssessorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uuid", nullable: false),
                    CodigoConvite = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AceitoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevogadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NomeCliente = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    NomeAssessor = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VinculosAssessoria", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VinculosAssessoria_AssessorId",
                table: "VinculosAssessoria",
                column: "AssessorId");

            migrationBuilder.CreateIndex(
                name: "IX_VinculosAssessoria_ClienteId",
                table: "VinculosAssessoria",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_VinculosAssessoria_CodigoConvite",
                table: "VinculosAssessoria",
                column: "CodigoConvite",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VinculosAssessoria");
        }
    }
}
