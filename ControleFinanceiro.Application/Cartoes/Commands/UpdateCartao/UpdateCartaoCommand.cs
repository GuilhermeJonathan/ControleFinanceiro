using MediatR;

namespace ControleFinanceiro.Application.Cartoes.Commands.UpdateCartao;

public record UpdateCartaoCommand(Guid Id, string Nome, int? DiaVencimento = null) : IRequest;
