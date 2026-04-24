namespace ControleFinanceiro.Application.Cartoes.Queries.GetCartoes;

public record CartaoDto(
    Guid Id,
    string Nome,
    int? DiaVencimento,
    decimal TotalMes,
    IEnumerable<CartaoLancamentoDto> Lancamentos
);
