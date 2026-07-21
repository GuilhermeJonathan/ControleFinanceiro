using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations;

/// <summary>Posição manual das estruturas no mapa (PosX/PosY). SQL idempotente.</summary>
[DbContext(typeof(Persistence.AppDbContext))]
[Migration("20260724140000_AddPosicaoEstrutura")]
public partial class AddPosicaoEstrutura : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("ALTER TABLE \"Estruturas\" ADD COLUMN IF NOT EXISTS \"PosX\" double precision;");
        migrationBuilder.Sql("ALTER TABLE \"Estruturas\" ADD COLUMN IF NOT EXISTS \"PosY\" double precision;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("ALTER TABLE \"Estruturas\" DROP COLUMN IF EXISTS \"PosX\";");
        migrationBuilder.Sql("ALTER TABLE \"Estruturas\" DROP COLUMN IF EXISTS \"PosY\";");
    }
}
