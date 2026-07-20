using ControleFinanceiro.Application.Categorias.Queries.GetOrcamento;
using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Lancamentos.Queries.GetDashboard;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
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
/// Os limites vêm dos parâmetros do assessor (ParametrosSaude) — ou dos padrões.
/// Usa a composição das queries existentes, funcionando também sob view-as.
/// </summary>
public class GetSaudeFinanceiraQueryHandler(
    ISender mediator,
    IParametrosSaudeRepository parametrosRepository,
    ICurrentUser currentUser)
    : IRequestHandler<GetSaudeFinanceiraQuery, SaudeFinanceiraDto>
{
    public async Task<SaudeFinanceiraDto> Handle(GetSaudeFinanceiraQuery request, CancellationToken cancellationToken)
    {
        var dashboard = await mediator.Send(new GetDashboardQuery(request.Mes, request.Ano), cancellationToken);
        var orcamento = (await mediator.Send(new GetOrcamentoQuery(request.Mes, request.Ano), cancellationToken)).ToList();

        var semDados = dashboard.TotalCreditos == 0
            && dashboard.TotalDebitos == 0
            && (dashboard.DiasReserva is null or <= 0)
            && !orcamento.Any(o => o.LimiteMensal is > 0);

        if (semDados)
            return new SaudeFinanceiraDto(0, "Sem dados", new List<PilarSaudeDto>());

        // Limites configuráveis pelo assessor (consultoria); padrão se não houver.
        var p = await parametrosRepository.GetByAssessorAsync(currentUser.RealUserId, cancellationToken)
                ?? ParametrosSaude.Padrao();

        var pilares = new List<PilarSaudeDto>
        {
            PilarComprometimento(dashboard.ComprometimentoRenda, p),
            PilarOrcamento(orcamento),
            PilarReserva(dashboard.DiasReserva, p),
            PilarTendencia(dashboard.Saldo, dashboard.VariacaoSaldo),
        };

        var score = pilares.Sum(x => x.Pontos);
        var classificacao =
            score >= p.ScoreExcelenteMin ? "Excelente" :
            score >= p.ScoreBoaMin       ? "Boa" :
            score >= p.ScoreAtencaoMin   ? "Atenção" : "Crítica";

        return new SaudeFinanceiraDto(score, classificacao, pilares);
    }

    private static PilarSaudeDto PilarComprometimento(decimal? comprometimento, ParametrosSaude p)
    {
        int pontos; string detalhe;
        if (comprometimento is null) { pontos = 12; detalhe = "Sem receitas registradas no mês — não foi possível avaliar."; }
        else if (comprometimento <= p.ComprometimentoSaudavelMax) { pontos = 25; detalhe = $"Despesas consomem {comprometimento:F0}% da renda — saudável."; }
        else if (comprometimento <= p.ComprometimentoRazoavelMax) { pontos = 18; detalhe = $"Despesas consomem {comprometimento:F0}% da renda — razoável."; }
        else if (comprometimento <= p.ComprometimentoApertadoMax) { pontos = 12; detalhe = $"Despesas consomem {comprometimento:F0}% da renda — margem apertada."; }
        else if (comprometimento <= 100m) { pontos = 6; detalhe = $"Despesas consomem {comprometimento:F0}% da renda — quase sem sobra."; }
        else { pontos = 0; detalhe = $"Despesas superam a renda ({comprometimento:F0}%) — déficit no mês."; }
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

    private static PilarSaudeDto PilarReserva(int? diasReserva, ParametrosSaude p)
    {
        int pontos; string detalhe;
        if (diasReserva is null or <= 0) { pontos = 0; detalhe = "Sem reserva registrada nas contas."; }
        else if (diasReserva >= p.ReservaExcelenteMinDias) { pontos = 25; detalhe = $"Reserva cobre {diasReserva} dias de gastos — excelente."; }
        else if (diasReserva >= p.ReservaBoaMinDias) { pontos = 18; detalhe = $"Reserva cobre {diasReserva} dias de gastos — boa."; }
        else if (diasReserva >= p.ReservaCurtaMinDias) { pontos = 10; detalhe = $"Reserva cobre {diasReserva} dias de gastos — curta."; }
        else { pontos = 5; detalhe = $"Reserva cobre apenas {diasReserva} dias de gastos."; }
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
