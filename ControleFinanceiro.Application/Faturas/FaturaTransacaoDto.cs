namespace ControleFinanceiro.Application.Faturas;

public record FaturaTransacaoDto(
    string Descricao,
    DateTime Data,
    decimal Valor,
    int Mes,
    int Ano,
    int? ParcelaAtual,
    int? TotalParcelas,
    string SecaoCartao,
    string TitularCartao,
    string CategoriaNome   // da col E do Excel; "Outros" se vazia/ausente
);
