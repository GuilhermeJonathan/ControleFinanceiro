using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Api.BackgroundServices;

/// <summary>
/// Serviço que roda diariamente à meia-noite assim que a API sobe.
/// Jobs atuais:
///   1. Auto-vencimento  — marca como Vencido os lançamentos A Vencer com data passada.
///   2. Gerar recorrentes — estende grupos recorrentes para sempre ter 24 meses à frente.
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
        await JobGerarRecorrentesAsync(ct);

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

    // ── Job 2: Geração de recorrentes ─────────────────────────────────────────
    // Para cada grupo com IsRecorrente=true, garante que sempre haja lançamentos
    // gerados até 24 meses à frente. Se sobrar menos de 12 meses, estende.
    private async Task JobGerarRecorrentesAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var hoje = DateTime.Today;
        // Gera até 24 meses à frente
        var horizonte   = hoje.AddMonths(24);
        var limiteInt   = horizonte.Year * 100 + horizonte.Month;
        // Só precisa de mais se o último gerado estiver a menos de 12 meses
        var minimoHorizonte = hoje.AddMonths(12);
        var minimoInt   = minimoHorizonte.Year * 100 + minimoHorizonte.Month;

        // Para cada grupo recorrente calcula:
        //   UltimoAnoMes — MAX dos meses não-cancelados (fronteira ativa da série)
        //   MaxCancelado  — MAX dos meses cancelados (fronteira de encerramento, se houver)
        // Se MaxCancelado > UltimoAnoMes o usuário encerrou a série → não estender.
        var todosGrupos = await db.Lancamentos
            .Where(l => l.IsRecorrente && l.GrupoParcelas.HasValue)
            .GroupBy(l => new { l.GrupoParcelas, l.UsuarioId })
            .Select(g => new
            {
                GrupoParcelas = g.Key.GrupoParcelas!.Value,
                g.Key.UsuarioId,
                UltimoAnoMes = g
                    .Where(l => l.Situacao != SituacaoLancamento.Cancelado)
                    .Max(l => (int?)(l.Ano * 100 + l.Mes)),
                MaxCancelado = g
                    .Where(l => l.Situacao == SituacaoLancamento.Cancelado)
                    .Max(l => (int?)(l.Ano * 100 + l.Mes)),
            })
            .ToListAsync(ct);

        var resumoGrupos = todosGrupos
            .Where(g => g.UltimoAnoMes.HasValue                   // tem registros ativos
                     && g.UltimoAnoMes.Value < minimoInt           // precisa de mais meses
                     && (!g.MaxCancelado.HasValue                  // sem cancelamentos, OU
                         || g.MaxCancelado.Value <= g.UltimoAnoMes.Value)) // cancelamentos só no meio
            .Select(g => new { g.GrupoParcelas, g.UsuarioId, UltimoAnoMes = g.UltimoAnoMes!.Value })
            .ToList();

        if (resumoGrupos.Count == 0)
        {
            logger.LogInformation("[DailyJob] Geração de recorrentes: nenhum grupo precisa de extensão.");
            return;
        }

        var novos = new List<Lancamento>();

        foreach (var resumo in resumoGrupos)
        {
            // Carrega o template (último lançamento do grupo, incluindo Cancelados)
            var ultimoAnoMes = resumo.UltimoAnoMes;
            var template = await db.Lancamentos
                .Where(l => l.GrupoParcelas == resumo.GrupoParcelas
                         && l.UsuarioId    == resumo.UsuarioId
                         && l.Ano * 100 + l.Mes == ultimoAnoMes)
                .OrderByDescending(l => l.ParcelaAtual)
                .FirstOrDefaultAsync(ct);

            if (template is null) continue;

            // Meses que já existem no banco para este grupo (qualquer situação, incluindo Cancelado).
            // Evita recriar meses que o usuário excluiu (soft-cancel) ou que já foram gerados.
            var mesesExistentes = (await db.Lancamentos
                .Where(l => l.GrupoParcelas == resumo.GrupoParcelas && l.UsuarioId == resumo.UsuarioId)
                .Select(l => l.Ano * 100 + l.Mes)
                .ToListAsync(ct))
                .ToHashSet();

            var mes             = ultimoAnoMes % 100;
            var ano             = ultimoAnoMes / 100;
            var proximaParcela  = (template.ParcelaAtual ?? 1);

            while (true)
            {
                // Avança um mês
                mes++;
                if (mes > 12) { mes = 1; ano++; }
                proximaParcela++;

                if (ano * 100 + mes > limiteInt) break;

                // Pula meses que já existem (AVencer, Cancelado, Pago, etc.)
                if (mesesExistentes.Contains(ano * 100 + mes)) continue;

                var diaMax   = DateTime.DaysInMonth(ano, mes);
                var dia      = Math.Min(template.Data.Day, diaMax);
                var dataGerada = new DateTime(ano, mes, dia);

                novos.Add(new Lancamento(
                    template.Descricao, dataGerada, template.Valor,
                    template.Tipo, SituacaoLancamento.AVencer,
                    mes, ano,
                    template.CategoriaId, template.CartaoId,
                    proximaParcela, template.TotalParcelas, resumo.GrupoParcelas,
                    isRecorrente: true,
                    usuarioId: resumo.UsuarioId));
            }
        }

        if (novos.Count > 0)
        {
            await db.Lancamentos.AddRangeAsync(novos, ct);
            await db.SaveChangesAsync(ct);
        }

        logger.LogInformation("[DailyJob] Geração de recorrentes: {qtd} lançamento(s) gerados em {grupos} grupo(s).",
            novos.Count, resumoGrupos.Count);
    }
}
