using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Lancamentos.Commands.CreateLancamento;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Lancamentos;

public class CreateLancamentoCommandHandlerTests
{
    private readonly Mock<ILancamentoRepository> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly CreateLancamentoCommandHandler _handler;

    private static readonly Guid _userId = Guid.NewGuid();
    private static readonly Guid _realUserId = Guid.NewGuid();

    public CreateLancamentoCommandHandlerTests()
    {
        _currentUserMock.Setup(u => u.UserId).Returns(_userId);
        _currentUserMock.Setup(u => u.RealUserId).Returns(_realUserId);
        _currentUserMock.Setup(u => u.RealUserName).Returns("Test User");
        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<Lancamento>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _repoMock
            .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<Lancamento>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new CreateLancamentoCommandHandler(
            _repoMock.Object,
            _uowMock.Object,
            _currentUserMock.Object);
    }

    [Fact]
    public async Task Handle_SingleLancamento_ShouldCreateAndReturnId()
    {
        // Arrange
        var command = new CreateLancamentoCommand(
            Descricao: "Mercado",
            Data: new DateTime(2025, 5, 1),
            Valor: 200m,
            Tipo: TipoLancamento.Debito,
            Situacao: SituacaoLancamento.Pago,
            Mes: 5,
            Ano: 2025,
            CategoriaId: Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _repoMock.Verify(r => r.AddAsync(
            It.Is<Lancamento>(l => l.Descricao == "Mercado" && l.Valor == 200m && l.UsuarioId == _userId),
            It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ParceledLancamento_ShouldCreateMultipleAndReturnFirstId()
    {
        // Arrange
        var command = new CreateLancamentoCommand(
            Descricao: "Notebook",
            Data: new DateTime(2025, 5, 1),
            Valor: 3000m,
            Tipo: TipoLancamento.Debito,
            Situacao: SituacaoLancamento.Pago,
            Mes: 5,
            Ano: 2025,
            CategoriaId: Guid.NewGuid(),
            TotalParcelas: 3);

        List<Lancamento>? capturedLancamentos = null;
        _repoMock
            .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<Lancamento>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<Lancamento>, CancellationToken>((items, _) => capturedLancamentos = items.ToList())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        capturedLancamentos.Should().HaveCount(3);
        capturedLancamentos![0].Descricao.Should().Contain("(1/3)");
        capturedLancamentos![1].Descricao.Should().Contain("(2/3)");
        capturedLancamentos![2].Descricao.Should().Contain("(3/3)");
        _repoMock.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<Lancamento>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_RecorrenteLancamento_ShouldCreateMultipleWithSameValueAndNoSuffix()
    {
        // Arrange
        var command = new CreateLancamentoCommand(
            Descricao: "Aluguel",
            Data: new DateTime(2025, 5, 1),
            Valor: 1500m,
            Tipo: TipoLancamento.Debito,
            Situacao: SituacaoLancamento.Pago,
            Mes: 5,
            Ano: 2025,
            CategoriaId: Guid.NewGuid(),
            TotalParcelas: 3,
            IsRecorrente: true);

        List<Lancamento>? capturedLancamentos = null;
        _repoMock
            .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<Lancamento>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<Lancamento>, CancellationToken>((items, _) => capturedLancamentos = items.ToList())
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedLancamentos.Should().HaveCount(3);
        capturedLancamentos!.Should().OnlyContain(l => l.Valor == 1500m);
        capturedLancamentos!.Should().OnlyContain(l => l.Descricao == "Aluguel");
    }

    [Fact]
    public async Task Handle_TotalParcelasZeroOrNegative_ShouldCreateSingleLancamento()
    {
        // Arrange
        var command = new CreateLancamentoCommand(
            Descricao: "Teste",
            Data: new DateTime(2025, 5, 1),
            Valor: 100m,
            Tipo: TipoLancamento.Debito,
            Situacao: SituacaoLancamento.AVencer,
            Mes: 5,
            Ano: 2025,
            CategoriaId: null,
            TotalParcelas: 0);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Lancamento>(), It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<Lancamento>>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
