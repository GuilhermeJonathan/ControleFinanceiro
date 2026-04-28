using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVinculoFamiliar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VinculosFamiliares",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DonoId = table.Column<Guid>(type: "uuid", nullable: false),
                    MembroId = table.Column<Guid>(type: "uuid", nullable: false),
                    CodigoConvite = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AceitoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NomeMembro = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VinculosFamiliares", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VinculosFamiliares_CodigoConvite",
                table: "VinculosFamiliares",
                column: "CodigoConvite",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VinculosFamiliares_MembroId",
                table: "VinculosFamiliares",
                column: "MembroId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VinculosFamiliares");
        }
    }
}
