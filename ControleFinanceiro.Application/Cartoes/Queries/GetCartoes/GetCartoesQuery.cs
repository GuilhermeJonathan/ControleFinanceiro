using ControleFinanceiro.Application.Common;
using MediatR;

namespace ControleFinanceiro.Application.Cartoes.Queries.GetCartoes;

public record GetCartoesQuery(int Mes, int Ano, int Page = 1, int PageSize = 100) : IRequest<PagedResult<CartaoDto>>;
