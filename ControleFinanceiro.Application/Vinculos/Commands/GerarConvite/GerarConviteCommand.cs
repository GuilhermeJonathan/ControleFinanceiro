using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Vinculos.Commands.GerarConvite;

public record GerarConviteCommand : IRequest<string>;

public class GerarConviteCommandHandler(
    IVinculoFamiliarRepository repo,
    ICurrentUser currentUser,
    IUnitOfWork uow) : IRequestHandler<GerarConviteCommand, string>
{
    public async Task<string> Handle(GerarConviteCommand request, CancellationToken ct)
    {
        // Gera código único de 6 chars
        string codigo;
        do
        {
            codigo = GenerateCodigo();
        } while (await repo.GetByCodigo(codigo, ct) != null);

        var vinculo = VinculoFamiliar.Criar(currentUser.RealUserId, codigo);
        await repo.AddAsync(vinculo, ct);
        await uow.SaveChangesAsync(ct);
        return codigo;
    }

    private static string GenerateCodigo()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var rng = Random.Shared;
        return new string(Enumerable.Range(0, 6).Select(_ => chars[rng.Next(chars.Length)]).ToArray());
    }
}
