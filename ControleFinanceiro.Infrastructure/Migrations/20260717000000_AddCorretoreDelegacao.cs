using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations;

/// <inheritdoc />
[DbContext(typeof(ControleFinanceiro.Infrastructure.Persistence.AppDbContext))]
[Migration("20260717000000_AddCorretoreDelegacao")]
public partial class AddCorretoreDelegacao : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "VinculosCorretor",
            columns: table => new
            {
                Id            = table.Column<Guid>(nullable: false),
                AssessorId    = table.Column<Guid>(nullable: false),
                CorretorId    = table.Column<Guid>(nullable: false),
                CodigoConvite = table.Column<string>(maxLength: 10, nullable: false),
                CriadoEm     = table.Column<DateTime>(nullable: false),
                AceitoEm     = table.Column<DateTime>(nullable: true),
                RevogadoEm   = table.Column<DateTime>(nullable: true),
                NomeCorretor  = table.Column<string>(maxLength: 200, nullable: true),
                NomeAssessor  = table.Column<string>(maxLength: 200, nullable: true),
            },
            constraints: table => table.PrimaryKey("PK_VinculosCorretor", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_VinculosCorretor_CodigoConvite",
            table: "VinculosCorretor",
            column: "CodigoConvite",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_VinculosCorretor_AssessorId",
            table: "VinculosCorretor",
            column: "AssessorId");

        migrationBuilder.CreateIndex(
            name: "IX_VinculosCorretor_CorretorId",
            table: "VinculosCorretor",
            column: "CorretorId");

        migrationBuilder.CreateTable(
            name: "DelegacoesCarteira",
            columns: table => new
            {
                Id                  = table.Column<Guid>(nullable: false),
                AssessorId          = table.Column<Guid>(nullable: false),
                CorretorId          = table.Column<Guid>(nullable: false),
                VinculoAssessoriaId = table.Column<Guid>(nullable: false),
                ClienteId           = table.Column<Guid>(nullable: false),
                NomeCliente         = table.Column<string>(maxLength: 200, nullable: true),
                NomeCorretor        = table.Column<string>(maxLength: 200, nullable: true),
                DelegadoEm          = table.Column<DateTime>(nullable: false),
                RevogadoEm          = table.Column<DateTime>(nullable: true),
            },
            constraints: table => table.PrimaryKey("PK_DelegacoesCarteira", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_DelegacoesCarteira_AssessorId",
            table: "DelegacoesCarteira",
            column: "AssessorId");

        migrationBuilder.CreateIndex(
            name: "IX_DelegacoesCarteira_CorretorId",
            table: "DelegacoesCarteira",
            column: "CorretorId");

        migrationBuilder.CreateIndex(
            name: "IX_DelegacoesCarteira_CorretorId_ClienteId",
            table: "DelegacoesCarteira",
            columns: ["CorretorId", "ClienteId"]);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "DelegacoesCarteira");
        migrationBuilder.DropTable(name: "VinculosCorretor");
    }
}
