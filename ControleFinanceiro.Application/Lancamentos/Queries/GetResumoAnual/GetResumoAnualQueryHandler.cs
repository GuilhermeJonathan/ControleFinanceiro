using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Lancamentos.Queries.GetResumoAnual;

public class GetResumoAnualQueryHandler(
    ILancamentoRepository repository,
    ICurrentUser currentUser)
    : IRequestHandler<GetResumoAnualQuery, ResumoAnualDto>
{
    public async Task<ResumoAnualDto> Handle(GetResumoAnualQuery request, CancellationToken cancellationToken)
    {
        var lancamentos = (await repository.GetByAnoAsync(request.Ano, currentUser.UserId, cancellationToken)).ToList();

        // Agrega por mês
        var meses = Enumerable.Range(1, 12).Select(m =>
        {
            var doMes   = lancamentos.Where(l => l.Mes == m).ToList();
            var creditos = doMes.Where(l => l.Tipo == TipoLancamento.Credito).Sum(l => l.Valor);
            var debitos  = doMes.Where(l => l.Tipo != TipoLancamento.Credito).Sum(l => l.Valor);
            return new ResumoMesDto(m, creditos, debitos, creditos - debitos);
        }).ToList();

        // Agrega categorias de débito no ano todo
        var topCats = lancamentos
            .Where(l => l.Tipo != TipoLancamento.Credito)
            .GroupBy(l => l.Categoria?.Nome ?? "Sem Categoria")
            .Select(g => new ResumoCatAnualDto(g.Key, g.Sum(l => l.Valor)))
            .OrderByDescending(c => c.Total)
            .Take(8)
            .ToList();

        var totalCred = meses.Sum(m => m.TotalCreditos);
        var totalDeb  = meses.Sum(m => m.TotalDebitos);

        return new ResumoAnualDto(request.Ano, totalCred, totalDeb, totalCred - totalDeb, meses, topCats);
    }
}
