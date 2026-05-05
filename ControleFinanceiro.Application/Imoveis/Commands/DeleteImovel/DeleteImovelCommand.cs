using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Imoveis.Commands.DeleteImovel;

public record DeleteImovelCommand(Guid Id) : IRequest;

public class DeleteImovelCommandHandler(
    IImovelRepository repo,
    ICurrentUser currentUser,
    IUnitOfWork uow) : IRequestHandler<DeleteImovelCommand>
{
    public async Task Handle(DeleteImovelCommand r, CancellationToken ct)
    {
        var imovel = await repo.GetByIdAsync(r.Id, currentUser.UserId, currentUser.PodeVerImoveis, ct)
            ?? throw new KeyNotFoundException("Imóvel não encontrado.");

        repo.Delete(imovel);
        await uow.SaveChangesAsync(ct);
    }
}
