using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.SaldoContas.Commands.UpdateConta;

public class UpdateContaCommandHandler(
    ISaldoContaRepository repository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<UpdateContaCommand>
{
    public async Task Handle(UpdateContaCommand request, CancellationToken cancellationToken)
    {
        var conta = await repository.GetByIdAsync(request.Id, currentUser.UserId, cancellationToken)
            ?? throw new KeyNotFoundException($"Conta {request.Id} não encontrada.");

        conta.Atualizar(request.Banco, request.Saldo, request.Tipo);
        repository.Update(conta);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
