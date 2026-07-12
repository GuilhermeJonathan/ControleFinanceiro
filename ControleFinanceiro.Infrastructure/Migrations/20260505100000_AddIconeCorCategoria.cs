using ControleFinanceiro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations
{
    /// <summary>
    /// SQL idempotente + atributos [DbContext]/[Migration] adicionados em 12/07/2026:
    /// esta migration foi criada sem Designer (aplicada manualmente em prod) e o EF
    /// não a descobria. IF NOT EXISTS garante que funcione tanto em banco novo
    /// quanto em prod, onde as colunas já existem.
    /// </summary>
    [DbContext(typeof(AppDbContext))]
    [Migration("20260505100000_AddIconeCorCategoria")]
    public partial class AddIconeCorCategoria : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Categorias" ADD COLUMN IF NOT EXISTS "Icone" character varying(100);
                ALTER TABLE "Categorias" ADD COLUMN IF NOT EXISTS "Cor" character varying(20);
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Categorias" DROP COLUMN IF EXISTS "Icone";
                ALTER TABLE "Categorias" DROP COLUMN IF EXISTS "Cor";
                """);
        }
    }
}
