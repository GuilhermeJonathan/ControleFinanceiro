using ControleFinanceiro.Application.Common;
using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Categorias.Queries.GetCategorias;

public class GetCategoriasQueryHandler(ICategoriaRepository repository, ICurrentUser currentUser)
    : IRequestHandler<GetCategoriasQuery, PagedResult<CategoriaDto>>
{
    public async Task<PagedResult<CategoriaDto>> Handle(GetCategoriasQuery request, CancellationToken cancellationToken)
    {
        var page     = Math.Max(1, request.Page);
        var pageSize = Math.Max(1, request.PageSize);

        var (itens, total) = await repository.GetPagedAsync(currentUser.UserId, page, pageSize, cancellationToken);
        var dtos = itens.Select(c => new CategoriaDto(c.Id, c.Nome, c.Tipo, c.LimiteMensal, c.Icone, c.Cor)).ToList();
        return new PagedResult<CategoriaDto>(dtos, total, page, pageSize);
    }
}
