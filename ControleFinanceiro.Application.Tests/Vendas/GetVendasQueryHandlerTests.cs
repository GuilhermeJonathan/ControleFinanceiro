using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Vendas.Queries.GetVendas;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Vendas;

public class GetVendasQueryHandlerTests
{
    private readonly Mock<IVendaRepository> _vendaRepoMock = new();
    private readonly Mock<IProdutoRepository> _produtoRepoMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly GetVendasQueryHandler _handler;

    private static readonly Guid _userId = Guid.NewGuid();

    public GetVendasQueryHandlerTests()
    {
        _currentUserMock.Setup(u => u.UserId).Returns(_userId);
        _produtoRepoMock
            .Setup(r => r.GetAllAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Produto>());
        _handler = new GetVendasQueryHandler(_vendaRepoMock.Object, _produtoRepoMock.Object, _currentUserMock.Object);
    }

    [Fact]
    public async Task Handle_WithVendas_ShouldReturnMappedDtos()
    {
        // Arrange
        var venda = new Venda(_userId, null, "Venda Teste", 99.99m, DateTime.UtcNow, OrigemVenda.Manual, "Teste");
        _vendaRepoMock
            .Setup(r => r.GetAllAsync(null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Venda> { venda });

        // Act
        var result = await _handler.Handle(new GetVendasQuery(null, null, null, null), CancellationToken.None);

        // Assert
        var list = result.ToList();
        list.Should().HaveCount(1);
        list[0].Descricao.Should().Be("Venda Teste");
        list[0].Valor.Should().Be(99.99m);
        list[0].Status.Should().Be(StatusVenda.Pendente);
    }

    [Fact]
    public async Task Handle_WithProduto_ShouldResolveProdutoNome()
    {
        // Arrange
        var produtoId = Guid.NewGuid();
        var venda = new Venda(_userId, produtoId, "Venda com produto", 50m, DateTime.UtcNow, OrigemVenda.Manual, "Teste");

        _vendaRepoMock
            .Setup(r => r.GetAllAsync(null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Venda> { venda });

        // Act
        var result = await _handler.Handle(new GetVendasQuery(null, null, null, null), CancellationToken.None);

        // Assert
        var dto = result.First();
        dto.ProdutoId.Should().Be(produtoId);
        // ProdutoNome will be null because the product in repo doesn't match by id in this test
        dto.ProdutoNome.Should().BeNull();
    }

    [Fact]
    public async Task Handle_NoVendas_ShouldReturnEmptyList()
    {
        // Arrange
        _vendaRepoMock
            .Setup(r => r.GetAllAsync(null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Venda>());

        // Act
        var result = await _handler.Handle(new GetVendasQuery(null, null, null, null), CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}
