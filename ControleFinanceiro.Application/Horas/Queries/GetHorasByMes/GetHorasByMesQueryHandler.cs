using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Horas.Queries.GetHorasByMes;

public class GetHorasByMesQueryHandler(IHorasTrabalhadasRepository repository, ICurrentUser currentUser)
    : IRequestHandler<GetHorasByMesQuery, IEnumerable<HorasDto>>
{
    public async Task<IEnumerable<HorasDto>> Handle(GetHorasByMesQuery request, CancellationToken cancellationToken)
    {
        var horas = await repository.GetByMesAnoAsync(request.Mes, request.Ano, currentUser.UserId, cancellationToken);
        return horas.Select(h => new HorasDto(h.Id, h.Descricao, h.ValorHora, h.Quantidade, h.ValorTotal, h.Mes, h.Ano));
    }
}
