using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Cartoes.Commands.DeleteCartao;

public class DeleteCartaoCommandHandler(ICartaoCreditoRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteCartaoCommand>
{
    public async Task Handle(DeleteCartaoCommand request, CancellationToken cancellationToken)
    {
        var cartao = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Cartão {request.Id} não encontrado.");

        repository.Delete(cartao);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
