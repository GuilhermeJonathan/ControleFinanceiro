using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.SaldoContas.Commands.CreateConta;

public class CreateContaCommandHandler(ISaldoContaRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateContaCommand, Guid>
{
    public async Task<Guid> Handle(CreateContaCommand request, CancellationToken cancellationToken)
    {
        var conta = new SaldoConta(request.Banco, request.SaldoInicial, request.Tipo);
        await repository.AddAsync(conta, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return conta.Id;
    }
}
