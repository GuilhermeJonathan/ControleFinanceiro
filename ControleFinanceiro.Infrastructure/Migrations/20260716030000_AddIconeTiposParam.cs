using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations;

/// <summary>
/// Adiciona a coluna Icone a TiposAtivoParam e TiposInvestimentoParam.
/// A entidade/config já usavam Icone, mas nenhuma migration criava a coluna
/// (a AddParametros criou as tabelas sem ela) — causava 500 ao listar em prod.
/// Idempotente (ADD COLUMN IF NOT EXISTS) para não quebrar ambientes onde já existe.
/// </summary>
[DbContext(typeof(Persistence.AppDbContext))]
[Migration("20260716030000_AddIconeTiposParam")]
public partial class AddIconeTiposParam : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            ALTER TABLE ""TiposAtivoParam""        ADD COLUMN IF NOT EXISTS ""Icone"" character varying(10);
            ALTER TABLE ""TiposInvestimentoParam"" ADD COLUMN IF NOT EXISTS ""Icone"" character varying(10);
        ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            ALTER TABLE ""TiposAtivoParam""        DROP COLUMN IF EXISTS ""Icone"";
            ALTER TABLE ""TiposInvestimentoParam"" DROP COLUMN IF EXISTS ""Icone"";
        ");
    }
}
