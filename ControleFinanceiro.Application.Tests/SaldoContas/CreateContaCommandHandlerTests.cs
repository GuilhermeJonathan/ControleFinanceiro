using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.SaldoContas.Commands.CreateConta;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.SaldoContas;

public class CreateContaCommandHandlerTests
{
    private readonly Mock<ISaldoContaRepository> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly CreateContaCommandHandler _handler;

    private static readonly Guid _userId = Guid.NewGuid();

    public CreateContaCommandHandlerTests()
    {
        _currentUserMock.Setup(u => u.UserId).Returns(_userId);
        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<SaldoConta>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new CreateContaCommandHandler(
            _repoMock.Object,
            _uowMock.Object,
            _currentUserMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateAndReturnId()
    {
        // Arrange
        var command = new CreateContaCommand("Nubank", 1500.00m, TipoConta.ContaCorrente);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _repoMock.Verify(r => r.AddAsync(
            It.Is<SaldoConta>(c => c.Banco == "Nubank" && c.Saldo == 1500.00m && c.Tipo == TipoConta.ContaCorrente && c.UsuarioId == _userId),
            It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ZeroSaldo_ShouldCreateContaSuccessfully()
    {
        // Arrange
        var command = new CreateContaCommand("Caixa", 0m, TipoConta.Carteira);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _repoMock.Verify(r => r.AddAsync(
            It.Is<SaldoConta>(c => c.Saldo == 0m),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
