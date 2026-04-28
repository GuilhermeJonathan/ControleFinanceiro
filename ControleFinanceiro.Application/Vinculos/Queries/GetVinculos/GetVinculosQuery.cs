using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Vinculos.Queries.GetVinculos;

public record VinculoDto(Guid Id, string NomeMembro, bool Aceito, DateTime CriadoEm);

public record GetVinculosQuery : IRequest<List<VinculoDto>>;

public class GetVinculosQueryHandler(
    IVinculoFamiliarRepository repo,
    ICurrentUser currentUser) : IRequestHandler<GetVinculosQuery, List<VinculoDto>>
{
    public async Task<List<VinculoDto>> Handle(GetVinculosQuery request, CancellationToken ct)
    {
        var vinculos = await repo.GetByDonoAsync(currentUser.RealUserId, ct);
        return vinculos.Select(v => new VinculoDto(
            v.Id,
            v.NomeMembro ?? "(aguardando aceite)",
            v.AceitoEm != null,
            v.CriadoEm
        )).ToList();
    }
}
