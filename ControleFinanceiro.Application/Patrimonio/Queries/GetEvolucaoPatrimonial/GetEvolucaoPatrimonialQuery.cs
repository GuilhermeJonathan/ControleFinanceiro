using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Patrimonio.Queries.GetEvolucaoPatrimonial;

public record EvolucaoPontoDto(int Ano, int Mes, decimal PatrimonioLiquidoBRL, decimal TotalBensBRL, decimal TotalDividasBRL);

/// <summary>Série mensal do patrimônio do usuário efetivo (para o gráfico de evolução).</summary>
public record GetEvolucaoPatrimonialQuery(int Meses = 12) : IRequest<IEnumerable<EvolucaoPontoDto>>;

public class GetEvolucaoPatrimonialQueryHandler(
    IPatrimonioSnapshotRepository repository,
    ICurrentUser currentUser)
    : IRequestHandler<GetEvolucaoPatrimonialQuery, IEnumerable<EvolucaoPontoDto>>
{
    public async Task<IEnumerable<EvolucaoPontoDto>> Handle(GetEvolucaoPatrimonialQuery request, CancellationToken cancellationToken)
    {
        var meses = request.Meses is > 0 and <= 60 ? request.Meses : 12;
        var snaps = await repository.GetByUsuarioAsync(currentUser.UserId, meses, cancellationToken);
        // Repositório retorna do mais recente ao mais antigo → inverte para cronológico (gráfico).
        return snaps
            .OrderBy(s => s.Ano * 100 + s.Mes)
            .Select(s => new EvolucaoPontoDto(s.Ano, s.Mes, s.PatrimonioLiquidoBRL, s.TotalBensBRL, s.TotalDividasBRL))
            .ToList();
    }
}
