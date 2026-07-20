using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Assessoria.Commands.SaveParametrosSaude;

/// <summary>Salva (upsert) os parâmetros do termômetro do assessor logado.</summary>
public record SaveParametrosSaudeCommand(
    int ScoreExcelenteMin, int ScoreBoaMin, int ScoreAtencaoMin,
    int ComprometimentoSaudavelMax, int ComprometimentoRazoavelMax, int ComprometimentoApertadoMax,
    int ReservaExcelenteMinDias, int ReservaBoaMinDias, int ReservaCurtaMinDias) : IRequest<Unit>;

public class SaveParametrosSaudeCommandHandler(
    IParametrosSaudeRepository repository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : IRequestHandler<SaveParametrosSaudeCommand, Unit>
{
    private static int Clamp(int v, int min, int max) => Math.Max(min, Math.Min(max, v));

    public async Task<Unit> Handle(SaveParametrosSaudeCommand r, CancellationToken cancellationToken)
    {
        var assessorId = currentUser.RealUserId;
        var existente = await repository.GetByAssessorAsync(assessorId, cancellationToken);
        var alvo = existente ?? new ParametrosSaude(assessorId);

        alvo.Atualizar(
            Clamp(r.ScoreExcelenteMin, 0, 100), Clamp(r.ScoreBoaMin, 0, 100), Clamp(r.ScoreAtencaoMin, 0, 100),
            Clamp(r.ComprometimentoSaudavelMax, 0, 300), Clamp(r.ComprometimentoRazoavelMax, 0, 300), Clamp(r.ComprometimentoApertadoMax, 0, 300),
            Clamp(r.ReservaExcelenteMinDias, 0, 3650), Clamp(r.ReservaBoaMinDias, 0, 3650), Clamp(r.ReservaCurtaMinDias, 0, 3650));

        if (existente is null) await repository.AddAsync(alvo, cancellationToken);
        else repository.Update(alvo);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
