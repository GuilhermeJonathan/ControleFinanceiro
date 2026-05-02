using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Lancamentos.Commands.AtualizarSituacao;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Lancamentos;

public class AtualizarSituacaoCommandHandlerTests
{
    private readonly Mock<ILancamentoRepository> _lancamentoRepoMock = new();
    private readonly Mock<ISaldoContaRepository> _contaRepoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly AtualizarSituacaoCommandHandler _handler;

    private static readonly Guid _userId = Guid.NewGuid();

    public AtualizarSituacaoCommandHandlerTests()
    {
        _currentUserMock.Setup(u => u.UserId).Returns(_userId);
        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new AtualizarSituacaoCommandHandler(
            _lancamentoRepoMock.Object,
            _contaRepoMock.Object,
            _uowMock.Object,
            _currentUserMock.Object);
    }

    [Fact]
    public async Task Handle_MarkAsPago_ShouldUpdateSituacaoAndSave()
    {
        // Arrange
        var lancamentoId = Guid.NewGuid();
        var lancamento = new Lancamento(
            "Mercado", new DateTime(2025, 5, 1), 200m,
            TipoLancamento.Debito, SituacaoLancamento.AVencer, 5, 2025,
            usuarioId: _userId);
        _lancamentoRepoMock
            .Setup(r => r.GetByIdAsync(lancamentoId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lancamento);

        var command = new AtualizarSituacaoCommand(lancamentoId, SituacaoLancamento.Pago);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        lancamento.Situacao.Should().Be(SituacaoLancamento.Pago);
        _lancamentoRepoMock.Verify(r => r.Update(lancamento), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConfirmWithContaBancaria_ShouldMovimentarConta()
    {
        // Arrange
        var lancamentoId = Guid.NewGuid();
        var contaId = Guid.NewGuid();
        var lancamento = new Lancamento(
            "Conta de Luz", new DateTime(2025, 5, 1), 300m,
            TipoLancamento.Debito, SituacaoLancamento.AVencer, 5, 2025,
            usuarioId: _userId);
        var conta = new SaldoConta("Nubank", 1000m, TipoConta.ContaCorrente, _userId);

        _lancamentoRepoMock
            .Setup(r => r.GetByIdAsync(lancamentoId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lancamento);
        _contaRepoMock
            .Setup(r => r.GetByIdAsync(contaId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conta);

        var command = new AtualizarSituacaoCommand(lancamentoId, SituacaoLancamento.Pago, contaId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        // Débito: 1000 - 300 = 700
        conta.Saldo.Should().Be(700m);
        _contaRepoMock.Verify(r => r.Update(conta), Times.Once);
    }

    [Fact]
    public async Task Handle_RollbackFromPagoToAVencer_ShouldRevertContaSaldo()
    {
        // Arrange
        var lancamentoId = Guid.NewGuid();
        var contaId = Guid.NewGuid();

        // Lancamento already confirmed (Pago) and linked to conta
        var lancamento = new Lancamento(
            "Mercado", new DateTime(2025, 5, 1), 200m,
            TipoLancamento.Debito, SituacaoLancamento.AVencer, 5, 2025,
            usuarioId: _userId);

        // Simulate it being in Pago state with conta linked
        lancamento.AtualizarSituacao(SituacaoLancamento.Pago);
        lancamento.SetContaBancaria(contaId);

        var conta = new SaldoConta("Nubank", 800m, TipoConta.ContaCorrente, _userId);

        _lancamentoRepoMock
            .Setup(r => r.GetByIdAsync(lancamentoId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lancamento);
        _contaRepoMock
            .Setup(r => r.GetByIdAsync(contaId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conta);

        var command = new AtualizarSituacaoCommand(lancamentoId, SituacaoLancamento.AVencer);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        // Débito rollback: 800 + 200 = 1000
        conta.Saldo.Should().Be(1000m);
        lancamento.Situacao.Should().Be(SituacaoLancamento.AVencer);
    }

    [Fact]
    public async Task Handle_NonExistingLancamento_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var lancamentoId = Guid.NewGuid();
        _lancamentoRepoMock
            .Setup(r => r.GetByIdAsync(lancamentoId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Lancamento?)null);

        var command = new AtualizarSituacaoCommand(lancamentoId, SituacaoLancamento.Pago);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }
}
