using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Vendas.Commands.DeleteVenda;

public record DeleteVendaCommand(Guid Id) : IRequest;

public class DeleteVendaCommandHandler(
    IVendaRepository repo,
    IUnitOfWork uow) : IRequestHandler<DeleteVendaCommand>
{
    public async Task Handle(DeleteVendaCommand r, CancellationToken ct)
    {
        var venda = await repo.GetByIdAsync(r.Id, ct)
            ?? throw new KeyNotFoundException("Venda não encontrada.");
        repo.Remove(venda);
        await uow.SaveChangesAsync(ct);
    }
}
