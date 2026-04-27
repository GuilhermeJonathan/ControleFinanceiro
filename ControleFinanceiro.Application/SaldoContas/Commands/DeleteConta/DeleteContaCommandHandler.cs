using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.SaldoContas.Commands.DeleteConta;

public class DeleteContaCommandHandler(
    ISaldoContaRepository repository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<DeleteContaCommand>
{
    public async Task Handle(DeleteContaCommand request, CancellationToken cancellationToken)
    {
        var conta = await repository.GetByIdAsync(request.Id, currentUser.UserId, cancellationToken)
            ?? throw new KeyNotFoundException($"Conta {request.Id} não encontrada.");

        repository.Delete(conta);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
