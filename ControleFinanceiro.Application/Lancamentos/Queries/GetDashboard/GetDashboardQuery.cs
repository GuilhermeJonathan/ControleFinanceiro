using MediatR;

namespace ControleFinanceiro.Application.Lancamentos.Queries.GetDashboard;

public record GetDashboardQuery(int Mes, int Ano) : IRequest<DashboardDto>;
