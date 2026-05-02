using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Lancamentos.Commands.DeleteLancamento;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Lancamentos;

public class DeleteLancamentoCommandHandlerTests
{
    private readonly Mock<ILancamentoRepository> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly DeleteLancamentoCommandHandler _handler;

    private static readonly Guid _userId = Guid.NewGuid();

    public DeleteLancamentoCommandHandlerTests()
    {
        _currentUserMock.Setup(u => u.UserId).Returns(_userId);
        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new DeleteLancamentoCommandHandler(
            _repoMock.Object,
            _uowMock.Object,
            _currentUserMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingLancamento_ShouldDeleteAndSave()
    {
        // Arrange
        var lancamentoId = Guid.NewGuid();
        var lancamento = new Lancamento(
            "Mercado", new DateTime(2025, 5, 1), 200m,
            TipoLancamento.Debito, SituacaoLancamento.AVencer, 5, 2025,
            usuarioId: _userId);
        _repoMock
            .Setup(r => r.GetByIdAsync(lancamentoId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lancamento);

        var command = new DeleteLancamentoCommand(lancamentoId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repoMock.Verify(r => r.Delete(lancamento), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistingLancamento_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var lancamentoId = Guid.NewGuid();
        _repoMock
            .Setup(r => r.GetByIdAsync(lancamentoId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Lancamento?)null);

        var command = new DeleteLancamentoCommand(lancamentoId);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }
}
