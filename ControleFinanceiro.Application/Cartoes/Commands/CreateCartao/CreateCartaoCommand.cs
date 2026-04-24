using MediatR;

namespace ControleFinanceiro.Application.Cartoes.Commands.CreateCartao;

public record CreateCartaoCommand(string Nome, int? DiaVencimento = null) : IRequest<Guid>;
