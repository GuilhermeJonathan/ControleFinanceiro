using MediatR;

namespace ControleFinanceiro.Application.SaldoContas.Commands.DeleteConta;

public record DeleteContaCommand(Guid Id) : IRequest;
