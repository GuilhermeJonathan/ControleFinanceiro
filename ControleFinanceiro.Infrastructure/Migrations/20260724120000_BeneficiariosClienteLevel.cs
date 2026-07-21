using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations;

/// <summary>
/// Beneficiários passam a ser do CLIENTE (não da estrutura): remove EstruturaId de Beneficiarios;
/// em Distribuicoes a EstruturaId vira opcional (origem). SQL idempotente.
/// </summary>
[DbContext(typeof(Persistence.AppDbContext))]
[Migration("20260724120000_BeneficiariosClienteLevel")]
public partial class BeneficiariosClienteLevel : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_Beneficiarios_EstruturaId\";");
        migrationBuilder.Sql("ALTER TABLE \"Beneficiarios\" DROP COLUMN IF EXISTS \"EstruturaId\";");
        migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS \"IX_Beneficiarios_UsuarioId\" ON \"Beneficiarios\" (\"UsuarioId\");");

        migrationBuilder.Sql("ALTER TABLE \"Distribuicoes\" ALTER COLUMN \"EstruturaId\" DROP NOT NULL;");
        migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_Distribuicoes_EstruturaId\";");
        migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS \"IX_Distribuicoes_UsuarioId\" ON \"Distribuicoes\" (\"UsuarioId\");");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("ALTER TABLE \"Beneficiarios\" ADD COLUMN IF NOT EXISTS \"EstruturaId\" uuid;");
        migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_Beneficiarios_UsuarioId\";");
        migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_Distribuicoes_UsuarioId\";");
    }
}
