using ControleFinanceiro.Domain.Enums;

namespace ControleFinanceiro.Application.Cartoes.Queries.GetCartoes;

public record CartaoLancamentoDto(
    Guid Id,
    string Descricao,
    decimal Valor,
    DateTime Data,
    SituacaoLancamento Situacao,
    int? ParcelaAtual,
    int? TotalParcelas
);
