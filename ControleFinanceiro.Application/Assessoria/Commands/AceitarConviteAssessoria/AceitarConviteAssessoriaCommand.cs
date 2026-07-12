using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Assessoria.Commands.AceitarConviteAssessoria;

public record AceitarConviteAssessoriaCommand(string Codigo, string NomeCliente) : IRequest;

public class AceitarConviteAssessoriaCommandHandler(
    IVinculoAssessoriaRepository repository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AceitarConviteAssessoriaCommand>
{
    public async Task Handle(AceitarConviteAssessoriaCommand request, CancellationToken cancellationToken)
    {
        var vinculo = await repository.GetByCodigoAsync(request.Codigo, cancellationToken)
            ?? throw new KeyNotFoundException("Código de convite inválido.");

        var existente = await repository.GetByClienteAsync(currentUser.RealUserId, cancellationToken);
        if (existente != null)
            throw new InvalidOperationException("Você já possui um assessor ativo. Revogue o vínculo atual antes de aceitar outro.");

        vinculo.Aceitar(currentUser.RealUserId, request.NomeCliente);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
