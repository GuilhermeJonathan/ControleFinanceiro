using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Patrimonio.Commands.UpdateInvestimento;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Patrimonio;

public class UpdateInvestimentoCommandHandlerTests
{
    private readonly Mock<IInvestimentoRepository> _repoMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private static readonly Guid UserId = Guid.NewGuid();

    public UpdateInvestimentoCommandHandlerTests()
    {
        _currentUserMock.Setup(c => c.UserId).Returns(UserId);
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Handle_Existing_ShouldUpdateAndSave()
    {
        var inv = new Investimento(UserId, "Fundo Antigo", TipoInvestimento.Multimercado,
            MoedaPatrimonio.BRL, null, null, 10_000m, 10_500m, null);
        _repoMock.Setup(r => r.GetByIdAsync(inv.Id, It.IsAny<CancellationToken>())).ReturnsAsync(inv);

        var handler = new UpdateInvestimentoCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);
        var command = new UpdateInvestimentoCommand(inv.Id, "Fundo Novo", TipoInvestimento.Multimercado,
            MoedaPatrimonio.BRL, "BTG", null, 10_000m, 11_000m, 8m);

        await handler.Handle(command, CancellationToken.None);

        inv.Nome.Should().Be("Fundo Novo");
        inv.ValorAtual.Should().Be(11_000m);
        _repoMock.Verify(r => r.Update(inv), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NotFound_ShouldThrowKeyNotFoundException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Investimento?)null);

        var handler = new UpdateInvestimentoCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);
        var command = new UpdateInvestimentoCommand(Guid.NewGuid(), "X", TipoInvestimento.Outro,
            MoedaPatrimonio.BRL, null, null, 1m, 1m, null);

        await handler.Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<KeyNotFoundException>();

        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_OtherUser_ShouldThrowUnauthorizedAccessException()
    {
        var inv = new Investimento(Guid.NewGuid(), "Alheio", TipoInvestimento.Acoes,
            MoedaPatrimonio.BRL, null, null, 1m, 1m, null);
        _repoMock.Setup(r => r.GetByIdAsync(inv.Id, It.IsAny<CancellationToken>())).ReturnsAsync(inv);

        var handler = new UpdateInvestimentoCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);
        var command = new UpdateInvestimentoCommand(inv.Id, "X", TipoInvestimento.Outro,
            MoedaPatrimonio.BRL, null, null, 1m, 1m, null);

        await handler.Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<UnauthorizedAccessException>();

        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
