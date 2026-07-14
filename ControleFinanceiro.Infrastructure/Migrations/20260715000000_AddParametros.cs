using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations;

[DbContext(typeof(Persistence.AppDbContext))]
[Migration("20260715000000_AddParametros")]
public partial class AddParametros : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "TiposAtivoParam",
            columns: table => new
            {
                Id       = table.Column<int>(nullable: false).Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.SerialColumn),
                Nome     = table.Column<string>(maxLength: 100, nullable: false),
                Ordem    = table.Column<int>(nullable: false),
                Ativo    = table.Column<bool>(nullable: false),
                IsSystem = table.Column<bool>(nullable: false),
            },
            constraints: table => table.PrimaryKey("PK_TiposAtivoParam", x => x.Id));

        migrationBuilder.CreateTable(
            name: "TiposInvestimentoParam",
            columns: table => new
            {
                Id       = table.Column<int>(nullable: false).Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.SerialColumn),
                Nome     = table.Column<string>(maxLength: 100, nullable: false),
                Ordem    = table.Column<int>(nullable: false),
                Ativo    = table.Column<bool>(nullable: false),
                IsSystem = table.Column<bool>(nullable: false),
            },
            constraints: table => table.PrimaryKey("PK_TiposInvestimentoParam", x => x.Id));

        migrationBuilder.CreateTable(
            name: "MoedasParam",
            columns: table => new
            {
                Id       = table.Column<int>(nullable: false).Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.SerialColumn),
                Codigo   = table.Column<string>(maxLength: 10, nullable: false),
                Nome     = table.Column<string>(maxLength: 100, nullable: false),
                Ordem    = table.Column<int>(nullable: false),
                Ativo    = table.Column<bool>(nullable: false),
                IsSystem = table.Column<bool>(nullable: false),
            },
            constraints: table => table.PrimaryKey("PK_MoedasParam", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_MoedasParam_Codigo",
            table: "MoedasParam",
            column: "Codigo",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("TiposAtivoParam");
        migrationBuilder.DropTable("TiposInvestimentoParam");
        migrationBuilder.DropTable("MoedasParam");
    }
}
