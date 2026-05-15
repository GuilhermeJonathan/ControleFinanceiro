using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Vendas.Commands.UpdateVenda;

public record UpdateVendaCommand(
    Guid Id,
    Guid? ProdutoId,
    string Descricao,
    decimal Valor,
    DateTime Data) : IRequest;

public class UpdateVendaCommandHandler(
    IVendaRepository repo,
    IUnitOfWork uow) : IRequestHandler<UpdateVendaCommand>
{
    public async Task Handle(UpdateVendaCommand r, CancellationToken ct)
    {
        var venda = await repo.GetByIdAsync(r.Id, ct)
            ?? throw new KeyNotFoundException("Venda não encontrada.");
        venda.Atualizar(r.Descricao, r.Valor, r.Data, r.ProdutoId);
        repo.Update(venda);
        await uow.SaveChangesAsync(ct);
    }
}
