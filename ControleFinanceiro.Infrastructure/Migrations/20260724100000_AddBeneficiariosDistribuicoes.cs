using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations;

/// <summary>F2: Beneficiarios + Distribuicoes de uma estrutura (lente Trust). SQL idempotente.</summary>
[DbContext(typeof(Persistence.AppDbContext))]
[Migration("20260724100000_AddBeneficiariosDistribuicoes")]
public partial class AddBeneficiariosDistribuicoes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE TABLE IF NOT EXISTS "Beneficiarios" (
                "Id"                     uuid NOT NULL,
                "UsuarioId"              uuid NOT NULL,
                "EstruturaId"            uuid NOT NULL,
                "Nome"                   character varying(200) NOT NULL,
                "Papel"                  integer NOT NULL,
                "PercentualDistribuicao" numeric(9,4) NOT NULL,
                "CondicaoLiberacao"      character varying(500),
                "CriadoEm"               timestamp with time zone NOT NULL,
                "AtualizadoEm"           timestamp with time zone,
                CONSTRAINT "PK_Beneficiarios" PRIMARY KEY ("Id")
            );
            """);
        migrationBuilder.Sql("""
            CREATE INDEX IF NOT EXISTS "IX_Beneficiarios_EstruturaId" ON "Beneficiarios" ("EstruturaId");
            """);
        migrationBuilder.Sql("""
            CREATE TABLE IF NOT EXISTS "Distribuicoes" (
                "Id"             uuid NOT NULL,
                "UsuarioId"      uuid NOT NULL,
                "EstruturaId"    uuid NOT NULL,
                "Data"           timestamp with time zone NOT NULL,
                "Valor"          numeric(18,2) NOT NULL,
                "Moeda"          integer NOT NULL,
                "BeneficiarioId" uuid,
                "Descricao"      character varying(500),
                "CriadoEm"       timestamp with time zone NOT NULL,
                CONSTRAINT "PK_Distribuicoes" PRIMARY KEY ("Id")
            );
            """);
        migrationBuilder.Sql("""
            CREATE INDEX IF NOT EXISTS "IX_Distribuicoes_EstruturaId" ON "Distribuicoes" ("EstruturaId");
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""DROP TABLE IF EXISTS "Distribuicoes";""");
        migrationBuilder.Sql("""DROP TABLE IF EXISTS "Beneficiarios";""");
    }
}
