using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Assessoria.Commands.ResponderRecomendacao;

public record ResponderRecomendacaoCommand(Guid RecomendacaoId, bool Aceitar, string? Comentario) : IRequest;

public class ResponderRecomendacaoCommandHandler(
    IRecomendacaoRepository repository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ResponderRecomendacaoCommand>
{
    public async Task Handle(ResponderRecomendacaoCommand request, CancellationToken cancellationToken)
    {
        var recomendacao = await repository.GetByIdAsync(request.RecomendacaoId, cancellationToken)
            ?? throw new KeyNotFoundException("Recomendação não encontrada.");

        if (recomendacao.ClienteId != currentUser.RealUserId)
            throw new UnauthorizedAccessException("Apenas o cliente destinatário pode responder.");

        recomendacao.Responder(
            request.Aceitar ? StatusRecomendacao.Aceita : StatusRecomendacao.Recusada,
            request.Comentario);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
