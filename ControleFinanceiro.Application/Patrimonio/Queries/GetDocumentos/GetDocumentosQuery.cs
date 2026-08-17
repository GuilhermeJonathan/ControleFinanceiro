using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Patrimonio.Queries.GetDocumentos;

public record DocumentoDto(
    Guid Id, int Alvo, Guid? AlvoId, string Nome, string? ContentType,
    long Tamanho, string? Categoria, DateTime CriadoEm);

/// <summary>Lista os documentos de um alvo (Cliente / Ativo / Estrutura) do cliente logado.</summary>
public record GetDocumentosQuery(AlvoDocumento Alvo, Guid? AlvoId) : IRequest<IReadOnlyList<DocumentoDto>>;

public class GetDocumentosQueryHandler(IDocumentoRepository repo, ICurrentUser currentUser)
    : IRequestHandler<GetDocumentosQuery, IReadOnlyList<DocumentoDto>>
{
    public async Task<IReadOnlyList<DocumentoDto>> Handle(GetDocumentosQuery request, CancellationToken ct)
    {
        var docs = await repo.GetByAlvoAsync(currentUser.UserId, request.Alvo, request.AlvoId, ct);
        return docs.Select(d => new DocumentoDto(
            d.Id, (int)d.Alvo, d.AlvoId, d.Nome, d.ContentType, d.Tamanho, d.Categoria, d.CriadoEm)).ToList();
    }
}

/// <summary>Recupera um documento (metadado) para download — valida dono no handler de download.</summary>
public record GetDocumentoParaDownloadQuery(Guid Id) : IRequest<(string Nome, string? ContentType, string StoragePath)?>;

public class GetDocumentoParaDownloadQueryHandler(IDocumentoRepository repo, ICurrentUser currentUser)
    : IRequestHandler<GetDocumentoParaDownloadQuery, (string Nome, string? ContentType, string StoragePath)?>
{
    public async Task<(string Nome, string? ContentType, string StoragePath)?> Handle(GetDocumentoParaDownloadQuery request, CancellationToken ct)
    {
        var doc = await repo.GetByIdAsync(request.Id, ct);
        if (doc is null || doc.UsuarioId != currentUser.UserId) return null;
        return (doc.Nome, doc.ContentType, doc.StoragePath);
    }
}
