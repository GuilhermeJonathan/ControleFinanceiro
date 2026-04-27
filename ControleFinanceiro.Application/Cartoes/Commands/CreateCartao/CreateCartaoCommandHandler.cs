using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Cartoes.Commands.CreateCartao;

public class CreateCartaoCommandHandler(
    ICartaoCreditoRepository repository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<CreateCartaoCommand, Guid>
{
    public async Task<Guid> Handle(CreateCartaoCommand request, CancellationToken cancellationToken)
    {
        var cartao = new CartaoCredito(request.Nome, request.DiaVencimento, currentUser.UserId);
        await repository.AddAsync(cartao, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return cartao.Id;
    }
}
