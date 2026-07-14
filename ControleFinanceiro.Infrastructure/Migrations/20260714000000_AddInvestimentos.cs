using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations;

/// <summary>
/// Cria a tabela de Investimentos. Idempotente (CREATE TABLE/INDEX IF NOT EXISTS)
/// porque em alguns ambientes a tabela foi criada fora do controle de migrations.
/// </summary>
[DbContext(typeof(Persistence.AppDbContext))]
[Migration("20260714000000_AddInvestimentos")]
public partial class AddInvestimentos : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            CREATE TABLE IF NOT EXISTS ""Investimentos"" (
                ""Id"" uuid NOT NULL,
                ""UsuarioId"" uuid NOT NULL,
                ""Nome"" character varying(200) NOT NULL,
                ""Tipo"" integer NOT NULL,
                ""Moeda"" integer NOT NULL,
                ""Corretora"" character varying(100),
                ""Ticker"" character varying(20),
                ""ValorAplicado"" numeric(18,2) NOT NULL,
                ""ValorAtual"" numeric(18,2) NOT NULL,
                ""RentabilidadeAnualPct"" numeric(9,4),
                ""CriadoEm"" timestamp with time zone NOT NULL,
                ""AtualizadoEm"" timestamp with time zone,
                CONSTRAINT ""PK_Investimentos"" PRIMARY KEY (""Id"")
            );
            CREATE INDEX IF NOT EXISTS ""IX_Investimentos_UsuarioId"" ON ""Investimentos"" (""UsuarioId"");
        ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Investimentos");
    }
}
