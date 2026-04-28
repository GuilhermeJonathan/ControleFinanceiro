using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Metas.Commands.DeleteMeta;

public record DeleteMetaCommand(Guid Id) : IRequest;

public class DeleteMetaCommandHandler(
    IMetaRepository repo,
    IUnitOfWork uow) : IRequestHandler<DeleteMetaCommand>
{
    public async Task Handle(DeleteMetaCommand r, CancellationToken ct)
    {
        var meta = await repo.GetByIdAsync(r.Id, ct)
            ?? throw new KeyNotFoundException("Meta não encontrada.");
        repo.Remove(meta);
        await uow.SaveChangesAsync(ct);
    }
}
