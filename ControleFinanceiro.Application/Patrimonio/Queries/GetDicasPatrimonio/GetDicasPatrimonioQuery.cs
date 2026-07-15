using MediatR;
using ControleFinanceiro.Application.Lancamentos.Queries.GetDicas;

namespace ControleFinanceiro.Application.Patrimonio.Queries.GetDicasPatrimonio;

public record GetDicasPatrimonioQuery : IRequest<IEnumerable<DicaFinanceiraDto>>;
