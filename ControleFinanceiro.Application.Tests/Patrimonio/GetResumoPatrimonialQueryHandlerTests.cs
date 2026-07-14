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
    private readonly Mock<IAtivoPatrimonialRepository> _repoMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private static readonly Guid UserId = Guid.NewGuid();

    public GetResumoPatrimonialQueryHandlerTests()
    {
        _currentUserMock.Setup(c => c.UserId).Returns(UserId);
    }

    [Fact]
    public async Task Handle_MultiMoeda_ShouldGroupAndConsolidate()
    {
        var ativos = new[]
        {
            new AtivoPatrimonial(UserId, "Apartamento SP", TipoAtivo.Imovel,     MoedaPatrimonio.BRL, 2_000_000m, 8m),
            new AtivoPatrimonial(UserId, "Conta Suíça",    TipoAtivo.Investimento, MoedaPatrimonio.CHF, 100_000m, null),
            new AtivoPatrimonial(UserId, "ETF EUA",        TipoAtivo.Investimento, MoedaPatrimonio.USD, 50_000m, 12m),
        };
        _repoMock.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync(ativos);

        var handler = new GetResumoPatrimonialQueryHandler(_repoMock.Object, _currentUserMock.Object);
        var result = await handler.Handle(new GetResumoPatrimonialQuery(), CancellationToken.None);

        result.QtdAtivos.Should().Be(3);
        result.TotaisPorMoeda.Should().HaveCount(3);
        result.CambioEstimado.Should().BeTrue();
        // BRL 2.000.000 + CHF 100k*6,10 + USD 50k*5,40 = 2.000.000 + 610.000 + 270.000 = 2.880.000
        result.TotalConsolidadoBRL.Should().Be(2_880_000m);
    }

    [Fact]
    public async Task Handle_SemAtivos_ShouldReturnZero()
    {
        _repoMock.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<AtivoPatrimonial>());

        var handler = new GetResumoPatrimonialQueryHandler(_repoMock.Object, _currentUserMock.Object);
        var result = await handler.Handle(new GetResumoPatrimonialQuery(), CancellationToken.None);

        result.QtdAtivos.Should().Be(0);
        result.TotalConsolidadoBRL.Should().Be(0m);
        result.TotaisPorMoeda.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldUseEffectiveUserId()
    {
        // Sob view-as, ICurrentUser.UserId é o do cliente — o handler deve consultar por ele
        _repoMock.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<AtivoPatrimonial>());

        var handler = new GetResumoPatrimonialQueryHandler(_repoMock.Object, _currentUserMock.Object);
        await handler.Handle(new GetResumoPatrimonialQuery(), CancellationToken.None);

        _repoMock.Verify(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
