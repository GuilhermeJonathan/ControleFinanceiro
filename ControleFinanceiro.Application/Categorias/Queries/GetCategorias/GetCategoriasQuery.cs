using MediatR;

namespace ControleFinanceiro.Application.Categorias.Queries.GetCategorias;

public record GetCategoriasQuery : IRequest<IEnumerable<CategoriaDto>>;
