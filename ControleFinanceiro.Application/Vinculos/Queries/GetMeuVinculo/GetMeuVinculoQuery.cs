using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Vinculos.Queries.GetMeuVinculo;

public record MeuVinculoDto(bool EhMembro, Guid? DonoId, Guid? VinculoId);

public record GetMeuVinculoQuery : IRequest<MeuVinculoDto>;

public class GetMeuVinculoQueryHandler(
    IVinculoFamiliarRepository repo,
    ICurrentUser currentUser) : IRequestHandler<GetMeuVinculoQuery, MeuVinculoDto>
{
    public async Task<MeuVinculoDto> Handle(GetMeuVinculoQuery request, CancellationToken ct)
    {
        var realId = currentUser.RealUserId;
        var donoId = await repo.GetDonoIdAsync(realId, ct);

        if (donoId == null)
            return new MeuVinculoDto(false, null, null);

        // Busca o vínculo para obter o Id
        var vinculos = await repo.GetByDonoAsync(donoId.Value, ct);
        var vinculo = vinculos.FirstOrDefault(v => v.MembroId == realId && v.AceitoEm != null);

        return vinculo != null
            ? new MeuVinculoDto(true, donoId, vinculo.Id)
            : new MeuVinculoDto(false, null, null);
    }
}
