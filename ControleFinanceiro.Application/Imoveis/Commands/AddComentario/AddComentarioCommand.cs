using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Imoveis.Commands.AddComentario;

public record AddComentarioCommand(Guid ImovelId, string Texto) : IRequest<Guid>;

public class AddComentarioCommandHandler(
    IImovelRepository repo,
    ICurrentUser currentUser,
    IUnitOfWork uow) : IRequestHandler<AddComentarioCommand, Guid>
{
    public async Task<Guid> Handle(AddComentarioCommand r, CancellationToken ct)
    {
        var imovel = await repo.GetByIdAsync(r.ImovelId, currentUser.UserId, currentUser.PodeVerImoveis, ct)
            ?? throw new KeyNotFoundException("Imóvel não encontrado.");

        var comentario = new ImovelComentario(imovel.Id, r.Texto.Trim());
        await repo.AddComentarioAsync(comentario, ct);
        await uow.SaveChangesAsync(ct);
        return comentario.Id;
    }
}
