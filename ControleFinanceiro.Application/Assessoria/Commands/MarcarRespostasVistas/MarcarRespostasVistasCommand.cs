using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Assessoria.Commands.MarcarRespostasVistas;

/// <summary>Marca como vistas todas as respostas de recomendação ainda não lidas pelo assessor.</summary>
public record MarcarRespostasVistasCommand : IRequest;

public class MarcarRespostasVistasCommandHandler(
    IRecomendacaoRepository repository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : IRequestHandler<MarcarRespostasVistasCommand>
{
    public async Task Handle(MarcarRespostasVistasCommand request, CancellationToken cancellationToken)
    {
        var todas = await repository.GetByAssessorAsync(currentUser.RealUserId, cancellationToken);
        var naoVistas = todas.Where(r => r.RespostaNaoVista).ToList();
        if (naoVistas.Count == 0) return;

        foreach (var r in naoVistas)
            r.MarcarRespostaVista();

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
