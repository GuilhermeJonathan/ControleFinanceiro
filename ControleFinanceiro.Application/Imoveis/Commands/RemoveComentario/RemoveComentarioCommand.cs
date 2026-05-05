using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Imoveis.Commands.RemoveComentario;

public record RemoveComentarioCommand(Guid ComentarioId) : IRequest;

public class RemoveComentarioCommandHandler(
    IImovelRepository repo,
    ICurrentUser currentUser,
    IUnitOfWork uow) : IRequestHandler<RemoveComentarioCommand>
{
    public async Task Handle(RemoveComentarioCommand r, CancellationToken ct)
    {
        var comentario = await repo.GetComentarioAsync(r.ComentarioId, currentUser.UserId, currentUser.PodeVerImoveis, ct)
            ?? throw new KeyNotFoundException("Comentário não encontrado.");

        repo.DeleteComentario(comentario);
        await uow.SaveChangesAsync(ct);
    }
}
