using ControleFinanceiro.Application.Categorias.Commands.DeleteCategoria;
using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Categorias;

public class DeleteCategoriaCommandHandlerTests
{
    private readonly Mock<ICategoriaRepository> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly DeleteCategoriaCommandHandler _handler;

    private static readonly Guid _userId = Guid.NewGuid();

    public DeleteCategoriaCommandHandlerTests()
    {
        _currentUserMock.Setup(u => u.UserId).Returns(_userId);
        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new DeleteCategoriaCommandHandler(
            _repoMock.Object,
            _uowMock.Object,
            _currentUserMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingCategoria_ShouldDeleteAndSave()
    {
        // Arrange
        var categoriaId = Guid.NewGuid();
        var categoria = new Categoria("Alimentação", TipoLancamento.Debito, _userId);
        _repoMock
            .Setup(r => r.GetByIdAsync(categoriaId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categoria);

        var command = new DeleteCategoriaCommand(categoriaId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repoMock.Verify(r => r.Delete(categoria), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistingCategoria_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var categoriaId = Guid.NewGuid();
        _repoMock
            .Setup(r => r.GetByIdAsync(categoriaId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Categoria?)null);

        var command = new DeleteCategoriaCommand(categoriaId);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NonExistingCategoria_ShouldNotCallSaveChanges()
    {
        // Arrange
        var categoriaId = Guid.NewGuid();
        _repoMock
            .Setup(r => r.GetByIdAsync(categoriaId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Categoria?)null);

        var command = new DeleteCategoriaCommand(categoriaId);

        // Act
        try { await _handler.Handle(command, CancellationToken.None); } catch { }

        // Assert
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
