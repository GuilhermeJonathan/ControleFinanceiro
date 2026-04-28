namespace ControleFinanceiro.Application.Lancamentos.Queries.GetDashboard;

public record DashboardDto(
    int Mes,
    int Ano,
    decimal TotalCreditos,
    decimal TotalDebitos,
    decimal Saldo,
    IEnumerable<ResumoCategoriaDto> ResumoDebitos,
    // Variação percentual vs mês anterior (null = sem dados anteriores)
    decimal? VariacaoCreditos,
    decimal? VariacaoDebitos,
    decimal? VariacaoSaldo,
    // Saúde financeira
    int?     DiasReserva,          // Total saldos ÷ (gasto médio diário). Null se sem saldo.
    decimal? ComprometimentoRenda  // Débitos ÷ Créditos × 100. Null se sem receita.
);

public record ResumoCategoriaDto(string Categoria, decimal Total);
