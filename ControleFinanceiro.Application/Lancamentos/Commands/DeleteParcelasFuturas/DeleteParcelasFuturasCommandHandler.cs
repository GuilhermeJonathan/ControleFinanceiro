using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Lancamentos.Commands.DeleteParcelasFuturas;

public class DeleteParcelasFuturasCommandHandler(ILancamentoRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteParcelasFuturasCommand>
{
    public async Task Handle(DeleteParcelasFuturasCommand request, CancellationToken cancellationToken)
    {
        var parcelas = await repository.GetByGrupoParcelasFromAsync(
            request.GrupoParcelas, request.ParcelaAtualFrom, cancellationToken);

        repository.DeleteRange(parcelas);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
