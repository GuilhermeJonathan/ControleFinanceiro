using ControleFinanceiro.Domain.Enums;
using MediatR;

namespace ControleFinanceiro.Application.SaldoContas.Commands.UpdateConta;

public record UpdateContaCommand(
    Guid Id,
    string Banco,
    decimal Saldo,
    TipoConta Tipo
) : IRequest;
