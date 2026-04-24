using MediatR;

namespace ControleFinanceiro.Application.Cartoes.Commands.DeleteParcela;

public record DeleteParcelaCommand(Guid Id) : IRequest;
