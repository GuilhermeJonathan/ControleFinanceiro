using MediatR;

namespace ControleFinanceiro.Application.ReceitasRecorrentes.Queries.GetReceitasRecorrentes;

public record GetReceitasRecorrentesQuery() : IRequest<IEnumerable<ReceitaRecorrenteDto>>;
