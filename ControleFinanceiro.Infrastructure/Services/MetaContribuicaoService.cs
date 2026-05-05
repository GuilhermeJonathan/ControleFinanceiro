using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ControleFinanceiro.Infrastructure.Services;

public class MetaContribuicaoService(
    IServiceScopeFactory scopeFactory,
    ILogger<MetaContribuicaoService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try { await ProcessAsync(stoppingToken); }
            catch (Exception ex) { logger.LogError(ex, "Erro ao processar contribuições automáticas de metas"); }

            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }

    internal async Task ProcessAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var metaRepo = scope.ServiceProvider.GetRequiredService<IMetaRepository>();
        var lancRepo = scope.ServiceProvider.GetRequiredService<ILancamentoRepository>();
        var catRepo  = scope.ServiceProvider.GetRequiredService<ICategoriaRepository>();
        var uow      = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        // Brasília UTC-3
        var hoje = DateTime.UtcNow.AddHours(-3).Date;

        var metas = await metaRepo.GetAllWithContribuicaoAsync(ct);

        foreach (var meta in metas)
        {
            if (meta.ContribuicaoDia != hoje.Day) continue;
            if (meta.Status != StatusMeta.Ativa) continue;

            // Já contribuiu este mês?
            if (meta.UltimaContribuicaoEm.HasValue)
            {
                var ultima = meta.UltimaContribuicaoEm.Value.AddHours(-3).Date;
                if (ultima.Year == hoje.Year && ultima.Month == hoje.Month) continue;
            }

            try
            {
                // Busca uma categoria de débito do usuário para associar ao lançamento
                var categorias = await catRepo.GetAllAsync(meta.UsuarioId, ct);
                var catId = categorias
                    .Where(c => c.Tipo == TipoLancamento.Debito)
                    .Select(c => (Guid?)c.Id)
                    .FirstOrDefault();

                var lancamento = new Lancamento(
                    descricao:   $"Meta: {meta.Titulo}",
                    data:        DateTime.UtcNow,
                    valor:       meta.ContribuicaoMensalValor!.Value,
                    tipo:        TipoLancamento.Debito,
                    situacao:    SituacaoLancamento.Pago,
                    mes:         hoje.Month,
                    ano:         hoje.Year,
                    categoriaId: catId,
                    usuarioId:   meta.UsuarioId);

                await lancRepo.AddAsync(lancamento, ct);

                // Recarrega a meta com tracking para poder salvar as alterações
                var metaTracked = await metaRepo.GetByIdAsync(meta.Id, ct);
                if (metaTracked is null) continue;

                metaTracked.RegistrarContribuicao(meta.ContribuicaoMensalValor!.Value);
                metaRepo.Update(metaTracked);

                await uow.SaveChangesAsync(ct);

                logger.LogInformation(
                    "Contribuição automática criada: Meta={MetaId} Valor={Valor}",
                    meta.Id, meta.ContribuicaoMensalValor);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao criar contribuição para meta {MetaId}", meta.Id);
            }
        }
    }
}
