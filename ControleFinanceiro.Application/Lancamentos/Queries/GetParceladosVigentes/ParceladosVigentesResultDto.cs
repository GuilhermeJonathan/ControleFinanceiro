namespace ControleFinanceiro.Application.Lancamentos.Queries.GetParceladosVigentes;

public record ParceladoVigenteItemDto(
    string Descricao,
    string? CategoriaNome,
    string? CartaoNome,
    DateTime PrimeiraData,
    DateTime UltimaData,       // data da última parcela vigente = data real da quitação
    int ParcelaMin,
    int TotalParcelas,
    decimal ValorParcela,
    decimal SaldoRestante      // soma das parcelas vigentes deste grupo
);

public record ParceladosVigentesResultDto(
    decimal TotalDivida,
    IEnumerable<ParceladoVigenteItemDto> Itens
);
