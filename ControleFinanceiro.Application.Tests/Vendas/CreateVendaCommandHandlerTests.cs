using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Vendas.Commands.CreateVenda;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Vendas;

public class CreateVendaCommandHandlerTests
{
    private readonly Mock<IVendaRepository> _repoMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly CreateVendaCommandHandler _handler;

    private static readonly Guid _userId = Guid.NewGuid();

    public CreateVendaCommandHandlerTests()
    {
        _currentUserMock.Setup(u => u.UserId).Returns(_userId);
        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<Venda>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new CreateVendaCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateVendaAndReturnId()
    {
        // Arrange
        var command = new CreateVendaCommand(null, "Venda Teste", 150m, DateTime.UtcNow);

        // Act
        var id = await _handler.Handle(command, CancellationToken.None);

        // Assert
        id.Should().NotBeEmpty();
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Venda>(), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithProduto_ShouldSetProdutoIdAndUsuarioId()
    {
        // Arrange
        Venda? captured = null;
        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<Venda>(), It.IsAny<CancellationToken>()))
            .Callback<Venda, CancellationToken>((v, _) => captured = v)
            .Returns(Task.CompletedTask);

        var produtoId = Guid.NewGuid();
        var command = new CreateVendaCommand(produtoId, "Com Produto", 200m, DateTime.UtcNow, OrigemVenda.WhatsApp);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        captured.Should().NotBeNull();
        captured!.ProdutoId.Should().Be(produtoId);
        captured.UsuarioId.Should().Be(_userId);
        captured.Status.Should().Be(StatusVenda.Pendente);
        captured.Origem.Should().Be(OrigemVenda.WhatsApp);
    }
}
