using ControleFinanceiro.Application.Cartoes.Commands.CreateCartao;
using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Cartoes;

public class CreateCartaoCommandHandlerTests
{
    private readonly Mock<ICartaoCreditoRepository> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly CreateCartaoCommandHandler _handler;

    private static readonly Guid _userId = Guid.NewGuid();

    public CreateCartaoCommandHandlerTests()
    {
        _currentUserMock.Setup(u => u.UserId).Returns(_userId);
        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<CartaoCredito>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new CreateCartaoCommandHandler(
            _repoMock.Object,
            _uowMock.Object,
            _currentUserMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateAndReturnId()
    {
        // Arrange
        var command = new CreateCartaoCommand("Nubank", 10);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _repoMock.Verify(r => r.AddAsync(
            It.Is<CartaoCredito>(c => c.Nome == "Nubank" && c.UsuarioId == _userId),
            It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CommandWithNullDiaVencimento_ShouldCreateCartaoWithNullDia()
    {
        // Arrange
        var command = new CreateCartaoCommand("Itaú");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _repoMock.Verify(r => r.AddAsync(
            It.Is<CartaoCredito>(c => c.DiaVencimento == null),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
