using ControleFinanceiro.Application.Categorias.Commands.CreateCategoria;
using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Categorias;

public class CreateCategoriaCommandHandlerTests
{
    private readonly Mock<ICategoriaRepository> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly CreateCategoriaCommandHandler _handler;

    private static readonly Guid _userId = Guid.NewGuid();

    public CreateCategoriaCommandHandlerTests()
    {
        _currentUserMock.Setup(u => u.UserId).Returns(_userId);
        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<Categoria>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new CreateCategoriaCommandHandler(
            _repoMock.Object,
            _uowMock.Object,
            _currentUserMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateAndReturnId()
    {
        // Arrange
        var command = new CreateCategoriaCommand("Alimentação", TipoLancamento.Debito);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _repoMock.Verify(r => r.AddAsync(
            It.Is<Categoria>(c => c.Nome == "Alimentação" && c.Tipo == TipoLancamento.Debito && c.UsuarioId == _userId),
            It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldAssignCurrentUserToCategoria()
    {
        // Arrange
        var command = new CreateCategoriaCommand("Salário", TipoLancamento.Credito);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repoMock.Verify(r => r.AddAsync(
            It.Is<Categoria>(c => c.UsuarioId == _userId),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
