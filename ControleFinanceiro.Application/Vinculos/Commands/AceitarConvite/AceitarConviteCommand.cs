using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Vinculos.Commands.AceitarConvite;

public record AceitarConviteCommand(string Codigo, string NomeMembro) : IRequest;

public class AceitarConviteCommandHandler(
    IVinculoFamiliarRepository repo,
    ICurrentUser currentUser,
    IUnitOfWork uow) : IRequestHandler<AceitarConviteCommand>
{
    public async Task Handle(AceitarConviteCommand request, CancellationToken ct)
    {
        var vinculo = await repo.GetByCodigo(request.Codigo, ct)
            ?? throw new KeyNotFoundException("Código de convite inválido ou expirado.");

        if (vinculo.DonoId == currentUser.RealUserId)
            throw new InvalidOperationException("Você não pode aceitar seu próprio convite.");

        vinculo.Aceitar(currentUser.RealUserId, request.NomeMembro);
        await uow.SaveChangesAsync(ct);
    }
}
