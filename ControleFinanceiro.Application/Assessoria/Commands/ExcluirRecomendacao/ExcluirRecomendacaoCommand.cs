using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Assessoria.Commands.ExcluirRecomendacao;

public record ExcluirRecomendacaoCommand(Guid RecomendacaoId) : IRequest;

public class ExcluirRecomendacaoCommandHandler(
    IRecomendacaoRepository repository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ExcluirRecomendacaoCommand>
{
    public async Task Handle(ExcluirRecomendacaoCommand request, CancellationToken cancellationToken)
    {
        var recomendacao = await repository.GetByIdAsync(request.RecomendacaoId, cancellationToken)
            ?? throw new KeyNotFoundException("Recomendação não encontrada.");

        if (recomendacao.AssessorId != currentUser.RealUserId)
            throw new UnauthorizedAccessException("Apenas o assessor autor pode excluir a recomendação.");

        if (recomendacao.Status != StatusRecomendacao.Pendente)
            throw new InvalidOperationException("Recomendações já respondidas não podem ser excluídas.");

        repository.Remove(recomendacao);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
