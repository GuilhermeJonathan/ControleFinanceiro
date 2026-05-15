using ControleFinanceiro.Application.Produtos.Commands.DeleteProduto;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Produtos;

public class DeleteProdutoCommandHandlerTests
{
    private readonly Mock<IProdutoRepository> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly DeleteProdutoCommandHandler _handler;

    private static readonly Guid _userId = Guid.NewGuid();

    public DeleteProdutoCommandHandlerTests()
    {
        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new DeleteProdutoCommandHandler(_repoMock.Object, _uowMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingProduto_ShouldRemoveAndSave()
    {
        // Arrange
        var produtoId = Guid.NewGuid();
        var produto = new Produto(_userId, "Produto", 10m);
        _repoMock
            .Setup(r => r.GetByIdAsync(produtoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(produto);

        // Act
        await _handler.Handle(new DeleteProdutoCommand(produtoId), CancellationToken.None);

        // Assert
        _repoMock.Verify(r => r.Remove(produto), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistingProduto_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var produtoId = Guid.NewGuid();
        _repoMock
            .Setup(r => r.GetByIdAsync(produtoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Produto?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(new DeleteProdutoCommand(produtoId), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NonExistingProduto_ShouldNotCallRemoveOrSave()
    {
        // Arrange
        var produtoId = Guid.NewGuid();
        _repoMock
            .Setup(r => r.GetByIdAsync(produtoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Produto?)null);

        // Act
        try { await _handler.Handle(new DeleteProdutoCommand(produtoId), CancellationToken.None); } catch { }

        // Assert
        _repoMock.Verify(r => r.Remove(It.IsAny<Produto>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
