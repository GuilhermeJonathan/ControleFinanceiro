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
    /// quanto em prod, onde a tabela já existe. A tabela permanece no schema mesmo
    /// com o módulo Imóveis removido do código (dados preservados).
    /// </summary>
    [DbContext(typeof(AppDbContext))]
    [Migration("20260505220000_AddImovelComentarios")]
    public partial class AddImovelComentarios : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS "ImovelComentarios" (
                    "Id" uuid NOT NULL,
                    "ImovelId" uuid NOT NULL,
                    "Texto" character varying(2000) NOT NULL,
                    "CriadoEm" timestamp with time zone NOT NULL,
                    "CreatedAt" timestamp with time zone NOT NULL,
                    "UpdatedAt" timestamp with time zone,
                    CONSTRAINT "PK_ImovelComentarios" PRIMARY KEY ("Id"),
                    CONSTRAINT "FK_ImovelComentarios_Imoveis_ImovelId"
                        FOREIGN KEY ("ImovelId") REFERENCES "Imoveis" ("Id") ON DELETE CASCADE
                );
                CREATE INDEX IF NOT EXISTS "IX_ImovelComentarios_ImovelId"
                    ON "ImovelComentarios" ("ImovelId");
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DROP TABLE IF EXISTS "ImovelComentarios";""");
        }
    }
}
