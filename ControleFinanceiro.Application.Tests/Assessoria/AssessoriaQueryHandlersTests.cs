using ControleFinanceiro.Application.Assessoria.Queries.GetClientesAssessoria;
using ControleFinanceiro.Application.Assessoria.Queries.GetMeuAssessor;
using ControleFinanceiro.Application.Assessoria.Queries.GetSaudeFinanceira;
using ControleFinanceiro.Application.Categorias.Queries.GetOrcamento;
using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Lancamentos.Queries.GetDashboard;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using MediatR;
using Moq;

namespace ControleFinanceiro.Application.Tests.Assessoria;

public class AssessoriaQueryHandlersTests
{
    private readonly Mock<IVinculoAssessoriaRepository> _repoMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();

    private static readonly Guid AssessorId = Guid.NewGuid();
    private static readonly Guid ClienteId = Guid.NewGuid();

    public AssessoriaQueryHandlersTests()
    {
        _currentUserMock.Setup(c => c.RealUserId).Returns(AssessorId);
    }

    // ── GetClientes ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetClientes_ShouldReturnOnlyNonRevoked()
    {
        var pendente = VinculoAssessoria.Criar(AssessorId, "AAA111");
        var ativo = VinculoAssessoria.Criar(AssessorId, "BBB222");
        ativo.Aceitar(ClienteId, "Cliente Ativo");
        var revogado = VinculoAssessoria.Criar(AssessorId, "CCC333");
        revogado.Aceitar(Guid.NewGuid(), "Cliente Revogado");
        revogado.Revogar();

        _repoMock.Setup(r => r.GetByAssessorAsync(AssessorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { pendente, ativo, revogado });
        var lookupMock = new Mock<ControleFinanceiro.Application.Common.Interfaces.IUserNameLookup>();
        lookupMock.Setup(l => l.GetContatoAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ControleFinanceiro.Application.Common.Interfaces.UserContato("Cliente", "c@t.com", "https://avatar.png"));

        var handler = new GetClientesAssessoriaQueryHandler(_repoMock.Object, _currentUserMock.Object, lookupMock.Object);
        var result = (await handler.Handle(new GetClientesAssessoriaQuery(), CancellationToken.None)).ToList();

        result.Should().HaveCount(2);
        result.Should().Contain(c => c.CodigoConvite == "AAA111" && !c.Aceito);
        result.Should().Contain(c => c.CodigoConvite == "BBB222" && c.Ativo && c.NomeCliente == "Cliente Ativo");
    }

    // ── GetMeuAssessor ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetMeuAssessor_ComVinculoAtivo_ShouldReturnAssessor()
    {
        var vinculo = VinculoAssessoria.Criar(AssessorId, "AAA111", "Dr. Assessor");
        vinculo.Aceitar(ClienteId, "Cliente");
        _currentUserMock.Setup(c => c.RealUserId).Returns(ClienteId);
        _repoMock.Setup(r => r.GetByClienteAsync(ClienteId, It.IsAny<CancellationToken>())).ReturnsAsync(vinculo);

        var handler = new GetMeuAssessorQueryHandler(_repoMock.Object, _currentUserMock.Object);
        var result = await handler.Handle(new GetMeuAssessorQuery(), CancellationToken.None);

        result.TemAssessor.Should().BeTrue();
        result.NomeAssessor.Should().Be("Dr. Assessor");
        result.VinculoId.Should().Be(vinculo.Id);
    }

    [Fact]
    public async Task GetMeuAssessor_SemVinculo_ShouldReturnFalse()
    {
        _repoMock.Setup(r => r.GetByClienteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VinculoAssessoria?)null);

        var handler = new GetMeuAssessorQueryHandler(_repoMock.Object, _currentUserMock.Object);
        var result = await handler.Handle(new GetMeuAssessorQuery(), CancellationToken.None);

        result.TemAssessor.Should().BeFalse();
        result.VinculoId.Should().BeNull();
    }

    // ── GetSaudeFinanceira ───────────────────────────────────────────────────

    private static GetSaudeFinanceiraQueryHandler BuildSaudeHandler(
        DashboardDto dashboard, IEnumerable<OrcamentoItemDto> orcamento)
    {
        var mediatorMock = new Mock<ISender>();
        mediatorMock.Setup(m => m.Send(It.IsAny<GetDashboardQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dashboard);
        mediatorMock.Setup(m => m.Send(It.IsAny<GetOrcamentoQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(orcamento);
        return new GetSaudeFinanceiraQueryHandler(mediatorMock.Object);
    }

    private static DashboardDto BuildDashboard(
        decimal creditos, decimal debitos, decimal? variacaoSaldo, int? diasReserva, decimal? comprometimento) =>
        new(7, 2026, creditos, debitos, creditos - debitos,
            Enumerable.Empty<ResumoCategoriaDto>(),
            null, null, variacaoSaldo, diasReserva, comprometimento);

    [Fact]
    public async Task Saude_CenarioSaudavel_ShouldScoreHigh()
    {
        // 40% comprometimento, sem estouro, 100 dias reserva, tendência positiva
        var dashboard = BuildDashboard(10000, 4000, variacaoSaldo: 5, diasReserva: 100, comprometimento: 40);
        var orcamento = new[] { new OrcamentoItemDto(Guid.NewGuid(), "Mercado", 1000, 800, null, null) };

        var handler = BuildSaudeHandler(dashboard, orcamento);
        var result = await handler.Handle(new GetSaudeFinanceiraQuery(7, 2026), CancellationToken.None);

        result.ScoreGeral.Should().Be(100);
        result.Classificacao.Should().Be("Excelente");
        result.Pilares.Should().HaveCount(4);
    }

    [Fact]
    public async Task Saude_CenarioCritico_ShouldScoreLow()
    {
        // Déficit (120%), tudo estourado, sem reserva, tendência negativa
        var dashboard = BuildDashboard(1000, 1200, variacaoSaldo: -10, diasReserva: null, comprometimento: 120);
        var orcamento = new[]
        {
            new OrcamentoItemDto(Guid.NewGuid(), "Mercado", 100, 500, null, null),
            new OrcamentoItemDto(Guid.NewGuid(), "Lazer", 100, 300, null, null),
        };

        var handler = BuildSaudeHandler(dashboard, orcamento);
        var result = await handler.Handle(new GetSaudeFinanceiraQuery(7, 2026), CancellationToken.None);

        result.ScoreGeral.Should().BeLessThan(40);
        result.Classificacao.Should().Be("Crítica");
    }

    [Fact]
    public async Task Saude_SemDados_ShouldReturnNeutralScore()
    {
        // Sem receitas, sem limites, sem reserva
        var dashboard = BuildDashboard(0, 0, variacaoSaldo: null, diasReserva: null, comprometimento: null);

        var handler = BuildSaudeHandler(dashboard, Enumerable.Empty<OrcamentoItemDto>());
        var result = await handler.Handle(new GetSaudeFinanceiraQuery(7, 2026), CancellationToken.None);

        result.Pilares.Should().HaveCount(4);
        result.ScoreGeral.Should().BeInRange(0, 100);
    }
}
