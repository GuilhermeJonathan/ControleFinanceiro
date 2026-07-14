using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Patrimonio.Commands.UpdateAtivo;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Patrimonio;

public class UpdateAtivoPatrimonialCommandHandlerTests
{
    private readonly Mock<IAtivoPatrimonialRepository> _repoMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private static readonly Guid UserId = Guid.NewGuid();

    public UpdateAtivoPatrimonialCommandHandlerTests()
    {
        _currentUserMock.Setup(c => c.UserId).Returns(UserId);
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Handle_ExistingAtivo_ShouldUpdateAndSave()
    {
        var ativo = new AtivoPatrimonial(UserId, "Carro Antigo", TipoAtivo.Veiculo, MoedaPatrimonio.BRL, 80_000m, null);
        _repoMock.Setup(r => r.GetByIdAsync(ativo.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ativo);

        var handler = new UpdateAtivoPatrimonialCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);
        var command = new UpdateAtivoPatrimonialCommand(ativo.Id, "Carro Novo", TipoAtivo.Veiculo, MoedaPatrimonio.BRL, 120_000m, -5m);

        await handler.Handle(command, CancellationToken.None);

        ativo.Nome.Should().Be("Carro Novo");
        ativo.ValorAtual.Should().Be(120_000m);
        ativo.ValorizacaoAnualPct.Should().Be(-5m);
        _repoMock.Verify(r => r.Update(ativo), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NotFound_ShouldThrowKeyNotFoundException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((AtivoPatrimonial?)null);

        var handler = new UpdateAtivoPatrimonialCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);
        var command = new UpdateAtivoPatrimonialCommand(Guid.NewGuid(), "X", TipoAtivo.Outro, MoedaPatrimonio.BRL, 1m, null);

        await handler.Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<KeyNotFoundException>();

        _repoMock.Verify(r => r.Update(It.IsAny<AtivoPatrimonial>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_OtherUsersAtivo_ShouldThrowUnauthorizedAccessException()
    {
        var outroUserId = Guid.NewGuid();
        var ativo = new AtivoPatrimonial(outroUserId, "Ativo Alheio", TipoAtivo.Imovel, MoedaPatrimonio.BRL, 1m, null);
        _repoMock.Setup(r => r.GetByIdAsync(ativo.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ativo);

        var handler = new UpdateAtivoPatrimonialCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);
        var command = new UpdateAtivoPatrimonialCommand(ativo.Id, "X", TipoAtivo.Outro, MoedaPatrimonio.BRL, 1m, null);

        await handler.Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<UnauthorizedAccessException>();

        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
