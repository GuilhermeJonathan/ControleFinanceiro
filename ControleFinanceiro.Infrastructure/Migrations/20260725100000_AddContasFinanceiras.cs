using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations;

/// <summary>
/// Contas financeiras (bancária/custódia/internacional) + coluna ContaId em Investimentos
/// (investimento vinculado a uma conta de custódia). SQL idempotente.
/// </summary>
[DbContext(typeof(Persistence.AppDbContext))]
[Migration("20260725100000_AddContasFinanceiras")]
public partial class AddContasFinanceiras : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE TABLE IF NOT EXISTS "ContasFinanceiras" (
                "Id"            uuid NOT NULL,
                "UsuarioId"     uuid NOT NULL,
                "Nome"          character varying(200) NOT NULL,
                "Tipo"          integer NOT NULL,
                "Instituicao"   character varying(120),
                "Pais"          character varying(80),
                "Moeda"         integer NOT NULL,
                "Saldo"         numeric(18,2) NOT NULL,
                "Identificador" character varying(120),
                "EstruturaId"   uuid,
                "CriadoEm"      timestamp with time zone NOT NULL,
                "AtualizadoEm"  timestamp with time zone,
                CONSTRAINT "PK_ContasFinanceiras" PRIMARY KEY ("Id")
            );
            """);
        migrationBuilder.Sql("""
            CREATE INDEX IF NOT EXISTS "IX_ContasFinanceiras_UsuarioId" ON "ContasFinanceiras" ("UsuarioId");
            """);
        migrationBuilder.Sql(
            "ALTER TABLE \"Investimentos\" ADD COLUMN IF NOT EXISTS \"ContaId\" uuid;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("ALTER TABLE \"Investimentos\" DROP COLUMN IF EXISTS \"ContaId\";");
        migrationBuilder.Sql("""DROP TABLE IF EXISTS "ContasFinanceiras";""");
    }
}
