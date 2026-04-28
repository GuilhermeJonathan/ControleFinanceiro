using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Categorias.Queries.GetCategorias;

public class GetCategoriasQueryHandler(ICategoriaRepository repository, ICurrentUser currentUser)
    : IRequestHandler<GetCategoriasQuery, IEnumerable<CategoriaDto>>
{
    public async Task<IEnumerable<CategoriaDto>> Handle(GetCategoriasQuery request, CancellationToken cancellationToken)
    {
        var categorias = await repository.GetAllAsync(currentUser.UserId, cancellationToken);
        return categorias.Select(c => new CategoriaDto(c.Id, c.Nome, c.Tipo, c.LimiteMensal));
    }
}
