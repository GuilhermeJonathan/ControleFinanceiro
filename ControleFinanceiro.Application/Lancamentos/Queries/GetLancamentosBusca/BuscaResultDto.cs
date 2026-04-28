using ControleFinanceiro.Domain.Enums;

namespace ControleFinanceiro.Application.Lancamentos.Queries.GetLancamentosBusca;

public record BuscaResultDto(int TotalCount, IEnumerable<LancamentoBuscaItemDto> Itens);

public record LancamentoBuscaItemDto(
    Guid Id,
    string Descricao,
    DateTime Data,
    decimal Valor,
    TipoLancamento Tipo,
    SituacaoLancamento Situacao,
    int Mes,
    int Ano,
    Guid? CategoriaId,
    string? CategoriaNome,
    Guid? CartaoId,
    string? CartaoNome,
    int? ParcelaAtual,
    int? TotalParcelas,
    bool IsRecorrente,
    Guid? GrupoParcelas
);
