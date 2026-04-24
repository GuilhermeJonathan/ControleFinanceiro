using MediatR;

namespace ControleFinanceiro.Application.Horas.Queries.GetHorasByMes;

public record GetHorasByMesQuery(int Mes, int Ano) : IRequest<IEnumerable<HorasDto>>;
