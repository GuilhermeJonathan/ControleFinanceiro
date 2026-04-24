using MediatR;

namespace ControleFinanceiro.Application.Lancamentos.Commands.DeleteLancamento;

public record DeleteLancamentoCommand(Guid Id) : IRequest;
