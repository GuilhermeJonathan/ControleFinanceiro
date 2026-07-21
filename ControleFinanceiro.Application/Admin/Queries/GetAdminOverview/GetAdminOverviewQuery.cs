using ControleFinanceiro.Application.Common.Interfaces;
using MediatR;

namespace ControleFinanceiro.Application.Admin.Queries.GetAdminOverview;

/// <summary>Resumo de uma assessoria (tenant) para o painel do admin.</summary>
public record AssessoriaResumoDto(
    Guid AssessorId,
    string Nome,
    int QtdClientes,
    int QtdCorretores,
    decimal AumBRL);

/// <summary>Visão consolidada da plataforma para o admin (acima dos assessores).</summary>
public record AdminOverviewDto(
    int QtdAssessorias,
    int QtdClientes,
    int QtdCorretores,
    decimal AumTotalBRL,
    int QtdParametrosGlobais,
    IReadOnlyList<AssessoriaResumoDto> Assessorias)
{
    public AdminOverviewDto() : this(0, 0, 0, 0m, 0, []) { }
}

public record GetAdminOverviewQuery : IRequest<AdminOverviewDto>;

public class GetAdminOverviewQueryHandler(
    ICurrentUser currentUser,
    IAdminOverviewProvider provider)
    : IRequestHandler<GetAdminOverviewQuery, AdminOverviewDto>
{
    public async Task<AdminOverviewDto> Handle(GetAdminOverviewQuery request, CancellationToken ct)
    {
        if (!currentUser.IsAdmin)
            throw new UnauthorizedAccessException("Apenas o admin da plataforma pode acessar o painel.");

        return await provider.GetAsync(ct);
    }
}
