using ControleFinanceiro.Application.Vendas.Commands.DeleteVenda;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Vendas;

public class DeleteVendaCommandHandlerTests
{
    private readonly Mock<IVendaRepository> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly DeleteVendaCommandHandler _handler;

    private static readonly Guid _userId = Guid.NewGuid();

    public DeleteVendaCommandHandlerTests()
    {
        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new DeleteVendaCommandHandler(_repoMock.Object, _uowMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingVenda_ShouldRemoveAndSave()
    {
        // Arrange
        var vendaId = Guid.NewGuid();
        var venda = new Venda(_userId, null, "Venda", 50m, DateTime.UtcNow, OrigemVenda.Manual, "Teste");
        _repoMock
            .Setup(r => r.GetByIdAsync(vendaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(venda);

        // Act
        await _handler.Handle(new DeleteVendaCommand(vendaId), CancellationToken.None);

        // Assert
        _repoMock.Verify(r => r.Remove(venda), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistingVenda_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var vendaId = Guid.NewGuid();
        _repoMock
            .Setup(r => r.GetByIdAsync(vendaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Venda?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(new DeleteVendaCommand(vendaId), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NonExistingVenda_ShouldNotCallRemoveOrSave()
    {
        // Arrange
        var vendaId = Guid.NewGuid();
        _repoMock
            .Setup(r => r.GetByIdAsync(vendaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Venda?)null);

        // Act
        try { await _handler.Handle(new DeleteVendaCommand(vendaId), CancellationToken.None); } catch { }

        // Assert
        _repoMock.Verify(r => r.Remove(It.IsAny<Venda>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
