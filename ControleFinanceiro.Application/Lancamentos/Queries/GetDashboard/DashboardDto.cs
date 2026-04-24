namespace ControleFinanceiro.Application.Lancamentos.Queries.GetDashboard;

public record DashboardDto(
    int Mes,
    int Ano,
    decimal TotalCreditos,
    decimal TotalDebitos,
    decimal Saldo,
    IEnumerable<ResumoCategoriaDto> ResumoDebitos
);

public record ResumoCategoriaDto(string Categoria, decimal Total);
