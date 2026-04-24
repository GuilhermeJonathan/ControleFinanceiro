using MediatR;

namespace ControleFinanceiro.Application.Horas.Commands.UpdateHoras;

public record UpdateHorasCommand(Guid Id, string Descricao, decimal ValorHora, decimal Quantidade) : IRequest;
