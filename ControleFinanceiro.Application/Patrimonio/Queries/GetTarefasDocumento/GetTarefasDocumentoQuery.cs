using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Patrimonio.Queries.GetTarefasDocumento;

public record TarefaDocumentoDto(
    Guid Id, Guid ClienteId, string Titulo, string? Descricao,
    string? AtalhoRota, int Status, DateTime CriadoEm, DateTime? ConcluidaEm);

internal static class TarefaDocMap
{
    public static TarefaDocumentoDto ToDto(TarefaDocumento t) =>
        new(t.Id, t.ClienteId, t.Titulo, t.Descricao, t.AtalhoRota, (int)t.Status, t.CriadoEm, t.ConcluidaEm);
}

/// <summary>Visão do CLIENTE: tarefas de documento que recebeu.</summary>
public record GetTarefasDocumentoClienteQuery : IRequest<IReadOnlyList<TarefaDocumentoDto>>;

public class GetTarefasDocumentoClienteQueryHandler(ITarefaDocumentoRepository repo, ICurrentUser currentUser)
    : IRequestHandler<GetTarefasDocumentoClienteQuery, IReadOnlyList<TarefaDocumentoDto>>
{
    public async Task<IReadOnlyList<TarefaDocumentoDto>> Handle(GetTarefasDocumentoClienteQuery request, CancellationToken ct) =>
        (await repo.GetByClienteAsync(currentUser.RealUserId, ct)).Select(TarefaDocMap.ToDto).ToList();
}

/// <summary>Visão do ASSESSOR: tarefas que criou para um cliente.</summary>
public record GetTarefasDocumentoAssessorQuery(Guid ClienteId) : IRequest<IReadOnlyList<TarefaDocumentoDto>>;

public class GetTarefasDocumentoAssessorQueryHandler(ITarefaDocumentoRepository repo, ICurrentUser currentUser)
    : IRequestHandler<GetTarefasDocumentoAssessorQuery, IReadOnlyList<TarefaDocumentoDto>>
{
    public async Task<IReadOnlyList<TarefaDocumentoDto>> Handle(GetTarefasDocumentoAssessorQuery request, CancellationToken ct) =>
        (await repo.GetByAssessorClienteAsync(currentUser.RealUserId, request.ClienteId, ct)).Select(TarefaDocMap.ToDto).ToList();
}
