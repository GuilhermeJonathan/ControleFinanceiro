using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Lancamentos.Commands.DeleteGrupoParcelas;

public class DeleteGrupoParcelasCommandHandler(
    ILancamentoRepository repository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<DeleteGrupoParcelasCommand>
{
    public async Task Handle(DeleteGrupoParcelasCommand request, CancellationToken cancellationToken)
    {
        var parcelas = await repository.GetByGrupoParcelasAsync(
            request.GrupoParcelas, currentUser.UserId, cancellationToken);

        repository.DeleteRange(parcelas);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
