using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Produtos.Commands.UpdateProduto;

public record UpdateProdutoCommand(
    Guid Id,
    string Nome,
    decimal? PrecoDefault) : IRequest;

public class UpdateProdutoCommandHandler(
    IProdutoRepository repo,
    IUnitOfWork uow) : IRequestHandler<UpdateProdutoCommand>
{
    public async Task Handle(UpdateProdutoCommand r, CancellationToken ct)
    {
        var produto = await repo.GetByIdAsync(r.Id, ct)
            ?? throw new KeyNotFoundException("Produto não encontrado.");
        produto.Atualizar(r.Nome, r.PrecoDefault);
        repo.Update(produto);
        await uow.SaveChangesAsync(ct);
    }
}
