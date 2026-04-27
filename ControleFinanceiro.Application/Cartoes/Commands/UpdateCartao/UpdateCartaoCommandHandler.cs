using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Cartoes.Commands.UpdateCartao;

public class UpdateCartaoCommandHandler(
    ICartaoCreditoRepository repository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<UpdateCartaoCommand>
{
    public async Task Handle(UpdateCartaoCommand request, CancellationToken cancellationToken)
    {
        var cartao = await repository.GetByIdAsync(request.Id, currentUser.UserId, cancellationToken)
            ?? throw new KeyNotFoundException($"Cartão {request.Id} não encontrado.");

        cartao.Update(request.Nome, request.DiaVencimento);
        repository.Update(cartao);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
