namespace ControleFinanceiro.Application.Categorias.Queries.GetOrcamento;

public record OrcamentoItemDto(
    Guid CategoriaId,
    string CategoriaNome,
    decimal? LimiteMensal,
    decimal GastoAtual,
    string? CategoriaIcone,
    string? CategoriaCor
);
