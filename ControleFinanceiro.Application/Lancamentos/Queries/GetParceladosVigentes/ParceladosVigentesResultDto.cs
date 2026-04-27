namespace ControleFinanceiro.Application.Lancamentos.Queries.GetParceladosVigentes;

public record ParceladoVigenteItemDto(
    string Descricao,
    string? CategoriaNome,
    string? CartaoNome,
    DateTime PrimeiraData,
    int ParcelaMin,
    int TotalParcelas,
    decimal ValorParcela,
    decimal SaldoRestante      // soma das parcelas vigentes deste grupo
);

public record ParceladosVigentesResultDto(
    decimal TotalDivida,
    IEnumerable<ParceladoVigenteItemDto> Itens
);
