using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Vinculos.Commands.RemoverVinculo;

public record RemoverVinculoCommand(Guid VinculoId) : IRequest;

public class RemoverVinculoCommandHandler(
    IVinculoFamiliarRepository repo,
    ICurrentUser currentUser,
    IUnitOfWork uow) : IRequestHandler<RemoverVinculoCommand>
{
    public async Task Handle(RemoverVinculoCommand request, CancellationToken ct)
    {
        var vinculo = await repo.GetByIdAsync(request.VinculoId, ct)
            ?? throw new KeyNotFoundException("Vínculo não encontrado.");

        // Só o dono ou o próprio membro podem remover
        var realId = currentUser.RealUserId;
        if (vinculo.DonoId != realId && vinculo.MembroId != realId)
            throw new UnauthorizedAccessException();

        repo.Remove(vinculo);
        await uow.SaveChangesAsync(ct);
    }
}
