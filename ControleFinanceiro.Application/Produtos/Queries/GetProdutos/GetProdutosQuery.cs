using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Produtos.Queries.GetProdutos;

public record ProdutoDto(
    Guid Id,
    string Nome,
    decimal? PrecoDefault,
    bool Ativo,
    DateTime CriadoEm);

public record GetProdutosQuery : IRequest<IEnumerable<ProdutoDto>>;

public class GetProdutosQueryHandler(
    IProdutoRepository repo,
    ICurrentUser currentUser) : IRequestHandler<GetProdutosQuery, IEnumerable<ProdutoDto>>
{
    public async Task<IEnumerable<ProdutoDto>> Handle(GetProdutosQuery request, CancellationToken ct)
    {
        var produtos = await repo.GetAllAsync(currentUser.UserId, ct);
        return produtos.Select(p => new ProdutoDto(
            p.Id, p.Nome, p.PrecoDefault, p.Ativo, p.CriadoEm));
    }
}
