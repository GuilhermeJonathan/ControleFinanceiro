using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Vendas.Queries.GetResumoVendas;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Vendas;

public class GetResumoVendasQueryHandlerTests
{
    private readonly Mock<IVendaRepository> _repoMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly GetResumoVendasQueryHandler _handler;

    private static readonly Guid _userId = Guid.NewGuid();

    public GetResumoVendasQueryHandlerTests()
    {
        _currentUserMock.Setup(u => u.UserId).Returns(_userId);
        _handler = new GetResumoVendasQueryHandler(_repoMock.Object, _currentUserMock.Object);
    }

    [Fact]
    public async Task Handle_WithVendas_ShouldReturnResumo()
    {
        // Arrange
        var hoje = DateTime.UtcNow;
        var venda1 = new Venda(_userId, null, "Hoje", 100m, hoje, OrigemVenda.Manual, "Teste");
        var venda2 = new Venda(_userId, null, "Semana", 200m, hoje.AddDays(-2), OrigemVenda.Manual, "Teste");
        var venda3 = new Venda(_userId, null, "Mes", 300m, hoje.AddDays(-10), OrigemVenda.Manual, "Teste");

        _repoMock
            .Setup(r => r.GetAllAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Venda> { venda1, venda2, venda3 });

        // Act
        var result = await _handler.Handle(new GetResumoVendasQuery(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalMes.Should().Be(600m);
        result.QtdMes.Should().Be(3);
        result.TotalHoje.Should().Be(100m);
        result.QtdHoje.Should().Be(1);
    }

    [Fact]
    public async Task Handle_NoVendas_ShouldReturnZeros()
    {
        // Arrange
        _repoMock
            .Setup(r => r.GetAllAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Venda>());

        // Act
        var result = await _handler.Handle(new GetResumoVendasQuery(), CancellationToken.None);

        // Assert
        result.TotalHoje.Should().Be(0);
        result.TotalSemana.Should().Be(0);
        result.TotalMes.Should().Be(0);
        result.QtdHoje.Should().Be(0);
        result.QtdSemana.Should().Be(0);
        result.QtdMes.Should().Be(0);
    }
}
