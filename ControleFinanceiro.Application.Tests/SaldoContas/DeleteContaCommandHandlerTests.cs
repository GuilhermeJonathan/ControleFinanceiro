using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.SaldoContas.Commands.DeleteConta;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.SaldoContas;

public class DeleteContaCommandHandlerTests
{
    private readonly Mock<ISaldoContaRepository> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly DeleteContaCommandHandler _handler;

    private static readonly Guid _userId = Guid.NewGuid();

    public DeleteContaCommandHandlerTests()
    {
        _currentUserMock.Setup(u => u.UserId).Returns(_userId);
        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new DeleteContaCommandHandler(
            _repoMock.Object,
            _uowMock.Object,
            _currentUserMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingConta_ShouldDeleteAndSave()
    {
        // Arrange
        var contaId = Guid.NewGuid();
        var conta = new SaldoConta("Nubank", 500m, TipoConta.ContaCorrente, _userId);
        _repoMock
            .Setup(r => r.GetByIdAsync(contaId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conta);

        var command = new DeleteContaCommand(contaId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repoMock.Verify(r => r.Delete(conta), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistingConta_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var contaId = Guid.NewGuid();
        _repoMock
            .Setup(r => r.GetByIdAsync(contaId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SaldoConta?)null);

        var command = new DeleteContaCommand(contaId);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }
}
