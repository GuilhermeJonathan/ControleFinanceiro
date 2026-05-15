using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Vendas.Commands.AtualizarStatusVenda;

public record AtualizarStatusVendaCommand(Guid Id, StatusVenda Status) : IRequest;

public class AtualizarStatusVendaCommandHandler(
    IVendaRepository repo,
    IUnitOfWork uow) : IRequestHandler<AtualizarStatusVendaCommand>
{
    public async Task Handle(AtualizarStatusVendaCommand r, CancellationToken ct)
    {
        var venda = await repo.GetByIdAsync(r.Id, ct)
            ?? throw new KeyNotFoundException("Venda não encontrada.");
        venda.AtualizarStatus(r.Status);
        repo.Update(venda);
        await uow.SaveChangesAsync(ct);
    }
}
