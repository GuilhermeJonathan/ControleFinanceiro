using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Vendas.Queries.GetResumoVendas;

public record ResumoVendasDto(
    decimal TotalHoje,
    decimal TotalSemana,
    decimal TotalMes,
    int QtdHoje,
    int QtdSemana,
    int QtdMes);

public record GetResumoVendasQuery : IRequest<ResumoVendasDto>;

public class GetResumoVendasQueryHandler(
    IVendaRepository repo,
    ICurrentUser currentUser) : IRequestHandler<GetResumoVendasQuery, ResumoVendasDto>
{
    public async Task<ResumoVendasDto> Handle(GetResumoVendasQuery request, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var hoje = now.Date;
        var inicioSemana = hoje.AddDays(-(int)hoje.DayOfWeek);
        var inicioMes = new DateTime(now.Year, now.Month, 1);

        var vendas = await repo.GetAllAsync(inicioMes, now, null, null, ct);
        var lista = vendas.ToList();

        var vendaHoje = lista.Where(v => v.Data.Date == hoje).ToList();
        var vendaSemana = lista.Where(v => v.Data.Date >= inicioSemana).ToList();

        return new ResumoVendasDto(
            TotalHoje: vendaHoje.Sum(v => v.Valor),
            TotalSemana: vendaSemana.Sum(v => v.Valor),
            TotalMes: lista.Sum(v => v.Valor),
            QtdHoje: vendaHoje.Count,
            QtdSemana: vendaSemana.Count,
            QtdMes: lista.Count);
    }
}
