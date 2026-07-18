using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Patrimonio.Queries.GetResumoPatrimonial;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Patrimonio;

public class GetResumoPatrimonialQueryHandlerTests
{
    private readonly Mock<IAtivoPatrimonialRepository> _ativoRepoMock = new();
    private readonly Mock<IPassivoPatrimonialRepository> _passivoRepoMock = new();
    private readonly Mock<IMoedaParamRepository> _moedaRepoMock = new();
    private readonly Mock<IPatrimonioSnapshotRepository> _snapshotRepoMock = new();
    private readonly Mock<ControleFinanceiro.Domain.Common.IUnitOfWork> _uowMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private static readonly Guid UserId = Guid.NewGuid();

    public GetResumoPatrimonialQueryHandlerTests()
    {
        _currentUserMock.Setup(c => c.UserId).Returns(UserId);
        // Por padrão, sem dívidas — cada teste sobrescreve quando precisa.
        _passivoRepoMock.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<PassivoPatrimonial>());
        // Cotações (antes fixas no código) agora vêm dos parâmetros de moeda.
        _moedaRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MoedaParam>
            {
                new(1, "BRL", "Real",   1, true, 1.00m),
                new(2, "USD", "Dólar",  2, true, 5.40m),
                new(3, "EUR", "Euro",   3, true, 5.90m),
                new(4, "CHF", "Franco", 4, true, 6.10m),
                new(5, "GBP", "Libra",  5, true, 6.90m),
            });
    }

    private GetResumoPatrimonialQueryHandler CreateHandler() =>
        new(_ativoRepoMock.Object, _passivoRepoMock.Object, _moedaRepoMock.Object,
            _snapshotRepoMock.Object, _uowMock.Object, _currentUserMock.Object);

    [Fact]
    public async Task Handle_MultiMoeda_ShouldGroupAndConsolidate()
    {
        var ativos = new[]
        {
            new AtivoPatrimonial(UserId, "Apartamento SP", TipoAtivo.Imovel,       MoedaPatrimonio.BRL, 2_000_000m, 8m),
            new AtivoPatrimonial(UserId, "Conta Suíça",    TipoAtivo.Investimento, MoedaPatrimonio.CHF, 100_000m, null),
            new AtivoPatrimonial(UserId, "ETF EUA",        TipoAtivo.Investimento, MoedaPatrimonio.USD, 50_000m, 12m),
        };
        _ativoRepoMock.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync(ativos);

        var result = await CreateHandler().Handle(new GetResumoPatrimonialQuery(), CancellationToken.None);

        result.QtdAtivos.Should().Be(3);
        result.TotaisPorMoeda.Should().HaveCount(3);
        result.CambioEstimado.Should().BeTrue();
        // BRL 2.000.000 + CHF 100k*6,10 + USD 50k*5,40 = 2.000.000 + 610.000 + 270.000 = 2.880.000
        result.TotalBensBRL.Should().Be(2_880_000m);
        result.TotalConsolidadoBRL.Should().Be(2_880_000m);
        // Sem dívidas → patrimônio líquido == bens, alavancagem 0
        result.TotalDividasBRL.Should().Be(0m);
        result.PatrimonioLiquidoBRL.Should().Be(2_880_000m);
        result.AlavancagemPct.Should().Be(0m);
    }

    [Fact]
    public async Task Handle_ComDividas_ShouldComputePatrimonioLiquidoEAlavancagem()
    {
        _ativoRepoMock.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new AtivoPatrimonial(UserId, "Imóvel", TipoAtivo.Imovel, MoedaPatrimonio.BRL, 1_000_000m, null),
            });
        _passivoRepoMock.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new PassivoPatrimonial(UserId, "Financiamento", MoedaPatrimonio.BRL, 200_000m, PrazoDivida.Longo),
            });

        var result = await CreateHandler().Handle(new GetResumoPatrimonialQuery(), CancellationToken.None);

        result.TotalBensBRL.Should().Be(1_000_000m);
        result.TotalDividasBRL.Should().Be(200_000m);
        result.PatrimonioLiquidoBRL.Should().Be(800_000m);
        result.AlavancagemPct.Should().Be(20m); // 200k / 1.000k
        result.Passivos.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_ComFluxoDeCaixa_ShouldComputeReceitaDespesaERoi()
    {
        _ativoRepoMock.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                // Recebe 10k/mês, custa 2k/mês → fluxo líquido anual 96k sobre 1.200k = 8% a.a.
                new AtivoPatrimonial(UserId, "Sala comercial", TipoAtivo.Imovel, MoedaPatrimonio.BRL,
                    1_200_000m, null, receitaMensal: 10_000m, despesaMensal: 2_000m),
            });

        var result = await CreateHandler().Handle(new GetResumoPatrimonialQuery(), CancellationToken.None);

        result.ReceitaMensalBRL.Should().Be(10_000m);
        result.DespesaMensalBRL.Should().Be(2_000m);
        result.SaldoLiquidoMensalBRL.Should().Be(8_000m);
        result.RoiAnualPct.Should().Be(8m);
        result.Composicao.Should().ContainSingle(c => c.Categoria == "Imóveis");
    }

    [Fact]
    public async Task Handle_SemAtivos_ShouldReturnZero()
    {
        _ativoRepoMock.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<AtivoPatrimonial>());

        var result = await CreateHandler().Handle(new GetResumoPatrimonialQuery(), CancellationToken.None);

        result.QtdAtivos.Should().Be(0);
        result.TotalBensBRL.Should().Be(0m);
        result.PatrimonioLiquidoBRL.Should().Be(0m);
        result.TotaisPorMoeda.Should().BeEmpty();
        result.Composicao.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldUseEffectiveUserId()
    {
        // Sob view-as, ICurrentUser.UserId é o do cliente — o handler deve consultar por ele
        _ativoRepoMock.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<AtivoPatrimonial>());

        await CreateHandler().Handle(new GetResumoPatrimonialQuery(), CancellationToken.None);

        _ativoRepoMock.Verify(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>()), Times.Once);
        _passivoRepoMock.Verify(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
