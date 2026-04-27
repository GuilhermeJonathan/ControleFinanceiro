using MediatR;

namespace ControleFinanceiro.Application.Lancamentos.Queries.GetParceladosVigentes;

public record GetParceladosVigentesQuery : IRequest<ParceladosVigentesResultDto>;
