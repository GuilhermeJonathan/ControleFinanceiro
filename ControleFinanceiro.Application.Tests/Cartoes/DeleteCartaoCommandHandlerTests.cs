using ControleFinanceiro.Application.Cartoes.Commands.DeleteCartao;
using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Cartoes;

public class DeleteCartaoCommandHandlerTests
{
    private readonly Mock<ICartaoCreditoRepository> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly DeleteCartaoCommandHandler _handler;

    private static readonly Guid _userId = Guid.NewGuid();

    public DeleteCartaoCommandHandlerTests()
    {
        _currentUserMock.Setup(u => u.UserId).Returns(_userId);
        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new DeleteCartaoCommandHandler(
            _repoMock.Object,
            _uowMock.Object,
            _currentUserMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingCartao_ShouldDeleteAndSave()
    {
        // Arrange
        var cartaoId = Guid.NewGuid();
        var cartao = new CartaoCredito("Nubank", 10, _userId);
        _repoMock
            .Setup(r => r.GetByIdAsync(cartaoId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cartao);

        var command = new DeleteCartaoCommand(cartaoId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repoMock.Verify(r => r.Delete(cartao), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistingCartao_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var cartaoId = Guid.NewGuid();
        _repoMock
            .Setup(r => r.GetByIdAsync(cartaoId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CartaoCredito?)null);

        var command = new DeleteCartaoCommand(cartaoId);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }
}
