using ControleFinanceiro.Domain.Enums;
using MediatR;

namespace ControleFinanceiro.Application.Categorias.Commands.CreateCategoria;

public record CreateCategoriaCommand(string Nome, TipoLancamento Tipo, string? Icone = null, string? Cor = null) : IRequest<Guid>;
