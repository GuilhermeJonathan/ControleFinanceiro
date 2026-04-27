using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.SaldoContas.Commands.UpsertSaldo;

public class UpsertSaldoCommandHandler(
    ISaldoContaRepository repository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<UpsertSaldoCommand, Guid>
{
    public async Task<Guid> Handle(UpsertSaldoCommand request, CancellationToken cancellationToken)
    {
        var usuarioId = currentUser.UserId;
        var existente = await repository.GetByBancoAsync(request.Banco, usuarioId, cancellationToken);

        if (existente is not null)
        {
            existente.AtualizarSaldo(request.Saldo);
            repository.Update(existente);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return existente.Id;
        }

        var novo = new SaldoConta(request.Banco, request.Saldo, TipoConta.ContaCorrente, usuarioId);
        await repository.AddAsync(novo, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return novo.Id;
    }
}
