using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Produtos.Commands.DeleteProduto;

public record DeleteProdutoCommand(Guid Id) : IRequest;

public class DeleteProdutoCommandHandler(
    IProdutoRepository repo,
    IUnitOfWork uow) : IRequestHandler<DeleteProdutoCommand>
{
    public async Task Handle(DeleteProdutoCommand r, CancellationToken ct)
    {
        var produto = await repo.GetByIdAsync(r.Id, ct)
            ?? throw new KeyNotFoundException("Produto não encontrado.");
        repo.Remove(produto);
        await uow.SaveChangesAsync(ct);
    }
}
