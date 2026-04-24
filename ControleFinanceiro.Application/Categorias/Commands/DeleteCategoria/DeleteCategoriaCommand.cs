using MediatR;

namespace ControleFinanceiro.Application.Categorias.Commands.DeleteCategoria;

public record DeleteCategoriaCommand(Guid Id) : IRequest;
