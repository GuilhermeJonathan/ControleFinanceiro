using MediatR;

namespace ControleFinanceiro.Application.Lancamentos.Queries.GetLancamentosBusca;

public record GetLancamentosBuscaQuery(string Q, int Page, int PageSize) : IRequest<BuscaResultDto>;
