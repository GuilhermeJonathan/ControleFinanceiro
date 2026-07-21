using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Patrimonio.Queries.GetProjecaoPatrimonio;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Patrimonio;

public class GetProjecaoPatrimonioQueryHandlerTests
{
    private readonly Mock<IAtivoPatrimonialRepository> _ativoRepoMock = new();
    private readonly Mock<IPassivoPatrimonialRepository> _passivoRepoMock = new();
    private readonly Mock<IFxRateResolver> _fxMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private static readonly Guid UserId = Guid.NewGuid();

    public GetProjecaoPatrimonioQueryHandlerTests()
    {
        _currentUserMock.Setup(c => c.UserId).Returns(UserId);
        _fxMock.Setup(r => r.GetRatesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
            {
                ["BRL"] = 1.00m,
                ["USD"] = 5.00m,
            });
        _ativoRepoMock.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<AtivoPatrimonial>());
        _passivoRepoMock.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<PassivoPatrimonial>());
    }

    private GetProjecaoPatrimonioQueryHandler CreateHandler() =>
        new(_ativoRepoMock.Object, _passivoRepoMock.Object, _fxMock.Object, _currentUserMock.Object);

    [Fact]
    public async Task Handle_SemAtivosNemDividas_ShouldReturnEmpty()
    {
        var result = await CreateHandler().Handle(new GetProjecaoPatrimonioQuery(), CancellationToken.None);

        result.Pontos.Should().BeEmpty();
        result.HorizonteMeses.Should().Be(0);
        result.MesesQuitacaoDividas.Should().BeNull();
    }

    [Fact]
    public async Task Handle_BemSemValorizacao_ShouldStayFlat()
    {
        _ativoRepoMock.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new AtivoPatrimonial(UserId, "Casa", TipoAtivo.Imovel, MoedaPatrimonio.BRL, 1_000_000m, null),
            });

        var result = await CreateHandler().Handle(new GetProjecaoPatrimonioQuery(12), CancellationToken.None);

        result.HorizonteMeses.Should().Be(12);
        result.Pontos.First().BensBRL.Should().Be(1_000_000m);
        result.Pontos.Last().BensBRL.Should().Be(1_000_000m);
        result.Pontos.Last().PatrimonioLiquidoBRL.Should().Be(1_000_000m);
    }

    [Fact]
    public async Task Handle_BemComValorizacao_ShouldGrow()
    {
        _ativoRepoMock.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new AtivoPatrimonial(UserId, "Fundo", TipoAtivo.Investimento, MoedaPatrimonio.BRL, 100_000m, 12m),
            });

        var result = await CreateHandler().Handle(new GetProjecaoPatrimonioQuery(12), CancellationToken.None);

        result.Pontos.Last().BensBRL.Should().BeGreaterThan(result.Pontos.First().BensBRL);
        result.PatrimonioFinalBRL.Should().BeGreaterThan(result.PatrimonioInicialBRL);
    }

    [Fact]
    public async Task Handle_BensEDividas_PatrimonioLiquidoDeveSerBensMenosDividas_EDividaQuitaNoPrazo()
    {
        _ativoRepoMock.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new AtivoPatrimonial(UserId, "Casa", TipoAtivo.Imovel, MoedaPatrimonio.BRL, 1_000_000m, null),
            });
        _passivoRepoMock.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                // Sem juros, 12 meses → quita no mês 12.
                new PassivoPatrimonial(UserId, "Financiamento", MoedaPatrimonio.BRL, 120_000m, PrazoDivida.Curto, 0m, 12),
            });

        var result = await CreateHandler().Handle(new GetProjecaoPatrimonioQuery(), CancellationToken.None);

        result.HorizonteMeses.Should().Be(12);
        var primeiro = result.Pontos.First();
        primeiro.PatrimonioLiquidoBRL.Should().Be(primeiro.BensBRL - primeiro.DividasBRL);
        primeiro.PatrimonioLiquidoBRL.Should().Be(880_000m); // 1.000.000 − 120.000
        result.Pontos.Last().DividasBRL.Should().Be(0m);
        result.Pontos.Last().PatrimonioLiquidoBRL.Should().Be(1_000_000m);
        result.MesesQuitacaoDividas.Should().Be(12);
    }
}
