using MediatR;

namespace ControleFinanceiro.Application.Lancamentos.Queries.GetDicas;

public record GetDicasQuery(int Mes, int Ano) : IRequest<IEnumerable<DicaFinanceiraDto>>;
