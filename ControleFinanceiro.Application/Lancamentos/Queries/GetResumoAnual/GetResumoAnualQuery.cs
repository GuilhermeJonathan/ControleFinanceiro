using MediatR;

namespace ControleFinanceiro.Application.Lancamentos.Queries.GetResumoAnual;

public record GetResumoAnualQuery(int Ano) : IRequest<ResumoAnualDto>;
