using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Lancamentos.Commands.DeleteParcelasFuturas;

public class DeleteParcelasFuturasCommandHandler(
    ILancamentoRepository repository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<DeleteParcelasFuturasCommand>
{
    public async Task Handle(DeleteParcelasFuturasCommand request, CancellationToken cancellationToken)
    {
        var parcelas = await repository.GetByGrupoParcelasFromAsync(
            request.GrupoParcelas, request.ParcelaAtualFrom, currentUser.UserId, cancellationToken);

        repository.DeleteRange(parcelas);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
