using ControleFinanceiro.Application.Common;
using MediatR;

namespace ControleFinanceiro.Application.Categorias.Queries.GetCategorias;

public record GetCategoriasQuery(int Page = 1, int PageSize = 50) : IRequest<PagedResult<CategoriaDto>>;
