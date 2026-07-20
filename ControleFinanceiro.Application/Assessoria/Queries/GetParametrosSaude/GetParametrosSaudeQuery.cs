using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Assessoria.Queries.GetParametrosSaude;

public record ParametrosSaudeDto(
    int ScoreExcelenteMin, int ScoreBoaMin, int ScoreAtencaoMin,
    int ComprometimentoSaudavelMax, int ComprometimentoRazoavelMax, int ComprometimentoApertadoMax,
    int ReservaExcelenteMinDias, int ReservaBoaMinDias, int ReservaCurtaMinDias);

/// <summary>Parâmetros do termômetro do assessor logado (ou os padrões, se não configurou).</summary>
public record GetParametrosSaudeQuery : IRequest<ParametrosSaudeDto>;

public class GetParametrosSaudeQueryHandler(
    IParametrosSaudeRepository repository,
    ICurrentUser currentUser)
    : IRequestHandler<GetParametrosSaudeQuery, ParametrosSaudeDto>
{
    public async Task<ParametrosSaudeDto> Handle(GetParametrosSaudeQuery request, CancellationToken cancellationToken)
    {
        var p = await repository.GetByAssessorAsync(currentUser.RealUserId, cancellationToken) ?? ParametrosSaude.Padrao();
        return new ParametrosSaudeDto(
            p.ScoreExcelenteMin, p.ScoreBoaMin, p.ScoreAtencaoMin,
            p.ComprometimentoSaudavelMax, p.ComprometimentoRazoavelMax, p.ComprometimentoApertadoMax,
            p.ReservaExcelenteMinDias, p.ReservaBoaMinDias, p.ReservaCurtaMinDias);
    }
}
