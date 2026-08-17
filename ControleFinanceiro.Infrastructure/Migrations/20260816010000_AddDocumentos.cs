using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations;

/// <summary>
/// Vault de documentos: metadados dos arquivos anexados (o binário fica no Supabase Storage).
/// SQL idempotente, seguindo o padrão manual do projeto.
/// </summary>
[DbContext(typeof(Persistence.AppDbContext))]
[Migration("20260816010000_AddDocumentos")]
public partial class AddDocumentos : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE TABLE IF NOT EXISTS "Documentos" (
                "Id" uuid NOT NULL,
                "UsuarioId" uuid NOT NULL,
                "Alvo" integer NOT NULL,
                "AlvoId" uuid NULL,
                "Nome" character varying(400) NOT NULL,
                "StoragePath" character varying(1024) NOT NULL,
                "ContentType" character varying(200) NULL,
                "Tamanho" bigint NOT NULL,
                "Categoria" character varying(200) NULL,
                "EnviadoPor" uuid NOT NULL,
                "CriadoEm" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_Documentos" PRIMARY KEY ("Id")
            );

            CREATE INDEX IF NOT EXISTS "IX_Documentos_UsuarioId" ON "Documentos" ("UsuarioId");
            CREATE INDEX IF NOT EXISTS "IX_Documentos_UsuarioId_Alvo_AlvoId" ON "Documentos" ("UsuarioId", "Alvo", "AlvoId");
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""DROP TABLE IF EXISTS "Documentos";""");
    }
}
