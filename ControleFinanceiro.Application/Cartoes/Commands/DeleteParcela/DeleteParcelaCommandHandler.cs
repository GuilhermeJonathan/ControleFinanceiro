using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Cartoes.Commands.DeleteParcela;

public class DeleteParcelaCommandHandler(IParcelaCartaoRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteParcelaCommand>
{
    public async Task Handle(DeleteParcelaCommand request, CancellationToken cancellationToken)
    {
        var parcela = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Parcela {request.Id} não encontrada.");

        repository.Delete(parcela);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
