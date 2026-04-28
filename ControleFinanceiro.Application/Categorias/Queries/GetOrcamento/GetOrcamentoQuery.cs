using MediatR;

namespace ControleFinanceiro.Application.Categorias.Queries.GetOrcamento;

public record GetOrcamentoQuery(int Mes, int Ano) : IRequest<IEnumerable<OrcamentoItemDto>>;
