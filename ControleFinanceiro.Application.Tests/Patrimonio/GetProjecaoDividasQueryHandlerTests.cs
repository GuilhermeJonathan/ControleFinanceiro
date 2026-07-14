using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Patrimonio.Queries.GetProjecaoDividas;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Patrimonio;

public class GetProjecaoDividasQueryHandlerTests
{
    private readonly Mock<IPassivoPatrimonialRepository> _repoMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private static readonly Guid UserId = Guid.NewGuid();

    public GetProjecaoDividasQueryHandlerTests()
    {
        _currentUserMock.Setup(c => c.UserId).Returns(UserId);
    }

    private GetProjecaoDividasQueryHandler CreateHandler() => new(_repoMock.Object, _currentUserMock.Object);

    [Fact]
    public async Task Handle_SemDividas_ShouldReturnEmpty()
    {
        _repoMock.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<PassivoPatrimonial>());

        var result = await CreateHandler().Handle(new GetProjecaoDividasQuery(), CancellationToken.None);

        result.Pontos.Should().BeEmpty();
        result.HorizonteMeses.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ComCronograma_ShouldAmortizeToZero()
    {
        // Sem juros, 12 meses → quita linearmente até 0 no mês 12.
        _repoMock.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new PassivoPatrimonial(UserId, "Empréstimo", MoedaPatrimonio.BRL, 12_000m, PrazoDivida.Curto, 0m, 12),
            });

        var result = await CreateHandler().Handle(new GetProjecaoDividasQuery(), CancellationToken.None);

        result.HorizonteMeses.Should().Be(12);
        result.SaldoInicialBRL.Should().Be(12_000m);
        result.Pontos.First().SaldoBRL.Should().Be(12_000m);
        result.Pontos.Last().SaldoBRL.Should().Be(0m);
    }

    [Fact]
    public async Task Handle_SemCronograma_ShouldStayFlat()
    {
        // Dívida bullet (sem PrazoMeses) → saldo constante ao longo do horizonte padrão.
        _repoMock.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new PassivoPatrimonial(UserId, "Bullet", MoedaPatrimonio.BRL, 100_000m, PrazoDivida.Longo),
            });

        var result = await CreateHandler().Handle(new GetProjecaoDividasQuery(), CancellationToken.None);

        result.Pontos.First().SaldoBRL.Should().Be(100_000m);
        result.Pontos.Last().SaldoBRL.Should().Be(100_000m);
    }

    [Fact]
    public async Task Handle_MoedaEstrangeira_ShouldConvertToBRL()
    {
        _repoMock.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new PassivoPatrimonial(UserId, "Lombard EUR", MoedaPatrimonio.EUR, 100_000m, PrazoDivida.Longo),
            });

        var result = await CreateHandler().Handle(new GetProjecaoDividasQuery(6), CancellationToken.None);

        // EUR 100k * 5,90 = 590.000
        result.SaldoInicialBRL.Should().Be(590_000m);
        result.HorizonteMeses.Should().Be(6);
    }
}
