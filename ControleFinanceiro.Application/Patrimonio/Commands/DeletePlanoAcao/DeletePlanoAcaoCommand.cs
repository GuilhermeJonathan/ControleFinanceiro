using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Patrimonio.Commands.DeletePlanoAcao;

/// <summary>Exclui um plano de ação do usuário efetivo (com verificação de posse).</summary>
public record DeletePlanoAcaoCommand(Guid Id) : IRequest<Unit>;

public class DeletePlanoAcaoCommandHandler(
    IPlanoAcaoRepository repository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeletePlanoAcaoCommand, Unit>
{
    public async Task<Unit> Handle(DeletePlanoAcaoCommand request, CancellationToken cancellationToken)
    {
        var plano = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (plano is null || plano.UsuarioId != currentUser.UserId)
            throw new KeyNotFoundException("Plano não encontrado.");

        repository.Remove(plano);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
