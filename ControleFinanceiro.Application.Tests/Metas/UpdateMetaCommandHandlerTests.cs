using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Metas.Commands.UpdateMeta;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Metas;

public class UpdateMetaCommandHandlerTests
{
    private readonly Mock<IMetaRepository> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly UpdateMetaCommandHandler _handler;

    private static readonly Guid _userId = Guid.NewGuid();

    public UpdateMetaCommandHandlerTests()
    {
        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new UpdateMetaCommandHandler(_repoMock.Object, _uowMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingMeta_ShouldUpdateAndSave()
    {
        // Arrange
        var metaId = Guid.NewGuid();
        var meta = new Meta(_userId, "Old Title", null, 1_000m, null, null, null);
        _repoMock
            .Setup(r => r.GetByIdAsync(metaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(meta);

        var command = new UpdateMetaCommand(
            Id: metaId,
            Titulo: "New Title",
            Descricao: "Updated",
            ValorMeta: 2_000m,
            DataMeta: null,
            Capa: "🏠",
            CorFundo: "#0d2137");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        meta.Titulo.Should().Be("New Title");
        meta.ValorMeta.Should().Be(2_000m);
        _repoMock.Verify(r => r.Update(meta), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ExistingMeta_ShouldUpdateContribuicaoFields()
    {
        // Arrange
        var metaId = Guid.NewGuid();
        var meta = new Meta(_userId, "Viagem", null, 8_000m, null, null, null);
        _repoMock
            .Setup(r => r.GetByIdAsync(metaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(meta);

        var command = new UpdateMetaCommand(
            Id: metaId,
            Titulo: "Viagem",
            Descricao: null,
            ValorMeta: 8_000m,
            DataMeta: null,
            Capa: "✈️",
            CorFundo: "#1a2a3a",
            ContribuicaoMensalValor: 400m,
            ContribuicaoDia: 5);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        meta.ContribuicaoMensalValor.Should().Be(400m);
        meta.ContribuicaoDia.Should().Be(5);
        _repoMock.Verify(r => r.Update(meta), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistingMeta_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var metaId = Guid.NewGuid();
        _repoMock
            .Setup(r => r.GetByIdAsync(metaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Meta?)null);

        var command = new UpdateMetaCommand(
            Id: metaId,
            Titulo: "Qualquer",
            Descricao: null,
            ValorMeta: 1_000m,
            DataMeta: null,
            Capa: null,
            CorFundo: null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NonExistingMeta_ShouldNotCallUpdateOrSave()
    {
        // Arrange
        var metaId = Guid.NewGuid();
        _repoMock
            .Setup(r => r.GetByIdAsync(metaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Meta?)null);

        var command = new UpdateMetaCommand(
            Id: metaId,
            Titulo: "Qualquer",
            Descricao: null,
            ValorMeta: 1_000m,
            DataMeta: null,
            Capa: null,
            CorFundo: null);

        // Act
        try { await _handler.Handle(command, CancellationToken.None); } catch { }

        // Assert
        _repoMock.Verify(r => r.Update(It.IsAny<Meta>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
