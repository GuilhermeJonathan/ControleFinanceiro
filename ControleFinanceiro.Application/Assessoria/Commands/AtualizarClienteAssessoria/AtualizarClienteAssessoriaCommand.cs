using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Assessoria.Commands.AtualizarClienteAssessoria;

/// <summary>
/// Assessor ajusta os dados de contato que mantém do cliente (nome de exibição,
/// telefone/WhatsApp e nota interna). Só o assessor dono do vínculo pode fazê-lo.
/// </summary>
public record AtualizarClienteAssessoriaCommand(
    Guid VinculoId,
    string? NomeCliente,
    string? Telefone,
    string? Observacoes) : IRequest;

public class AtualizarClienteAssessoriaCommandHandler(
    IVinculoAssessoriaRepository repository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AtualizarClienteAssessoriaCommand>
{
    public async Task Handle(AtualizarClienteAssessoriaCommand request, CancellationToken cancellationToken)
    {
        var vinculo = await repository.GetByIdAsync(request.VinculoId, cancellationToken)
            ?? throw new KeyNotFoundException("Vínculo não encontrado.");

        if (vinculo.AssessorId != currentUser.RealUserId)
            throw new UnauthorizedAccessException("Apenas o assessor dono do vínculo pode editar os dados do cliente.");

        vinculo.AtualizarContato(request.NomeCliente, request.Telefone, request.Observacoes);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
