using MediatR;

namespace ControleFinanceiro.Application.Lancamentos.Queries.GetProjecao;

public record GetProjecaoQuery(int MesBase, int AnoBase) : IRequest<IEnumerable<ProjecaoMesDto>>;
