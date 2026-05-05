using MediatR;

namespace ControleFinanceiro.Application.Lancamentos.Commands.CreateTransferencia;

public record CreateTransferenciaCommand(
    Guid   ContaOrigemId,
    Guid   ContaDestinoId,
    decimal Valor,
    DateTime Data,
    string Descricao
) : IRequest<CreateTransferenciaResult>;

public record CreateTransferenciaResult(Guid IdDebito, Guid IdCredito);
