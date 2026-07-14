using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Patrimonio.Commands.DeleteInvestimento;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Patrimonio;

public class DeleteInvestimentoCommandHandlerTests
{
    private readonly Mock<IInvestimentoRepository> _repoMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private static readonly Guid UserId = Guid.NewGuid();

    public DeleteInvestimentoCommandHandlerTests()
    {
        _currentUserMock.Setup(c => c.UserId).Returns(UserId);
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Handle_Existing_ShouldRemoveAndSave()
    {
        var inv = new Investimento(UserId, "BTC", TipoInvestimento.Cripto,
            MoedaPatrimonio.USD, null, "BTC", 30_000m, 35_000m, null);
        _repoMock.Setup(r => r.GetByIdAsync(inv.Id, It.IsAny<CancellationToken>())).ReturnsAsync(inv);

        var handler = new DeleteInvestimentoCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);

        await handler.Handle(new DeleteInvestimentoCommand(inv.Id), CancellationToken.None);

        _repoMock.Verify(r => r.Remove(inv), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NotFound_ShouldThrowKeyNotFoundException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Investimento?)null);

        var handler = new DeleteInvestimentoCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);

        await handler.Invoking(h => h.Handle(new DeleteInvestimentoCommand(Guid.NewGuid()), CancellationToken.None))
            .Should().ThrowAsync<KeyNotFoundException>();

        _repoMock.Verify(r => r.Remove(It.IsAny<Investimento>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_OtherUser_ShouldThrowUnauthorizedAccessException()
    {
        var inv = new Investimento(Guid.NewGuid(), "Alheio", TipoInvestimento.FII,
            MoedaPatrimonio.BRL, null, null, 1m, 1m, null);
        _repoMock.Setup(r => r.GetByIdAsync(inv.Id, It.IsAny<CancellationToken>())).ReturnsAsync(inv);

        var handler = new DeleteInvestimentoCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);

        await handler.Invoking(h => h.Handle(new DeleteInvestimentoCommand(inv.Id), CancellationToken.None))
            .Should().ThrowAsync<UnauthorizedAccessException>();

        _repoMock.Verify(r => r.Remove(It.IsAny<Investimento>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
