using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.ReceitasRecorrentes.Queries.GetReceitasRecorrentes;

public class GetReceitasRecorrentesQueryHandler(IReceitaRecorrenteRepository repository)
    : IRequestHandler<GetReceitasRecorrentesQuery, IEnumerable<ReceitaRecorrenteDto>>
{
    public async Task<IEnumerable<ReceitaRecorrenteDto>> Handle(
        GetReceitasRecorrentesQuery request, CancellationToken cancellationToken)
    {
        var receitas = await repository.GetAllAsync(cancellationToken);
        return receitas.Select(r => new ReceitaRecorrenteDto(
            r.Id, r.Nome, r.Tipo, r.Valor, r.ValorHora, r.QuantidadeHoras, r.Dia, r.DataInicio));
    }
}
