using MediatR;

namespace ControleFinanceiro.Application.Lancamentos.Queries.GetLancamentosByMes;

public record GetLancamentosByMesQuery(int Mes, int Ano) : IRequest<IEnumerable<LancamentoDto>>;
