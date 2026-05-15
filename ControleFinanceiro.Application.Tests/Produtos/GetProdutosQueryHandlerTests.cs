using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Produtos.Queries.GetProdutos;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Produtos;

public class GetProdutosQueryHandlerTests
{
    private readonly Mock<IProdutoRepository> _repoMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly GetProdutosQueryHandler _handler;

    private static readonly Guid _userId = Guid.NewGuid();

    public GetProdutosQueryHandlerTests()
    {
        _currentUserMock.Setup(u => u.UserId).Returns(_userId);
        _handler = new GetProdutosQueryHandler(_repoMock.Object, _currentUserMock.Object);
    }

    [Fact]
    public async Task Handle_WithProdutos_ShouldReturnMappedDtos()
    {
        // Arrange
        var produto1 = new Produto(_userId, "Produto A", 100m);
        var produto2 = new Produto(_userId, "Produto B", null);
        _repoMock
            .Setup(r => r.GetAllAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Produto> { produto1, produto2 });

        // Act
        var result = await _handler.Handle(new GetProdutosQuery(), CancellationToken.None);

        // Assert
        var list = result.ToList();
        list.Should().HaveCount(2);
        list[0].Nome.Should().Be("Produto A");
        list[0].PrecoDefault.Should().Be(100m);
        list[1].Nome.Should().Be("Produto B");
        list[1].PrecoDefault.Should().BeNull();
    }

    [Fact]
    public async Task Handle_NoProdutos_ShouldReturnEmptyList()
    {
        // Arrange
        _repoMock
            .Setup(r => r.GetAllAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Produto>());

        // Act
        var result = await _handler.Handle(new GetProdutosQuery(), CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}
