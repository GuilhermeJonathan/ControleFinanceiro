using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Metas.Commands.CreateMeta;

public record CreateMetaCommand(
    string Titulo,
    string? Descricao,
    decimal ValorMeta,
    DateTime? DataMeta,
    string? Capa,
    string? CorFundo) : IRequest<Guid>;

public class CreateMetaCommandHandler(
    IMetaRepository repo,
    ICurrentUser currentUser,
    IUnitOfWork uow) : IRequestHandler<CreateMetaCommand, Guid>
{
    public async Task<Guid> Handle(CreateMetaCommand r, CancellationToken ct)
    {
        var meta = new Meta(currentUser.UserId, r.Titulo, r.Descricao, r.ValorMeta, r.DataMeta, r.Capa, r.CorFundo);
        await repo.AddAsync(meta, ct);
        await uow.SaveChangesAsync(ct);
        return meta.Id;
    }
}
