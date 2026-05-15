using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Vendas.Commands.CreateVenda;

public record CreateVendaCommand(
    Guid? ProdutoId,
    string Descricao,
    decimal Valor,
    DateTime Data,
    OrigemVenda Origem = OrigemVenda.Manual) : IRequest<Guid>;

public class CreateVendaCommandHandler(
    IVendaRepository repo,
    ICurrentUser currentUser,
    IUnitOfWork uow) : IRequestHandler<CreateVendaCommand, Guid>
{
    public async Task<Guid> Handle(CreateVendaCommand r, CancellationToken ct)
    {
        var venda = new Venda(currentUser.UserId, r.ProdutoId, r.Descricao, r.Valor, r.Data, r.Origem,
            currentUser.RealUserName ?? "Desconhecido");
        await repo.AddAsync(venda, ct);
        await uow.SaveChangesAsync(ct);
        return venda.Id;
    }
}
