using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Api.BackgroundServices;

/// <summary>
/// Serviço que roda diariamente à meia-noite assim que a API sobe.
/// Jobs atuais:
///   1. Auto-vencimento  — marca como Vencido os lançamentos A Vencer com data passada.
/// </summary>
public class DailyJobService(
    IServiceScopeFactory scopeFactory,
    ILogger<DailyJobService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        // Roda uma vez na subida (garante consistência caso a API ficou offline)
        await ExecutarJobsAsync(ct);

        while (!ct.IsCancellationRequested)
        {
            var agora = DateTime.Now;
            var proximaMeiaNoite = agora.Date.AddDays(1); // próximo 00:00:00
            var delay = proximaMeiaNoite - agora;

            logger.LogInformation("[DailyJob] Próxima execução em {horas:F1}h ({hora})",
                delay.TotalHours, proximaMeiaNoite);

            try { await Task.Delay(delay, ct); }
            catch (OperationCanceledException) { break; }

            await ExecutarJobsAsync(ct);
        }
    }

    private async Task ExecutarJobsAsync(CancellationToken ct)
    {
        logger.LogInformation("[DailyJob] Iniciando jobs diários — {agora}", DateTime.Now);

        await JobAutoVencimentoAsync(ct);

        logger.LogInformation("[DailyJob] Jobs concluídos — {agora}", DateTime.Now);
    }

    // ── Job 1: Auto-vencimento ────────────────────────────────────────────────
    // Lançamentos "A Vencer" cuja data já passou → Vencido
    private async Task JobAutoVencimentoAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var hoje = DateTime.Today;

        var vencidos = await db.Lancamentos
            .Where(l => l.Situacao == SituacaoLancamento.AVencer && l.Data.Date < hoje)
            .ToListAsync(ct);

        if (vencidos.Count == 0)
        {
            logger.LogInformation("[DailyJob] Auto-vencimento: nenhum lançamento a atualizar.");
            return;
        }

        foreach (var l in vencidos)
            l.AtualizarSituacao(SituacaoLancamento.Vencido);

        await db.SaveChangesAsync(ct);

        logger.LogInformation("[DailyJob] Auto-vencimento: {qtd} lançamento(s) marcados como Vencido " +
            "({usuarios} usuário(s)).",
            vencidos.Count,
            vencidos.Select(l => l.UsuarioId).Distinct().Count());
    }
}
