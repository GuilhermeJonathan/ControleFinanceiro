using MediatR;

namespace ControleFinanceiro.Application.SaldoContas.Queries.GetSaldos;

public record GetSaldosQuery : IRequest<IEnumerable<SaldoContaDto>>;
