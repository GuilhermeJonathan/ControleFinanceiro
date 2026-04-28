using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Metas.Commands.AtualizarValorMeta;

public record AtualizarValorMetaCommand(Guid Id, decimal NovoValor) : IRequest;

public class AtualizarValorMetaCommandHandler(
    IMetaRepository repo,
    IUnitOfWork uow) : IRequestHandler<AtualizarValorMetaCommand>
{
    public async Task Handle(AtualizarValorMetaCommand r, CancellationToken ct)
    {
        var meta = await repo.GetByIdAsync(r.Id, ct)
            ?? throw new KeyNotFoundException("Meta não encontrada.");
        meta.AtualizarValor(r.NovoValor);
        repo.Update(meta);
        await uow.SaveChangesAsync(ct);
    }
}
