using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Assessoria.Commands.RevogarVinculoAssessoria;

public record RevogarVinculoAssessoriaCommand(Guid VinculoId) : IRequest;

public class RevogarVinculoAssessoriaCommandHandler(
    IVinculoAssessoriaRepository repository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RevogarVinculoAssessoriaCommand>
{
    public async Task Handle(RevogarVinculoAssessoriaCommand request, CancellationToken cancellationToken)
    {
        var vinculo = await repository.GetByIdAsync(request.VinculoId, cancellationToken)
            ?? throw new KeyNotFoundException("Vínculo não encontrado.");

        var caller = currentUser.RealUserId;
        if (vinculo.AssessorId != caller && vinculo.ClienteId != caller)
            throw new UnauthorizedAccessException("Apenas o assessor ou o cliente podem revogar o vínculo.");

        vinculo.Revogar();
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
