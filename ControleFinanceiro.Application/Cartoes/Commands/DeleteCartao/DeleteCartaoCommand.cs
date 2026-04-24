using MediatR;

namespace ControleFinanceiro.Application.Cartoes.Commands.DeleteCartao;

public record DeleteCartaoCommand(Guid Id) : IRequest;
