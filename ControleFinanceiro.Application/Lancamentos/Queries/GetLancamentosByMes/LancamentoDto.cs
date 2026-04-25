using ControleFinanceiro.Domain.Enums;

namespace ControleFinanceiro.Application.Lancamentos.Queries.GetLancamentosByMes;

public record LancamentoDto(
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
    Guid? GrupoParcelas,
    int? CartaoDiaVencimento,
    // Receita recorrente
    Guid? ReceitaRecorrenteId,
    TipoReceita? ReceitaTipo,
    decimal? ReceitaValorHora,
    decimal? ReceitaQuantidadeHoras,
    // Recorrente
    bool IsRecorrente,
    // Conta bancária
    Guid? ContaBancariaId,
    string? ContaBancariaNome,
    // Pagamento
    DateTime? DataPagamento
);
