using ControleFinanceiro.Application.Lancamentos.Queries.GetDicas;
using MediatR;

namespace ControleFinanceiro.Application.Lancamentos.Queries.GetAnaliseDividas;

public record GetAnaliseDividasQuery : IRequest<IEnumerable<DicaFinanceiraDto>>;
