using ControleFinanceiro.Application.Common.Email;
using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Parametros.Commands;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using ControleFinanceiro.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Globalization;

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
        await JobSnapshotPatrimonioAsync(ct);
        await JobRelatorioMensalAsync(ct);
        await JobAtualizarCotacoesAsync(ct);
        await JobAtualizarInvestimentosAsync(ct);

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

    // ── Job 3: Snapshot mensal do patrimônio ──────────────────────────────────
    // Garante 1 foto por usuário no mês corrente (mesmo para quem não abre a tela).
    // A captura preguiçosa (GetResumoPatrimonial) mantém os usuários ativos em dia;
    // este job cobre os passivos. Não sobrescreve snapshots já existentes do mês.
    private async Task JobSnapshotPatrimonioAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var hoje = DateTime.UtcNow;
        int ano = hoje.Year, mes = hoje.Month;

        var fx = await db.MoedasParam.ToDictionaryAsync(m => m.Codigo.ToUpperInvariant(), m => m.CotacaoBRL, ct);
        decimal ParaBRL(decimal v, MoedaPatrimonio moeda) =>
            moeda == MoedaPatrimonio.BRL ? v : v * (fx.TryGetValue(moeda.ToString(), out var r) && r > 0 ? r : 1m);

        var ativos = await db.AtivosPatrimoniais
            .Select(a => new { a.UsuarioId, a.ValorAtual, a.Moeda }).ToListAsync(ct);
        var passivos = await db.PassivosPatrimoniais
            .Select(p => new { p.UsuarioId, p.Valor, p.Moeda }).ToListAsync(ct);

        var usuarios = ativos.Select(a => a.UsuarioId)
            .Concat(passivos.Select(p => p.UsuarioId))
            .Distinct().ToList();

        if (usuarios.Count == 0)
        {
            logger.LogInformation("[DailyJob] Snapshot patrimônio: nenhum usuário com patrimônio.");
            return;
        }

        var jaExiste = (await db.PatrimonioSnapshots
            .Where(s => s.Ano == ano && s.Mes == mes)
            .Select(s => s.UsuarioId).ToListAsync(ct)).ToHashSet();

        var novos = new List<PatrimonioSnapshot>();
        foreach (var uid in usuarios)
        {
            if (jaExiste.Contains(uid)) continue;
            var bens = ativos.Where(a => a.UsuarioId == uid).Sum(a => ParaBRL(a.ValorAtual, a.Moeda));
            var div  = passivos.Where(p => p.UsuarioId == uid).Sum(p => ParaBRL(p.Valor, p.Moeda));
            novos.Add(PatrimonioSnapshot.Criar(uid, ano, mes,
                Math.Round(bens - div, 2), Math.Round(bens, 2), Math.Round(div, 2)));
        }

        if (novos.Count > 0)
        {
            await db.PatrimonioSnapshots.AddRangeAsync(novos, ct);
            await db.SaveChangesAsync(ct);
        }

        logger.LogInformation("[DailyJob] Snapshot patrimônio: {qtd} snapshot(s) de {mes}/{ano} criados.",
            novos.Count, mes, ano);
    }

    // ── Job 4: Resumo mensal por e-mail ───────────────────────────────────────
    // Uma vez por mês, envia a cada cliente ativo um e-mail com o patrimônio do mês
    // (dos snapshots) + variação vs. mês anterior, com a marca da consultoria.
    private static readonly string[] MesesPt =
        { "janeiro","fevereiro","março","abril","maio","junho","julho","agosto","setembro","outubro","novembro","dezembro" };

    private async Task JobRelatorioMensalAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var email = scope.ServiceProvider.GetRequiredService<IEmailService>();
        var lookup = scope.ServiceProvider.GetRequiredService<IUserNameLookup>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var hoje = DateTime.UtcNow;
        int ano = hoje.Year, mes = hoje.Month;
        int anoMesAtual = ano * 100 + mes;
        int mesAnt = mes == 1 ? 12 : mes - 1;
        int anoAnt = mes == 1 ? ano - 1 : ano;
        var ptBR = CultureInfo.GetCultureInfo("pt-BR");
        var mesLabel = $"{MesesPt[mes - 1]}/{ano}";
        var link = $"{ConviteEmailBuilder.BaseUrl(config)}/patrimonio";

        var vinculos = await db.VinculosAssessoria
            .Where(v => v.AceitoEm != null && v.RevogadoEm == null)
            .ToListAsync(ct);

        int enviados = 0;
        foreach (var v in vinculos)
        {
            // Já enviado neste mês? pula.
            if (v.UltimoRelatorioMensalEm is { } u && u.Year * 100 + u.Month >= anoMesAtual) continue;

            var atual = await db.PatrimonioSnapshots
                .FirstOrDefaultAsync(s => s.UsuarioId == v.ClienteId && s.Ano == ano && s.Mes == mes, ct);
            if (atual is null) continue; // sem dados do mês → não envia

            var contato = await lookup.GetContatoAsync(v.ClienteId, ct);
            if (string.IsNullOrWhiteSpace(contato?.Email)) continue;

            var cfg = await db.ConsultoriaConfigs.FirstOrDefaultAsync(c => c.UsuarioId == v.AssessorId, ct);
            var marca = cfg?.NomeConsultoria is { Length: > 0 } n ? n : (v.NomeAssessor ?? "Seu assessor");
            var cor = cfg?.CorMarca is { Length: > 0 } c ? c : "#16a34a";
            var logo = ConviteEmailBuilder.LogoUrl(config, v.AssessorId, !string.IsNullOrWhiteSpace(cfg?.LogoBase64));

            var anterior = await db.PatrimonioSnapshots
                .FirstOrDefaultAsync(s => s.UsuarioId == v.ClienteId && s.Ano == anoAnt && s.Mes == mesAnt, ct);
            string? variacao = null;
            if (anterior is { PatrimonioLiquidoBRL: not 0 })
            {
                var pct = (atual.PatrimonioLiquidoBRL - anterior.PatrimonioLiquidoBRL) / Math.Abs(anterior.PatrimonioLiquidoBRL) * 100m;
                var seta = pct >= 0 ? "▲" : "▼";
                variacao = $"{seta} {Math.Abs(pct):F1}% vs mês anterior";
            }

            var patrimonioFmt = "R$ " + atual.PatrimonioLiquidoBRL.ToString("N2", ptBR);
            var nomeCliente = contato!.Nome ?? v.NomeCliente ?? "Cliente";
            var body = ConviteEmailBuilder.CorpoRelatorioMensal(marca, cor, logo, nomeCliente, mesLabel, patrimonioFmt, variacao, link);

            try
            {
                await email.SendAsync(contato.Email!, nomeCliente, $"Seu patrimônio em {mesLabel} — {marca}", body, ct, marca);
                v.MarcarRelatorioMensalEnviado();
                enviados++;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "[DailyJob] Falha ao enviar resumo mensal para cliente {ClienteId}.", v.ClienteId);
            }
        }

        if (enviados > 0) await db.SaveChangesAsync(ct);
        logger.LogInformation("[DailyJob] Resumo mensal: {qtd} e-mail(s) enviados.", enviados);
    }

    // ── Job 5: Atualizar cotações de moedas ──────────────────────────────────
    private async Task JobAtualizarCotacoesAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var mediator    = scope.ServiceProvider.GetRequiredService<IMediator>();

        // Reutiliza o command (dedup + histórico + guarda de frescor). Forcar=false: não martela a API a cada restart.
        var r = await mediator.Send(new AtualizarCotacoesMoedasCommand(false), ct);
        if (r.Pulado) logger.LogInformation("[DailyJob] Cotações: puladas (atualizadas recentemente).");
        else logger.LogInformation("[DailyJob] Cotações: {qtd} moeda(s) atualizadas.", r.Atualizadas);
    }

    // ── Job 6: Atualizar valor atual dos investimentos via ticker ────────────
    private async Task JobAtualizarInvestimentosAsync(CancellationToken ct)
    {
        using var scope      = scopeFactory.CreateScope();
        var db               = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var priceService     = scope.ServiceProvider.GetRequiredService<IAssetPriceService>();
        var historicoRepo    = scope.ServiceProvider.GetRequiredService<IPrecoAtivoHistoricoRepository>();

        // Mercado fechado no fim de semana — não gasta cota da brapi.dev à toa.
        var diaSemana = DateTime.Now.DayOfWeek;
        if (diaSemana is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            logger.LogInformation("[DailyJob] Investimentos: fim de semana ({dia}) — pulado (mercado fechado).", diaSemana);
            return;
        }

        var investimentos = await db.Investimentos
            .Where(i => !string.IsNullOrEmpty(i.Ticker))
            .ToListAsync(ct);

        if (investimentos.Count == 0)
        {
            logger.LogInformation("[DailyJob] Investimentos: nenhum investimento com ticker cadastrado.");
            return;
        }

        // No máximo 1× por dia: se já atualizou hoje, não reconsulta (poupa cota da brapi.dev em restarts).
        var maisRecente = investimentos
            .Where(i => i.ValorAtualizadoEm.HasValue)
            .Select(i => i.ValorAtualizadoEm!.Value)
            .DefaultIfEmpty()
            .Max();
        if (maisRecente != default && maisRecente.Date == DateTime.UtcNow.Date)
        {
            logger.LogInformation("[DailyJob] Investimentos: já atualizado hoje — pulado.");
            return;
        }

        var tickers = investimentos
            .Select(i => i.Ticker!.Trim().ToUpperInvariant())
            .Distinct()
            .ToList();

        var prices = await priceService.GetPricesAsync(tickers, ct);
        if (prices.Count == 0)
        {
            logger.LogWarning("[DailyJob] Investimentos: nenhum preço retornado pela API.");
            return;
        }

        var atualizados = 0;
        foreach (var ticker in tickers)
        {
            if (!prices.TryGetValue(ticker, out var preco)) continue;
            // brapi retorna preço unitário; aqui aplicamos direto em valorAtual (simplificação — sem quantidade separada).
            foreach (var inv in investimentos.Where(i => string.Equals(i.Ticker!.Trim(), ticker, StringComparison.OrdinalIgnoreCase)))
            {
                if (inv.AtualizarValorAutomatico(preco)) atualizados++;
            }
            await historicoRepo.AddAsync(new PrecoAtivoHistorico(ticker, preco, "brapi.dev"), ct);
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("[DailyJob] Investimentos: {qtd} investimento(s) atualizados via ticker.", atualizados);
    }
}
