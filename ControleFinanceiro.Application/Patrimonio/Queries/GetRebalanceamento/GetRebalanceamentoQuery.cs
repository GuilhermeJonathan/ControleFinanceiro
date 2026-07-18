using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Patrimonio.Queries.GetRebalanceamento;

public record RebalanceamentoClasseDto(
    int Tipo,
    decimal AtualBRL,
    decimal AtualPct,
    decimal AlvoPct,
    decimal DesvioPct);   // AtualPct - AlvoPct (positivo = acima do alvo)

public record RebalanceamentoDto(
    decimal TotalBRL,
    bool TemAlvo,
    IEnumerable<RebalanceamentoClasseDto> Classes);

/// <summary>Compara a alocação atual dos investimentos com a alocação-alvo do usuário efetivo.</summary>
public record GetRebalanceamentoQuery : IRequest<RebalanceamentoDto>;

public class GetRebalanceamentoQueryHandler(
    IInvestimentoRepository investimentoRepository,
    IAlocacaoAlvoRepository alvoRepository,
    IMoedaParamRepository moedaRepository,
    ICurrentUser currentUser)
    : IRequestHandler<GetRebalanceamentoQuery, RebalanceamentoDto>
{
    public async Task<RebalanceamentoDto> Handle(GetRebalanceamentoQuery request, CancellationToken cancellationToken)
    {
        var lista = (await investimentoRepository.GetByUsuarioAsync(currentUser.UserId, cancellationToken)).ToList();
        var alvos = (await alvoRepository.GetByUsuarioAsync(currentUser.UserId, cancellationToken))
            .ToDictionary(a => a.Tipo, a => a.PercentualAlvo);

        var fx = (await moedaRepository.GetAllAsync(cancellationToken))
            .ToDictionary(m => m.Codigo.ToUpperInvariant(), m => m.CotacaoBRL);
        decimal ParaBRL(decimal valor, MoedaPatrimonio moeda) =>
            moeda == MoedaPatrimonio.BRL ? valor
            : valor * (fx.TryGetValue(moeda.ToString(), out var r) && r > 0 ? r : 1m);

        var atualPorTipo = lista
            .GroupBy(i => i.Tipo)
            .ToDictionary(g => g.Key, g => g.Sum(i => ParaBRL(i.ValorAtual, i.Moeda)));
        var total = atualPorTipo.Values.Sum();

        // Todas as classes que têm alvo OU posição atual.
        var tipos = atualPorTipo.Keys.Union(alvos.Keys).Distinct().OrderBy(t => (int)t).ToList();

        var classes = tipos.Select(t =>
        {
            var atualBRL = atualPorTipo.GetValueOrDefault(t, 0m);
            var atualPct = total > 0 ? Math.Round(atualBRL / total * 100m, 1) : 0m;
            var alvoPct  = alvos.GetValueOrDefault(t, 0m);
            return new RebalanceamentoClasseDto((int)t, Math.Round(atualBRL, 2), atualPct, alvoPct, Math.Round(atualPct - alvoPct, 1));
        }).ToList();

        return new RebalanceamentoDto(Math.Round(total, 2), alvos.Count > 0, classes);
    }
}
