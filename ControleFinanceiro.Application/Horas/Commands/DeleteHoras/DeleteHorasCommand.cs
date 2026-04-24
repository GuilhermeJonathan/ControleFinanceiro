using MediatR;

namespace ControleFinanceiro.Application.Horas.Commands.DeleteHoras;

public record DeleteHorasCommand(Guid Id) : IRequest;
