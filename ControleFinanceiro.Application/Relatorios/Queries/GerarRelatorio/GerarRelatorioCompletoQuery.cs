using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Patrimonio.Queries.GetContas;
using ControleFinanceiro.Application.Patrimonio.Queries.GetEstruturas;
using ControleFinanceiro.Application.Patrimonio.Queries.GetPlanoAcao;
using ControleFinanceiro.Application.Patrimonio.Queries.GetProjecaoDividas;
using ControleFinanceiro.Application.Patrimonio.Queries.GetResumoInvestimentos;
using ControleFinanceiro.Application.Patrimonio.Queries.GetResumoPatrimonial;
using ControleFinanceiro.Application.Patrimonio.Queries.GetSucessao;
using ControleFinanceiro.Application.Simulacoes.Queries.GetSimulacoes;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Relatorios.Queries.GerarRelatorio;

/// <summary>Relatório COMPLETO (patrimonial + sucessão) do usuário efetivo, num único PDF.</summary>
public record GerarRelatorioCompletoQuery(string? ClienteNome, RelatorioBranding Branding)
    : IRequest<byte[]>;

public class GerarRelatorioCompletoQueryHandler(
    IMediator mediator,
    ICurrentUser currentUser,
    IConsultoriaConfigRepository consultoriaRepository,
    IRelatorioCompletoGenerator generator)
    : IRequestHandler<GerarRelatorioCompletoQuery, byte[]>
{
    public async Task<byte[]> Handle(GerarRelatorioCompletoQuery request, CancellationToken ct)
    {
        var nome = string.IsNullOrWhiteSpace(request.ClienteNome) ? "Cliente" : request.ClienteNome!;
        var assessor = currentUser.RealUserName ?? "Assessor";
        var geradoEm = DateTime.UtcNow;

        // ── Patrimonial ──
        var resumo        = await mediator.Send(new GetResumoPatrimonialQuery(), ct);
        var projecao      = await mediator.Send(new GetProjecaoDividasQuery(), ct);
        var investimentos = await mediator.Send(new GetResumoInvestimentosQuery(), ct);
        var simulacoes    = (await mediator.Send(new GetSimulacoesQuery(), ct)).ToList();
        var planos        = (await mediator.Send(new GetPlanosAcaoQuery(), ct)).ToList();

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

        var patrimonial = new RelatorioPatrimonialDados(nome, assessor, geradoEm, resumo, projecao, investimentos, destaque, planos);

        // ── Sucessão ──
        var grafo       = await mediator.Send(new GetEstruturasQuery(), ct);
        var sucessaoDto = await mediator.Send(new GetSucessaoQuery(), ct);
        var contas      = await mediator.Send(new GetContasQuery(), ct);
        var indicadores = await mediator.Send(new GetIndicadoresSucessaoQuery(), ct);

        // No completo os planos já aparecem na parte patrimonial → não repetir na de sucessão.
        var sucessao = new RelatorioSucessaoDados(nome, assessor, geradoEm, grafo, sucessaoDto, contas, [], indicadores);

        // Marca (consultoria autoritativa; request é fallback).
        var config = await consultoriaRepository.GetByUsuarioAsync(currentUser.RealUserId, ct);
        var branding = config is not null
            ? new RelatorioBranding(config.NomeConsultoria, config.LogoBase64, config.CorMarca, config.MensagemRodape)
            : request.Branding;

        return generator.Gerar(patrimonial, sucessao, branding);
    }
}
