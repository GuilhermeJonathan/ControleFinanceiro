using ControleFinanceiro.Application.Categorias.Queries.GetOrcamento;
using ControleFinanceiro.Application.Lancamentos.Queries.GetDashboard;
using MediatR;

namespace ControleFinanceiro.Application.Assessoria.Queries.GetSaudeFinanceira;

public record PilarSaudeDto(string Nome, int Pontos, int Maximo, string Detalhe);

public record SaudeFinanceiraDto(
    int ScoreGeral,          // 0-100
    string Classificacao,    // Excelente / Boa / Atenção / Crítica
    IEnumerable<PilarSaudeDto> Pilares);

public record GetSaudeFinanceiraQuery(int Mes, int Ano) : IRequest<SaudeFinanceiraDto>;

/// <summary>
/// Score de saúde financeira por regras transparentes (sem IA), 4 pilares de 0-25.
/// Usa ICurrentUser.UserId indiretamente via composição das queries existentes —
/// portanto funciona tanto para o próprio usuário quanto sob o modo view-as
/// do assessor (header X-Assessoria-Cliente).
/// </summary>
public class GetSaudeFinanceiraQueryHandler(ISender mediator)
    : IRequestHandler<GetSaudeFinanceiraQuery, SaudeFinanceiraDto>
{
    public async Task<SaudeFinanceiraDto> Handle(GetSaudeFinanceiraQuery request, CancellationToken cancellationToken)
    {
        var dashboard = await mediator.Send(new GetDashboardQuery(request.Mes, request.Ano), cancellationToken);
        var orcamento = (await mediator.Send(new GetOrcamentoQuery(request.Mes, request.Ano), cancellationToken)).ToList();

        var pilares = new List<PilarSaudeDto>
        {
            PilarComprometimento(dashboard.ComprometimentoRenda),
            PilarOrcamento(orcamento),
            PilarReserva(dashboard.DiasReserva),
            PilarTendencia(dashboard.Saldo, dashboard.VariacaoSaldo),
        };

        var score = pilares.Sum(p => p.Pontos);
        var classificacao = score switch
        {
            >= 80 => "Excelente",
            >= 60 => "Boa",
            >= 40 => "Atenção",
            _     => "Crítica"
        };

        return new SaudeFinanceiraDto(score, classificacao, pilares);
    }

    private static PilarSaudeDto PilarComprometimento(decimal? comprometimento)
    {
        var (pontos, detalhe) = comprometimento switch
        {
            null       => (12, "Sem receitas registradas no mês — não foi possível avaliar."),
            <= 50m     => (25, $"Despesas consomem {comprometimento:F0}% da renda — saudável."),
            <= 70m     => (18, $"Despesas consomem {comprometimento:F0}% da renda — razoável."),
            <= 85m     => (12, $"Despesas consomem {comprometimento:F0}% da renda — margem apertada."),
            <= 100m    => (6,  $"Despesas consomem {comprometimento:F0}% da renda — quase sem sobra."),
            _          => (0,  $"Despesas superam a renda ({comprometimento:F0}%) — déficit no mês.")
        };
        return new PilarSaudeDto("Comprometimento de renda", pontos, 25, detalhe);
    }

    private static PilarSaudeDto PilarOrcamento(List<OrcamentoItemDto> orcamento)
    {
        var comLimite = orcamento.Where(o => o.LimiteMensal is > 0).ToList();
        if (comLimite.Count == 0)
            return new PilarSaudeDto("Disciplina de orçamento", 12, 25,
                "Nenhuma categoria tem limite definido — defina limites para acompanhar.");

        var estouradas = comLimite.Count(o => o.GastoAtual > o.LimiteMensal!.Value);
        var pct = (decimal)estouradas / comLimite.Count * 100;

        var (pontos, detalhe) = pct switch
        {
            0m     => (25, $"Nenhuma das {comLimite.Count} categorias com limite foi estourada."),
            <= 25m => (18, $"{estouradas} de {comLimite.Count} categorias estouraram o limite."),
            <= 50m => (10, $"{estouradas} de {comLimite.Count} categorias estouraram o limite."),
            _      => (3,  $"{estouradas} de {comLimite.Count} categorias estouraram o limite — revisar orçamento.")
        };
        return new PilarSaudeDto("Disciplina de orçamento", pontos, 25, detalhe);
    }

    private static PilarSaudeDto PilarReserva(int? diasReserva)
    {
        var (pontos, detalhe) = diasReserva switch
        {
            null or <= 0 => (0,  "Sem reserva registrada nas contas."),
            >= 90        => (25, $"Reserva cobre {diasReserva} dias de gastos — excelente."),
            >= 30        => (18, $"Reserva cobre {diasReserva} dias de gastos — boa."),
            >= 15        => (10, $"Reserva cobre {diasReserva} dias de gastos — curta."),
            _            => (5,  $"Reserva cobre apenas {diasReserva} dias de gastos.")
        };
        return new PilarSaudeDto("Reserva", pontos, 25, detalhe);
    }

    private static PilarSaudeDto PilarTendencia(decimal saldo, decimal? variacaoSaldo)
    {
        var pontos = 0;
        if (saldo >= 0) pontos += 15;
        if (variacaoSaldo is >= 0) pontos += 10;
        else if (saldo >= 0 && variacaoSaldo is null) pontos += 5;

        var detalhe = saldo >= 0
            ? variacaoSaldo is >= 0
                ? "Mês positivo e melhor que o anterior."
                : "Mês positivo, mas abaixo do anterior."
            : variacaoSaldo is >= 0
                ? "Mês negativo, porém em recuperação."
                : "Mês negativo e em piora — atenção.";

        return new PilarSaudeDto("Tendência", Math.Min(pontos, 25), 25, detalhe);
    }
}
