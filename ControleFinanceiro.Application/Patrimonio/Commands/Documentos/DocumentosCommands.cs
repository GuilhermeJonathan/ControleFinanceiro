using System.Text.RegularExpressions;
using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Patrimonio.Commands.Documentos;

// ── Upload de documento (backend faz proxy: recebe e grava no storage) ──────────

public record UploadDocumentoCommand(
    AlvoDocumento Alvo,
    Guid? AlvoId,
    string Nome,
    string? ContentType,
    long Tamanho,
    Stream Conteudo,
    string? Categoria = null) : IRequest<Guid>;

public class UploadDocumentoCommandHandler(
    IDocumentoRepository repo,
    IArquivoStorage storage,
    ICurrentUser currentUser,
    IUnitOfWork uow)
    : IRequestHandler<UploadDocumentoCommand, Guid>
{
    public async Task<Guid> Handle(UploadDocumentoCommand request, CancellationToken ct)
    {
        if (!storage.Configurado)
            throw new InvalidOperationException("Armazenamento de arquivos não configurado no servidor.");
        if (string.IsNullOrWhiteSpace(request.Nome))
            throw new InvalidOperationException("Informe o nome do arquivo.");
        if (request.Tamanho <= 0)
            throw new InvalidOperationException("Arquivo vazio.");

        var owner = currentUser.UserId;
        var alvoSeg = request.Alvo.ToString().ToLowerInvariant();
        var alvoIdSeg = request.Alvo == AlvoDocumento.Cliente ? "cliente" : (request.AlvoId?.ToString() ?? "sem-alvo");
        var nomeSafe = Sanitizar(request.Nome);
        var caminho = $"{owner}/{alvoSeg}/{alvoIdSeg}/{Guid.NewGuid():N}-{nomeSafe}";

        var contentType = string.IsNullOrWhiteSpace(request.ContentType) ? "application/octet-stream" : request.ContentType;
        await storage.UploadAsync(caminho, request.Conteudo, contentType, ct);

        var doc = new Documento(owner, request.Alvo, request.AlvoId, request.Nome, caminho,
            contentType, request.Tamanho, owner, request.Categoria?.Trim());
        await repo.AddAsync(doc, ct);
        await uow.SaveChangesAsync(ct);
        return doc.Id;
    }

    private static string Sanitizar(string nome)
    {
        var limpo = Regex.Replace(nome.Trim(), @"[^\w.\-]+", "_");
        return limpo.Length > 120 ? limpo[^120..] : limpo;
    }
}

// ── Delete ──────────────────────────────────────────────────────────────────

public record DeleteDocumentoCommand(Guid Id) : IRequest;

public class DeleteDocumentoCommandHandler(
    IDocumentoRepository repo,
    IArquivoStorage storage,
    ICurrentUser currentUser,
    IUnitOfWork uow)
    : IRequestHandler<DeleteDocumentoCommand>
{
    public async Task Handle(DeleteDocumentoCommand request, CancellationToken ct)
    {
        var doc = await repo.GetByIdAsync(request.Id, ct)
            ?? throw new KeyNotFoundException("Documento não encontrado.");
        if (doc.UsuarioId != currentUser.UserId)
            throw new UnauthorizedAccessException("Acesso negado ao documento.");

        // Best-effort no storage: mesmo que a remoção do objeto falhe, removemos o metadado.
        try { if (storage.Configurado) await storage.DeleteAsync(doc.StoragePath, ct); } catch { /* ignora */ }

        repo.Remove(doc);
        await uow.SaveChangesAsync(ct);
    }
}
