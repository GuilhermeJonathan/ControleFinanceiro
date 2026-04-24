using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.SaldoContas.Commands.UpsertSaldo;

public class UpsertSaldoCommandHandler(ISaldoContaRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<UpsertSaldoCommand, Guid>
{
    public async Task<Guid> Handle(UpsertSaldoCommand request, CancellationToken cancellationToken)
    {
        var existente = await repository.GetByBancoAsync(request.Banco, cancellationToken);

        if (existente is not null)
        {
            existente.AtualizarSaldo(request.Saldo);
            repository.Update(existente);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return existente.Id;
        }

        var novo = new SaldoConta(request.Banco, request.Saldo, TipoConta.ContaCorrente);
        await repository.AddAsync(novo, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return novo.Id;
    }
}
