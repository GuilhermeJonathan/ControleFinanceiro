using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Imoveis.Commands.AddFoto;

public record AddFotoCommand(Guid ImovelId, string Dados, int Ordem) : IRequest<Guid>;

public class AddFotoCommandHandler(
    IImovelRepository repo,
    ICurrentUser currentUser,
    IUnitOfWork uow) : IRequestHandler<AddFotoCommand, Guid>
{
    public async Task<Guid> Handle(AddFotoCommand r, CancellationToken ct)
    {
        var imovel = await repo.GetByIdAsync(r.ImovelId, currentUser.UserId, currentUser.PodeVerImoveis, ct)
            ?? throw new KeyNotFoundException("Imóvel não encontrado.");

        var foto = new ImovelFoto(imovel.Id, r.Dados, r.Ordem);
        await repo.AddFotoAsync(foto, ct);
        await uow.SaveChangesAsync(ct);
        return foto.Id;
    }
}
