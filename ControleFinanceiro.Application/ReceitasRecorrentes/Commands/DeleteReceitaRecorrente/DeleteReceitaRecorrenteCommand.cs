using MediatR;

namespace ControleFinanceiro.Application.ReceitasRecorrentes.Commands.DeleteReceitaRecorrente;

public record DeleteReceitaRecorrenteCommand(Guid Id) : IRequest;
