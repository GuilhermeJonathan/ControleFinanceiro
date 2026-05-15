using ControleFinanceiro.Application.Vendas.Commands.AtualizarStatusVenda;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Vendas;

public class AtualizarStatusVendaCommandHandlerTests
{
    private readonly Mock<IVendaRepository> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly AtualizarStatusVendaCommandHandler _handler;

    private static readonly Guid _userId = Guid.NewGuid();

    public AtualizarStatusVendaCommandHandlerTests()
    {
        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new AtualizarStatusVendaCommandHandler(_repoMock.Object, _uowMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingVenda_ShouldUpdateStatusAndSave()
    {
        // Arrange
        var vendaId = Guid.NewGuid();
        var venda = new Venda(_userId, null, "Venda", 100m, DateTime.UtcNow, OrigemVenda.Manual, "Teste");
        venda.Status.Should().Be(StatusVenda.Pendente);

        _repoMock
            .Setup(r => r.GetByIdAsync(vendaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(venda);

        var command = new AtualizarStatusVendaCommand(vendaId, StatusVenda.Recebido);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        venda.Status.Should().Be(StatusVenda.Recebido);
        _repoMock.Verify(r => r.Update(venda), Times.Once);
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
            _handler.Handle(new AtualizarStatusVendaCommand(vendaId, StatusVenda.Recebido), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NonExistingVenda_ShouldNotCallUpdateOrSave()
    {
        // Arrange
        var vendaId = Guid.NewGuid();
        _repoMock
            .Setup(r => r.GetByIdAsync(vendaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Venda?)null);

        // Act
        try { await _handler.Handle(new AtualizarStatusVendaCommand(vendaId, StatusVenda.Recebido), CancellationToken.None); } catch { }

        // Assert
        _repoMock.Verify(r => r.Update(It.IsAny<Venda>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
