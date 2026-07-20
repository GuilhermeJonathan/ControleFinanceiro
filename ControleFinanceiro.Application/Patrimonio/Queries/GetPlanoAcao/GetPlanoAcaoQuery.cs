using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Patrimonio.Queries.GetPlanoAcao;

public record EtapaPlanoDto(int Ordem, string Titulo, string? Descricao, string? Prazo, string? Alvo, int Status);

public record PlanoAcaoDto(string Objetivo, string? Prazo, IEnumerable<EtapaPlanoDto> Etapas);

/// <summary>Plano de Ação do usuário efetivo (ou null se ainda não houver plano).</summary>
public record GetPlanoAcaoQuery : IRequest<PlanoAcaoDto?>;

public class GetPlanoAcaoQueryHandler(
    IPlanoAcaoRepository repository,
    ICurrentUser currentUser)
    : IRequestHandler<GetPlanoAcaoQuery, PlanoAcaoDto?>
{
    public async Task<PlanoAcaoDto?> Handle(GetPlanoAcaoQuery request, CancellationToken cancellationToken)
    {
        var plano = await repository.GetByUsuarioAsync(currentUser.UserId, cancellationToken);
        if (plano is null) return null;

        return new PlanoAcaoDto(
            plano.Objetivo,
            plano.Prazo,
            plano.Etapas
                .OrderBy(e => e.Ordem)
                .Select(e => new EtapaPlanoDto(e.Ordem, e.Titulo, e.Descricao, e.Prazo, e.Alvo, (int)e.Status))
                .ToList());
    }
}
