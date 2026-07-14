using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations;

/// <summary>Marca/identidade da consultoria do assessor (logo, nome, cor, WhatsApp, rodapé).</summary>
[DbContext(typeof(Persistence.AppDbContext))]
[Migration("20260716040000_AddConsultoriaConfig")]
public partial class AddConsultoriaConfig : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ConsultoriaConfigs",
            columns: table => new
            {
                Id              = table.Column<Guid>(type: "uuid", nullable: false),
                UsuarioId       = table.Column<Guid>(type: "uuid", nullable: false),
                NomeConsultoria = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                LogoBase64      = table.Column<string>(type: "text", nullable: true),
                CorMarca        = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                WhatsApp        = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                MensagemRodape  = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                AtualizadoEm    = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ConsultoriaConfigs", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ConsultoriaConfigs_UsuarioId",
            table: "ConsultoriaConfigs",
            column: "UsuarioId",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ConsultoriaConfigs");
    }
}
