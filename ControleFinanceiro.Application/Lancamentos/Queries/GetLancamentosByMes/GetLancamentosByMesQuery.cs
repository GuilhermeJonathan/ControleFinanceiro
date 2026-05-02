using ControleFinanceiro.Application.Common;
using MediatR;

namespace ControleFinanceiro.Application.Lancamentos.Queries.GetLancamentosByMes;

public record GetLancamentosByMesQuery(int Mes, int Ano, int Page = 1, int PageSize = 200) : IRequest<PagedResult<LancamentoDto>>;
