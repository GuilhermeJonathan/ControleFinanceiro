using Login.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Login.Infrastructure.Migrations
{
    /// <summary>
    /// SQL idempotente + atributos [DbContext]/[Migration] adicionados em 12/07/2026:
    /// esta migration foi criada sem Designer (aplicada manualmente em prod) e o EF
    /// não a descobria. IF NOT EXISTS garante que funcione tanto em banco novo
    /// quanto em prod, onde a coluna já existe. A coluna permanece no schema mesmo
    /// com o módulo Imóveis removido do código (dados preservados).
    /// </summary>
    [DbContext(typeof(AppDbContext))]
    [Migration("20260505230000_AddPodeVerImoveis")]
    public partial class AddPodeVerImoveis : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "PodeVerImoveis" boolean NOT NULL DEFAULT FALSE;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Users" DROP COLUMN IF EXISTS "PodeVerImoveis";
                """);
        }
    }
}
