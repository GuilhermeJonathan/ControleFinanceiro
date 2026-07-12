using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Assessoria.Queries.GetRecomendacoes;

public record RecomendacaoDto(
    Guid Id,
    Guid ClienteId,
    int Tipo,
    Guid? CategoriaId,
    string Texto,
    int Status,
    string? RespostaCliente,
    DateTime CriadoEm,
    DateTime? RespondidoEm);

/// <summary>Visão do cliente: todas as recomendações recebidas.</summary>
public record GetRecomendacoesClienteQuery : IRequest<IEnumerable<RecomendacaoDto>>;

public class GetRecomendacoesClienteQueryHandler(
    IRecomendacaoRepository repository,
    ICurrentUser currentUser)
    : IRequestHandler<GetRecomendacoesClienteQuery, IEnumerable<RecomendacaoDto>>
{
    public async Task<IEnumerable<RecomendacaoDto>> Handle(
        GetRecomendacoesClienteQuery request, CancellationToken cancellationToken)
    {
        var itens = await repository.GetByClienteAsync(currentUser.RealUserId, cancellationToken);
        return itens.Select(ToDto);
    }

    internal static RecomendacaoDto ToDto(Domain.Entities.Recomendacao r) =>
        new(r.Id, r.ClienteId, (int)r.Tipo, r.CategoriaId, r.Texto,
            (int)r.Status, r.RespostaCliente, r.CriadoEm, r.RespondidoEm);
}

/// <summary>Visão do assessor: recomendações enviadas a um cliente específico.</summary>
public record GetRecomendacoesAssessorQuery(Guid ClienteId) : IRequest<IEnumerable<RecomendacaoDto>>;

public class GetRecomendacoesAssessorQueryHandler(
    IRecomendacaoRepository repository,
    ICurrentUser currentUser)
    : IRequestHandler<GetRecomendacoesAssessorQuery, IEnumerable<RecomendacaoDto>>
{
    public async Task<IEnumerable<RecomendacaoDto>> Handle(
        GetRecomendacoesAssessorQuery request, CancellationToken cancellationToken)
    {
        var itens = await repository.GetByAssessorEClienteAsync(
            currentUser.RealUserId, request.ClienteId, cancellationToken);
        return itens.Select(GetRecomendacoesClienteQueryHandler.ToDto);
    }
}
