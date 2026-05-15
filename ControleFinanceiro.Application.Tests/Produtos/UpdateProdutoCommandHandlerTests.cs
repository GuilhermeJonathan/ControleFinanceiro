using ControleFinanceiro.Application.Produtos.Commands.UpdateProduto;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Produtos;

public class UpdateProdutoCommandHandlerTests
{
    private readonly Mock<IProdutoRepository> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly UpdateProdutoCommandHandler _handler;

    private static readonly Guid _userId = Guid.NewGuid();

    public UpdateProdutoCommandHandlerTests()
    {
        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new UpdateProdutoCommandHandler(_repoMock.Object, _uowMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingProduto_ShouldUpdateAndSave()
    {
        // Arrange
        var produtoId = Guid.NewGuid();
        var produto = new Produto(_userId, "Nome Antigo", 50m);
        _repoMock
            .Setup(r => r.GetByIdAsync(produtoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(produto);

        var command = new UpdateProdutoCommand(produtoId, "Nome Novo", 75m);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        produto.Nome.Should().Be("Nome Novo");
        produto.PrecoDefault.Should().Be(75m);
        _repoMock.Verify(r => r.Update(produto), Times.Once);
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

        var command = new UpdateProdutoCommand(produtoId, "Nome", null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NonExistingProduto_ShouldNotCallUpdateOrSave()
    {
        // Arrange
        var produtoId = Guid.NewGuid();
        _repoMock
            .Setup(r => r.GetByIdAsync(produtoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Produto?)null);

        var command = new UpdateProdutoCommand(produtoId, "Nome", null);

        // Act
        try { await _handler.Handle(command, CancellationToken.None); } catch { }

        // Assert
        _repoMock.Verify(r => r.Update(It.IsAny<Produto>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
