using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Produtos.Commands.CreateProduto;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Produtos;

public class CreateProdutoCommandHandlerTests
{
    private readonly Mock<IProdutoRepository> _repoMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly CreateProdutoCommandHandler _handler;

    private static readonly Guid _userId = Guid.NewGuid();

    public CreateProdutoCommandHandlerTests()
    {
        _currentUserMock.Setup(u => u.UserId).Returns(_userId);
        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<Produto>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new CreateProdutoCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateProdutoAndReturnId()
    {
        // Arrange
        var command = new CreateProdutoCommand("Produto Teste", 99.90m);

        // Act
        var id = await _handler.Handle(command, CancellationToken.None);

        // Assert
        id.Should().NotBeEmpty();
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Produto>(), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNullPreco_ShouldCreateProdutoWithNullPreco()
    {
        // Arrange
        Produto? captured = null;
        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<Produto>(), It.IsAny<CancellationToken>()))
            .Callback<Produto, CancellationToken>((p, _) => captured = p)
            .Returns(Task.CompletedTask);

        var command = new CreateProdutoCommand("Serviço", null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        captured.Should().NotBeNull();
        captured!.PrecoDefault.Should().BeNull();
        captured.Ativo.Should().BeTrue();
        captured.UsuarioId.Should().Be(_userId);
    }
}
