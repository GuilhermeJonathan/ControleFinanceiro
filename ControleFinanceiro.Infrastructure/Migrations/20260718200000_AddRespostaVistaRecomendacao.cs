using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations;

/// <summary>
/// Adiciona RespostaVistaEm às recomendações — marca quando o assessor visualizou a
/// resposta do cliente (badge do sino de notificações). SQL idempotente.
/// </summary>
[DbContext(typeof(Persistence.AppDbContext))]
[Migration("20260718200000_AddRespostaVistaRecomendacao")]
public partial class AddRespostaVistaRecomendacao : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            "ALTER TABLE \"Recomendacoes\" ADD COLUMN IF NOT EXISTS \"RespostaVistaEm\" timestamp with time zone;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("ALTER TABLE \"Recomendacoes\" DROP COLUMN IF EXISTS \"RespostaVistaEm\";");
    }
}
