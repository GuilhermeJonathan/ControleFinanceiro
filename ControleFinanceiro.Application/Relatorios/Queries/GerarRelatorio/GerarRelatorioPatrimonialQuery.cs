using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Patrimonio.Queries.GetProjecaoDividas;
using ControleFinanceiro.Application.Patrimonio.Queries.GetResumoInvestimentos;
using ControleFinanceiro.Application.Patrimonio.Queries.GetResumoPatrimonial;
using ControleFinanceiro.Application.Simulacoes.Queries.GetSimulacoes;
using MediatR;

namespace ControleFinanceiro.Application.Relatorios.Queries.GerarRelatorio;

/// <summary>
/// Monta e gera o PDF do relatório patrimonial do usuário EFETIVO (o cliente, quando
/// sob view-as do assessor). A marca vem do app (do assessor real). Retorna os bytes do PDF.
/// </summary>
public record GerarRelatorioPatrimonialQuery(string? ClienteNome, RelatorioBranding Branding)
    : IRequest<byte[]>;

public class GerarRelatorioPatrimonialQueryHandler(
    IMediator mediator,
    ICurrentUser currentUser,
    IRelatorioPatrimonialGenerator generator)
    : IRequestHandler<GerarRelatorioPatrimonialQuery, byte[]>
{
    public async Task<byte[]> Handle(GerarRelatorioPatrimonialQuery request, CancellationToken cancellationToken)
    {
        // Todas as sub-queries usam o usuário efetivo (cliente sob view-as).
        var resumo        = await mediator.Send(new GetResumoPatrimonialQuery(), cancellationToken);
        var projecao      = await mediator.Send(new GetProjecaoDividasQuery(), cancellationToken);
        var investimentos = await mediator.Send(new GetResumoInvestimentosQuery(), cancellationToken);
        var simulacoes    = (await mediator.Send(new GetSimulacoesQuery(), cancellationToken)).ToList();

        // Simulação em destaque: a favorita ou a mais recente. Calcula o resultado.
        var sim = simulacoes.FirstOrDefault(x => x.Favorita) ?? simulacoes.FirstOrDefault();
        SimulacaoDestaqueDto? destaque = null;
        if (sim != null)
        {
            var patrimonioInicial = sim.ModoAutomatico ? resumo.PatrimonioLiquidoBRL : sim.PatrimonioInicial;
            var (patrimonioAlvo, idadeExtincao) = ProjecaoSimulacaoCalc.Calcular(
                sim.IdadeAtual, sim.IdadeAlvo, patrimonioInicial,
                sim.AporteMensal, sim.TaxaRetornoRealAnualPct, sim.RetiradaMensal,
                sim.Cenarios.Select(c => new ProjecaoSimulacaoCalc.Cenario(c.Tipo, c.Valor, c.IdadeInicio, c.IdadeFim)));

            destaque = new SimulacaoDestaqueDto(
                sim.Nome, sim.IdadeAtual, sim.IdadeAlvo, sim.AporteMensal, sim.RetiradaMensal,
                sim.TaxaRetornoRealAnualPct, patrimonioInicial, patrimonioAlvo, idadeExtincao);
        }

        var dados = new RelatorioPatrimonialDados(
            ClienteNome: string.IsNullOrWhiteSpace(request.ClienteNome) ? "Cliente" : request.ClienteNome!,
            AssessorNome: currentUser.RealUserName ?? "Assessor",
            GeradoEm: DateTime.UtcNow,
            Resumo: resumo,
            Projecao: projecao,
            Investimentos: investimentos,
            SimulacaoDestaque: destaque);

        return generator.Gerar(dados, request.Branding);
    }
}
