using ControleFinanceiro.Domain.Enums;
using MediatR;

namespace ControleFinanceiro.Application.SaldoContas.Commands.CreateConta;

public record CreateContaCommand(
    string Banco,
    decimal SaldoInicial,
    TipoConta Tipo
) : IRequest<Guid>;
