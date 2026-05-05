using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Imoveis.Commands.RemoveFoto;

public record RemoveFotoCommand(Guid FotoId) : IRequest;

public class RemoveFotoCommandHandler(
    IImovelRepository repo,
    ICurrentUser currentUser,
    IUnitOfWork uow) : IRequestHandler<RemoveFotoCommand>
{
    public async Task Handle(RemoveFotoCommand r, CancellationToken ct)
    {
        var foto = await repo.GetFotoAsync(r.FotoId, currentUser.UserId, currentUser.PodeVerImoveis, ct)
            ?? throw new KeyNotFoundException("Foto não encontrada.");

        repo.DeleteFoto(foto);
        await uow.SaveChangesAsync(ct);
    }
}
