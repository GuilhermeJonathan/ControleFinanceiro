using MediatR;

namespace ControleFinanceiro.Application.Horas.Commands.CreateHoras;

public record CreateHorasCommand(string Descricao, decimal ValorHora, decimal Quantidade, int Mes, int Ano) : IRequest<Guid>;
