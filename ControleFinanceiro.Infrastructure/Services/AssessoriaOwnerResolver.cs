using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Repositories;

namespace ControleFinanceiro.Infrastructure.Services;

/// <inheritdoc />
public class AssessoriaOwnerResolver(
    ICurrentUser currentUser,
    IVinculoAssessoriaRepository vinculoAssessoriaRepo,
    IVinculoCorretorRepository vinculoCorretorRepo)
    : IAssessoriaOwnerResolver
{
    public async Task<Guid?> ResolveOwnerAsync(CancellationToken ct = default)
    {
        // Admin gerencia o catálogo global — não tem "dono".
        if (currentUser.IsAdmin)
            return null;

        // Assessor: ele próprio é o tenant. (RealUserId ignora o view-as.)
        if (currentUser.IsAssessor)
            return currentUser.RealUserId;

        // Corretor: herda o catálogo do assessor ao qual está vinculado.
        if (currentUser.IsCorretor)
        {
            var vinculos = await vinculoCorretorRepo.GetByCorretorAsync(currentUser.RealUserId, ct);
            var ativo = vinculos.FirstOrDefault(v => v.AceitoEm != null && v.RevogadoEm == null);
            return ativo?.AssessorId;
        }

        // Cliente: herda o catálogo do assessor que o atende (se houver).
        var vinculo = await vinculoAssessoriaRepo.GetByClienteAsync(currentUser.RealUserId, ct);
        return vinculo?.AssessorId;
    }
}
