using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Lancamentos.Queries.GetDashboard;

public class GetDashboardQueryHandler(ILancamentoRepository repository)
    : IRequestHandler<GetDashboardQuery, DashboardDto>
{
    public async Task<DashboardDto> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        var lancamentos = (await repository.GetByMesAnoAsync(request.Mes, request.Ano, cancellationToken)).ToList();

        var creditos = lancamentos.Where(l => l.Tipo == TipoLancamento.Credito).Sum(l => l.Valor);
        var debitos = lancamentos.Where(l => l.Tipo == TipoLancamento.Debito || l.Tipo == TipoLancamento.Pix).Sum(l => l.Valor);

        var resumo = lancamentos
            .Where(l => l.Tipo == TipoLancamento.Debito || l.Tipo == TipoLancamento.Pix)
            .GroupBy(l => l.Categoria?.Nome ?? "Sem Categoria")
            .Select(g => new ResumoCategoriaDto(g.Key, g.Sum(l => l.Valor)))
            .OrderByDescending(r => r.Total);

        return new DashboardDto(request.Mes, request.Ano, creditos, debitos, creditos - debitos, resumo);
    }
}
