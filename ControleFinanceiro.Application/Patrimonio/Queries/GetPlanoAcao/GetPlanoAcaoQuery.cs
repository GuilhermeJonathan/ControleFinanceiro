using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Patrimonio.Queries.GetPlanoAcao;

public record EtapaPlanoDto(int Ordem, string Titulo, string? Descricao, string? Prazo, string? Alvo, int Status);

public record PlanoAcaoDto(Guid Id, string Objetivo, string? Prazo, IEnumerable<EtapaPlanoDto> Etapas);

/// <summary>Todos os planos de ação do usuário efetivo (um cliente pode ter vários).</summary>
public record GetPlanosAcaoQuery : IRequest<IEnumerable<PlanoAcaoDto>>;

public class GetPlanosAcaoQueryHandler(
    IPlanoAcaoRepository repository,
    ICurrentUser currentUser)
    : IRequestHandler<GetPlanosAcaoQuery, IEnumerable<PlanoAcaoDto>>
{
    public async Task<IEnumerable<PlanoAcaoDto>> Handle(GetPlanosAcaoQuery request, CancellationToken cancellationToken)
    {
        var planos = await repository.GetByUsuarioAsync(currentUser.UserId, cancellationToken);
        return planos.Select(p => new PlanoAcaoDto(
            p.Id,
            p.Objetivo,
            p.Prazo,
            p.Etapas
                .OrderBy(e => e.Ordem)
                .Select(e => new EtapaPlanoDto(e.Ordem, e.Titulo, e.Descricao, e.Prazo, e.Alvo, (int)e.Status))
                .ToList()))
            .ToList();
    }
}
