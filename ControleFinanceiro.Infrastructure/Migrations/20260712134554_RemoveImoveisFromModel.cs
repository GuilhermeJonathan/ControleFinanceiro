using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations
{
    /// <summary>
    /// Remove as entidades de Imóveis do modelo EF (código do módulo foi excluído).
    /// Up/Down intencionalmente vazios: as tabelas Imoveis, ImovelFotos e
    /// ImovelComentarios são PRESERVADAS no banco com os dados existentes —
    /// decisão de 12/07/2026. Esta migration só sincroniza o model snapshot.
    /// </summary>
    public partial class RemoveImoveisFromModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
