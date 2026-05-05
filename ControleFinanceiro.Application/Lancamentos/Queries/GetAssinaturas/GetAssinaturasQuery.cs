using MediatR;

namespace ControleFinanceiro.Application.Lancamentos.Queries.GetAssinaturas;

public record AssinaturaDto(
    string GrupoId,
    string Descricao,
    decimal ValorMensal,
    string? CategoriaNome,
    string? CategoriaIcone,
    string? CategoriaCor,
    DateTime? ProximoVencimento,
    int TotalLancamentos,
    int LancamentosPagos);

public record GetAssinaturasQuery : IRequest<List<AssinaturaDto>>;
