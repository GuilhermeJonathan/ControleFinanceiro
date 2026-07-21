namespace ControleFinanceiro.Application.Common.Interfaces;

/// <summary>
/// Resolve o "assessor dono" (tenant raiz) do usuário atual, usado para escopar
/// os parâmetros com override por assessoria (tipos de ativo/investimento).
///   - Admin        → null (opera sobre o catálogo global).
///   - Assessor     → ele mesmo (RealUserId).
///   - Corretor     → o assessor ao qual está vinculado.
///   - Cliente      → o assessor que o atende (ou null se não tiver).
/// </summary>
public interface IAssessoriaOwnerResolver
{
    Task<Guid?> ResolveOwnerAsync(CancellationToken ct = default);
}
