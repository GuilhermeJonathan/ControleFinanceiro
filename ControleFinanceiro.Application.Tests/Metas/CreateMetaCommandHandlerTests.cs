using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Metas.Commands.CreateMeta;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Metas;

public class CreateMetaCommandHandlerTests
{
    private readonly Mock<IMetaRepository> _repoMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly CreateMetaCommandHandler _handler;

    private static readonly Guid _userId = Guid.NewGuid();

    public CreateMetaCommandHandlerTests()
    {
        _currentUserMock.Setup(u => u.UserId).Returns(_userId);
        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<Meta>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new CreateMetaCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateMetaAndReturnId()
    {
        // Arrange
        var command = new CreateMetaCommand(
            Titulo: "Viagem",
            Descricao: "Férias na Europa",
            ValorMeta: 10_000m,
            DataMeta: new DateTime(2027, 6, 1),
            Capa: "✈️",
            CorFundo: "#0d2137");

        // Act
        var id = await _handler.Handle(command, CancellationToken.None);

        // Assert
        id.Should().NotBeEmpty();
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Meta>(), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithContribuicao_ShouldSetContribuicaoFields()
    {
        // Arrange
        Meta? captured = null;
        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<Meta>(), It.IsAny<CancellationToken>()))
            .Callback<Meta, CancellationToken>((m, _) => captured = m)
            .Returns(Task.CompletedTask);

        var command = new CreateMetaCommand(
            Titulo: "Casa Própria",
            Descricao: null,
            ValorMeta: 50_000m,
            DataMeta: null,
            Capa: "🏠",
            CorFundo: "#1a3a2a",
            ContribuicaoMensalValor: 500m,
            ContribuicaoDia: 10);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        captured.Should().NotBeNull();
        captured!.ContribuicaoMensalValor.Should().Be(500m);
        captured.ContribuicaoDia.Should().Be(10);
    }

    [Fact]
    public async Task Handle_WithContribuicaoDiaOutOfRange_ShouldNullifyContribuicaoDia()
    {
        // Arrange
        Meta? captured = null;
        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<Meta>(), It.IsAny<CancellationToken>()))
            .Callback<Meta, CancellationToken>((m, _) => captured = m)
            .Returns(Task.CompletedTask);

        var command = new CreateMetaCommand(
            Titulo: "Reserva",
            Descricao: null,
            ValorMeta: 5_000m,
            DataMeta: null,
            Capa: null,
            CorFundo: "#1a1a2a",
            ContribuicaoMensalValor: 200m,
            ContribuicaoDia: 31); // inválido — deve ser ignorado

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        captured.Should().NotBeNull();
        captured!.ContribuicaoDia.Should().BeNull();
    }
}
