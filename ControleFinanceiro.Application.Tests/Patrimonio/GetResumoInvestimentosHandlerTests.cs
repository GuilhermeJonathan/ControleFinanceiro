using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Patrimonio.Queries.GetResumoInvestimentos;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Patrimonio;

public class GetResumoInvestimentosHandlerTests
{
    private readonly Mock<IInvestimentoRepository> _repo = new();
    private readonly Mock<IFxRateResolver> _fx = new();
    private readonly Mock<ICurrentUser> _user = new();
    private static readonly Guid UserId = Guid.NewGuid();

    public GetResumoInvestimentosHandlerTests()
    {
        _user.Setup(u => u.UserId).Returns(UserId);
        _fx.Setup(f => f.GetRatesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase) { ["BRL"] = 1m });
    }

    private static Investimento Inv(decimal aplicado, decimal atual, DateTime criadoEm, decimal? anualManual = null)
    {
        var i = new Investimento(UserId, "X", TipoInvestimento.Acoes, MoedaPatrimonio.BRL, null, null, aplicado, atual, anualManual);
        typeof(Investimento).GetProperty(nameof(Investimento.CriadoEm))!.SetValue(i, criadoEm);
        return i;
    }

    [Fact]
    public async Task Retorno_Acumulado_E_Anualizado()
    {
        // dobrou em ~2 anos → acumulado 100%, anualizado ≈ 41,42%
        var inv = Inv(100_000m, 200_000m, DateTime.UtcNow.AddDays(-730));
        _repo.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync(new[] { inv });

        var h = new GetResumoInvestimentosQueryHandler(_repo.Object, _fx.Object, _user.Object);
        var r = await h.Handle(new GetResumoInvestimentosQuery(), CancellationToken.None);

        var dto = r.Investimentos.Single();
        dto.RetornoTotalPct.Should().Be(100m);
        dto.RetornoAnualPct.Should().BeInRange(40m, 43m);
        r.RetornoTotalAnualPct.Should().BeInRange(40m, 43m);
    }

    [Fact]
    public async Task PeriodoCurto_SemRentabilidadeManual_NaoAnualiza()
    {
        var inv = Inv(100_000m, 110_000m, DateTime.UtcNow.AddDays(-30));
        _repo.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync(new[] { inv });

        var h = new GetResumoInvestimentosQueryHandler(_repo.Object, _fx.Object, _user.Object);
        var r = await h.Handle(new GetResumoInvestimentosQuery(), CancellationToken.None);

        var dto = r.Investimentos.Single();
        dto.RetornoTotalPct.Should().Be(10m);   // acumulado ainda aparece
        dto.RetornoAnualPct.Should().BeNull();   // período curto → não anualiza
        r.RetornoTotalAnualPct.Should().BeNull();
    }

    [Fact]
    public async Task UsaRentabilidadeManual_QuandoInformada()
    {
        var inv = Inv(100_000m, 105_000m, DateTime.UtcNow.AddDays(-30), anualManual: 12m);
        _repo.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync(new[] { inv });

        var h = new GetResumoInvestimentosQueryHandler(_repo.Object, _fx.Object, _user.Object);
        var r = await h.Handle(new GetResumoInvestimentosQuery(), CancellationToken.None);

        r.Investimentos.Single().RetornoAnualPct.Should().Be(12m);
        r.RetornoTotalAnualPct.Should().Be(12m);
    }
}
