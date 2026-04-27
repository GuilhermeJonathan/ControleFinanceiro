using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.SaldoContas.Queries.GetSaldos;

public class GetSaldosQueryHandler(ISaldoContaRepository repository, ICurrentUser currentUser)
    : IRequestHandler<GetSaldosQuery, IEnumerable<SaldoContaDto>>
{
    public async Task<IEnumerable<SaldoContaDto>> Handle(GetSaldosQuery request, CancellationToken cancellationToken)
    {
        var saldos = await repository.GetAllAsync(currentUser.UserId, cancellationToken);
        return saldos.Select(s => new SaldoContaDto(s.Id, s.Banco, s.Saldo, s.Tipo, s.DataAtualizacao));
    }
}
