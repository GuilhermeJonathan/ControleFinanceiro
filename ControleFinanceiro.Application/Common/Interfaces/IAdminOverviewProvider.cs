using ControleFinanceiro.Application.Admin.Queries.GetAdminOverview;

namespace ControleFinanceiro.Application.Common.Interfaces;

/// <summary>
/// Agrega os dados da plataforma (assessorias, clientes, corretores, AUM) para o painel do admin.
/// Implementado na Infraestrutura com consultas EF diretas (agregação cross-entidade).
/// </summary>
public interface IAdminOverviewProvider
{
    Task<AdminOverviewDto> GetAsync(CancellationToken ct = default);
}
