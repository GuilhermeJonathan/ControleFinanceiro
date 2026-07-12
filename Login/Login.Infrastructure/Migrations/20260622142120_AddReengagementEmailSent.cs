using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Login.Infrastructure.Migrations
{
    /// <summary>
    /// 12/07/2026: removido o AddColumn de PodeVerImoveis que o scaffold incluiu
    /// por engano (o snapshot da época não enxergava a migration AddPodeVerImoveis,
    /// que não tinha Designer). A coluna agora é criada pela própria AddPodeVerImoveis.
    /// </summary>
    public partial class AddReengagementEmailSent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "ReengagementEmailSent" boolean NOT NULL DEFAULT FALSE;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Users" DROP COLUMN IF EXISTS "ReengagementEmailSent";
                """);
        }
    }
}
