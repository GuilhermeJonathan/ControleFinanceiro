using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Lancamentos.Queries.GetAssinaturas;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Lancamentos;

public class GetAssinaturasQueryHandlerTests
{
    private readonly Mock<ILancamentoRepository> _repoMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly GetAssinaturasQueryHandler _handler;

    private static readonly Guid _userId = Guid.NewGuid();
    private static readonly Guid _receitaId = Guid.NewGuid();

    public GetAssinaturasQueryHandlerTests()
    {
        _currentUserMock.Setup(u => u.UserId).Returns(_userId);
        _handler = new GetAssinaturasQueryHandler(_repoMock.Object, _currentUserMock.Object);
    }

    [Fact]
    public async Task Handle_WithRecurrenteLancamentos_ShouldGroupAndReturnAssinaturas()
    {
        // Arrange
        var lancamentos = new List<Lancamento>
        {
            new("Netflix", new DateTime(2025, 1, 10), 49.90m,
                TipoLancamento.Debito, SituacaoLancamento.Pago, 1, 2025,
                receitaRecorrenteId: _receitaId, isRecorrente: true, usuarioId: _userId),
            new("Netflix", new DateTime(2025, 2, 10), 49.90m,
                TipoLancamento.Debito, SituacaoLancamento.AVencer, 2, 2025,
                receitaRecorrenteId: _receitaId, isRecorrente: true, usuarioId: _userId),
            new("Netflix", new DateTime(2025, 3, 10), 49.90m,
                TipoLancamento.Debito, SituacaoLancamento.AVencer, 3, 2025,
                receitaRecorrenteId: _receitaId, isRecorrente: true, usuarioId: _userId),
        };

        _repoMock
            .Setup(r => r.GetRecorrentesAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lancamentos);

        // Act
        var result = await _handler.Handle(new GetAssinaturasQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);

        var assinatura = result[0];
        assinatura.Descricao.Should().Be("Netflix");
        assinatura.ValorMensal.Should().Be(49.90m);
        assinatura.TotalLancamentos.Should().Be(3);
        assinatura.LancamentosPagos.Should().Be(1);
        assinatura.ProximoVencimento.Should().Be(new DateTime(2025, 2, 10));
    }

    [Fact]
    public async Task Handle_WithNoRecurrenteLancamentos_ShouldReturnEmptyList()
    {
        // Arrange
        _repoMock
            .Setup(r => r.GetRecorrentesAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Lancamento>());

        // Act
        var result = await _handler.Handle(new GetAssinaturasQuery(), CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_MultipleGroups_ShouldOrderByProximoVencimento()
    {
        // Arrange
        var receitaIdA = Guid.NewGuid();
        var receitaIdB = Guid.NewGuid();

        var lancamentos = new List<Lancamento>
        {
            // Grupo A — vence em março (mais longe)
            new("Spotify", new DateTime(2025, 3, 15), 19.90m,
                TipoLancamento.Debito, SituacaoLancamento.AVencer, 3, 2025,
                receitaRecorrenteId: receitaIdA, isRecorrente: true, usuarioId: _userId),
            // Grupo B — vence em fevereiro (mais próximo)
            new("Amazon Prime", new DateTime(2025, 2, 5), 14.90m,
                TipoLancamento.Debito, SituacaoLancamento.AVencer, 2, 2025,
                receitaRecorrenteId: receitaIdB, isRecorrente: true, usuarioId: _userId),
        };

        _repoMock
            .Setup(r => r.GetRecorrentesAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lancamentos);

        // Act
        var result = await _handler.Handle(new GetAssinaturasQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        // Amazon Prime (fevereiro) deve vir antes de Spotify (março)
        result[0].Descricao.Should().Be("Amazon Prime");
        result[1].Descricao.Should().Be("Spotify");
    }

    [Fact]
    public async Task Handle_AllPaid_ShouldNotCallRepositoryMutations()
    {
        // Arrange — verify the handler is truly read-only (no Update/Delete calls)
        var lancamentos = new List<Lancamento>
        {
            new("Netflix", new DateTime(2025, 1, 10), 49.90m,
                TipoLancamento.Debito, SituacaoLancamento.Pago, 1, 2025,
                receitaRecorrenteId: _receitaId, isRecorrente: true, usuarioId: _userId),
        };

        _repoMock
            .Setup(r => r.GetRecorrentesAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lancamentos);

        // Act
        var result = await _handler.Handle(new GetAssinaturasQuery(), CancellationToken.None);

        // Assert — mutations should never be called on a query handler
        _repoMock.Verify(r => r.Update(It.IsAny<Lancamento>()), Times.Never);
        _repoMock.Verify(r => r.Delete(It.IsAny<Lancamento>()), Times.Never);

        result[0].LancamentosPagos.Should().Be(1);
        result[0].ProximoVencimento.Should().BeNull();
    }
}
