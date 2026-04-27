namespace ControleFinanceiro.Application.Lancamentos.Queries.GetResumoAnual;

public record ResumoMesDto(
    int Mes,
    decimal TotalCreditos,
    decimal TotalDebitos,
    decimal Saldo
);

public record ResumoCatAnualDto(
    string Categoria,
    decimal Total
);

public record ResumoAnualDto(
    int Ano,
    decimal TotalCreditos,
    decimal TotalDebitos,
    decimal Saldo,
    IEnumerable<ResumoMesDto> Meses,
    IEnumerable<ResumoCatAnualDto> TopCategorias
);
