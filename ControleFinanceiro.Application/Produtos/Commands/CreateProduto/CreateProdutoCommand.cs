using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Produtos.Commands.CreateProduto;

public record CreateProdutoCommand(
    string Nome,
    decimal? PrecoDefault) : IRequest<Guid>;

public class CreateProdutoCommandHandler(
    IProdutoRepository repo,
    ICurrentUser currentUser,
    IUnitOfWork uow) : IRequestHandler<CreateProdutoCommand, Guid>
{
    public async Task<Guid> Handle(CreateProdutoCommand r, CancellationToken ct)
    {
        var produto = new Produto(currentUser.UserId, r.Nome, r.PrecoDefault);
        await repo.AddAsync(produto, ct);
        await uow.SaveChangesAsync(ct);
        return produto.Id;
    }
}
