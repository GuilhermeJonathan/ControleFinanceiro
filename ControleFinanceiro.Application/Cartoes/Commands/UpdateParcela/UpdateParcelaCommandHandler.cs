using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Cartoes.Commands.UpdateParcela;

public class UpdateParcelaCommandHandler(IParcelaCartaoRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateParcelaCommand>
{
    public async Task Handle(UpdateParcelaCommand request, CancellationToken cancellationToken)
    {
        var parcela = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Parcela {request.Id} não encontrada.");

        parcela.Update(request.Descricao, request.ValorParcela, request.ParcelaAtual, request.TotalParcelas);
        repository.Update(parcela);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
