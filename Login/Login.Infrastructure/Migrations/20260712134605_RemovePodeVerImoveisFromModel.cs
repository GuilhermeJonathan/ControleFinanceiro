using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Login.Infrastructure.Migrations
{
    /// <summary>
    /// Remove PodeVerImoveis do modelo EF (módulo Imóveis foi excluído).
    /// Up/Down intencionalmente vazios: a coluna Users.PodeVerImoveis é
    /// PRESERVADA no banco — decisão de 12/07/2026. Esta migration só
    /// sincroniza o model snapshot.
    /// </summary>
    public partial class RemovePodeVerImoveisFromModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
