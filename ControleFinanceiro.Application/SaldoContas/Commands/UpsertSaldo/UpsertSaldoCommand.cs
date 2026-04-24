using MediatR;

namespace ControleFinanceiro.Application.SaldoContas.Commands.UpsertSaldo;

public record UpsertSaldoCommand(string Banco, decimal Saldo) : IRequest<Guid>;
