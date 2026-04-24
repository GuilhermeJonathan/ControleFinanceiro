using MediatR;

namespace ControleFinanceiro.Application.Cartoes.Queries.GetCartoes;

public record GetCartoesQuery(int Mes, int Ano) : IRequest<IEnumerable<CartaoDto>>;
