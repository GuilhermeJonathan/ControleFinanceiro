using ControleFinanceiro.Application.Vendas.Commands.UpdateVenda;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Vendas;

public class UpdateVendaCommandHandlerTests
{
    private readonly Mock<IVendaRepository> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly UpdateVendaCommandHandler _handler;

    private static readonly Guid _userId = Guid.NewGuid();

    public UpdateVendaCommandHandlerTests()
    {
        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new UpdateVendaCommandHandler(_repoMock.Object, _uowMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingVenda_ShouldUpdateAndSave()
    {
        // Arrange
        var vendaId = Guid.NewGuid();
        var venda = new Venda(_userId, null, "Desc Antiga", 100m, DateTime.UtcNow, OrigemVenda.Manual, "Teste");
        _repoMock
            .Setup(r => r.GetByIdAsync(vendaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(venda);

        var novaData = DateTime.UtcNow.AddDays(1);
        var command = new UpdateVendaCommand(vendaId, null, "Desc Nova", 200m, novaData);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        venda.Descricao.Should().Be("Desc Nova");
        venda.Valor.Should().Be(200m);
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

        var command = new UpdateVendaCommand(vendaId, null, "Desc", 100m, DateTime.UtcNow);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NonExistingVenda_ShouldNotCallUpdateOrSave()
    {
        // Arrange
        var vendaId = Guid.NewGuid();
        _repoMock
            .Setup(r => r.GetByIdAsync(vendaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Venda?)null);

        var command = new UpdateVendaCommand(vendaId, null, "Desc", 100m, DateTime.UtcNow);

        // Act
        try { await _handler.Handle(command, CancellationToken.None); } catch { }

        // Assert
        _repoMock.Verify(r => r.Update(It.IsAny<Venda>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
