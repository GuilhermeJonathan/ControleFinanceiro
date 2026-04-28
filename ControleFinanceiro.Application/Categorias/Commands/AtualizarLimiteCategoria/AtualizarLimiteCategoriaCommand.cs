using MediatR;

namespace ControleFinanceiro.Application.Categorias.Commands.AtualizarLimiteCategoria;

public record AtualizarLimiteCategoriaCommand(Guid Id, decimal? LimiteMensal) : IRequest;
