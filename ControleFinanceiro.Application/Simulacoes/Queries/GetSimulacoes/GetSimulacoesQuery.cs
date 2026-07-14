using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Simulacoes.Queries.GetSimulacoes;

public record CenarioDto(string Nome, int Tipo, decimal Valor, int IdadeInicio, int? IdadeFim);

public record SimulacaoDto(
    Guid Id,
    string Nome,
    bool Favorita,
    int IdadeAtual,
    int IdadeAlvo,
    decimal PatrimonioInicial,
    bool ModoAutomatico,
    decimal AporteMensal,
    decimal TaxaRetornoRealAnualPct,
    decimal RetiradaMensal,
    DateTime CriadoEm,
    DateTime? AtualizadoEm,
    IEnumerable<CenarioDto> Cenarios);

public record GetSimulacoesQuery : IRequest<IEnumerable<SimulacaoDto>>;

public class GetSimulacoesQueryHandler(
    ISimulacaoPatrimonialRepository repository,
    ICurrentUser currentUser)
    : IRequestHandler<GetSimulacoesQuery, IEnumerable<SimulacaoDto>>
{
    public async Task<IEnumerable<SimulacaoDto>> Handle(GetSimulacoesQuery request, CancellationToken cancellationToken)
    {
        var simulacoes = await repository.GetByUsuarioAsync(currentUser.UserId, cancellationToken);

        return simulacoes.Select(s => new SimulacaoDto(
            s.Id, s.Nome, s.Favorita, s.IdadeAtual, s.IdadeAlvo,
            s.PatrimonioInicial, s.ModoAutomatico, s.AporteMensal,
            s.TaxaRetornoRealAnualPct, s.RetiradaMensal, s.CriadoEm, s.AtualizadoEm,
            s.Cenarios.Select(c => new CenarioDto(c.Nome, (int)c.Tipo, c.Valor, c.IdadeInicio, c.IdadeFim))))
            .ToList();
    }
}
