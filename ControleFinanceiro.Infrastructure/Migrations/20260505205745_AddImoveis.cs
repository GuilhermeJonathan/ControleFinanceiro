using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddImoveis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Imoveis",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Pros = table.Column<string>(type: "text", nullable: false),
                    Contras = table.Column<string>(type: "text", nullable: false),
                    Nota = table.Column<int>(type: "integer", nullable: false),
                    DataVisita = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NomeCorretor = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TelefoneCorretor = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Imobiliaria = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Imoveis", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ImovelFotos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ImovelId = table.Column<Guid>(type: "uuid", nullable: false),
                    Dados = table.Column<string>(type: "text", nullable: false),
                    Ordem = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImovelFotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImovelFotos_Imoveis_ImovelId",
                        column: x => x.ImovelId,
                        principalTable: "Imoveis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImovelFotos_ImovelId",
                table: "ImovelFotos",
                column: "ImovelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ImovelFotos");
            migrationBuilder.DropTable(name: "Imoveis");
        }
    }
}
