using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Vendas.Queries.GetVendas;

public record VendaDto(
    Guid Id,
    Guid? ProdutoId,
    string? ProdutoNome,
    string Descricao,
    decimal Valor,
    DateTime Data,
    StatusVenda Status,
    OrigemVenda Origem,
    DateTime CriadoEm,
    string CriadoPorNome);

public record GetVendasQuery(
    DateTime? De,
    DateTime? Ate,
    Guid? ProdutoId,
    StatusVenda? Status) : IRequest<IEnumerable<VendaDto>>;

public class GetVendasQueryHandler(
    IVendaRepository vendaRepo,
    IProdutoRepository produtoRepo,
    ICurrentUser currentUser) : IRequestHandler<GetVendasQuery, IEnumerable<VendaDto>>
{
    public async Task<IEnumerable<VendaDto>> Handle(GetVendasQuery r, CancellationToken ct)
    {
        var vendas = await vendaRepo.GetAllAsync(r.De, r.Ate, r.ProdutoId, r.Status, ct);
        var produtos = await produtoRepo.GetAllAsync(currentUser.UserId, ct);
        var produtoMap = produtos.ToDictionary(p => p.Id, p => p.Nome);

        return vendas.Select(v => new VendaDto(
            v.Id,
            v.ProdutoId,
            v.ProdutoId.HasValue && produtoMap.TryGetValue(v.ProdutoId.Value, out var nome) ? nome : null,
            v.Descricao,
            v.Valor,
            v.Data,
            v.Status,
            v.Origem,
            v.CriadoEm,
            v.CriadoPorNome));
    }
}
