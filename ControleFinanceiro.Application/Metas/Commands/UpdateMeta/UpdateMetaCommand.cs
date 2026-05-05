using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Metas.Commands.UpdateMeta;

public record UpdateMetaCommand(
    Guid Id,
    string Titulo,
    string? Descricao,
    decimal ValorMeta,
    DateTime? DataMeta,
    string? Capa,
    string? CorFundo,
    decimal? ContribuicaoMensalValor = null,
    int? ContribuicaoDia = null) : IRequest;

public class UpdateMetaCommandHandler(
    IMetaRepository repo,
    IUnitOfWork uow) : IRequestHandler<UpdateMetaCommand>
{
    public async Task Handle(UpdateMetaCommand r, CancellationToken ct)
    {
        var meta = await repo.GetByIdAsync(r.Id, ct)
            ?? throw new KeyNotFoundException("Meta não encontrada.");
        meta.Atualizar(r.Titulo, r.Descricao, r.ValorMeta, r.DataMeta, r.Capa, r.CorFundo,
            r.ContribuicaoMensalValor, r.ContribuicaoDia);
        repo.Update(meta);
        await uow.SaveChangesAsync(ct);
    }
}
