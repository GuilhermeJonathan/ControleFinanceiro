using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Patrimonio.Commands.DeleteAtivo;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Patrimonio;

public class DeleteAtivoPatrimonialCommandHandlerTests
{
    private readonly Mock<IAtivoPatrimonialRepository> _repoMock = new();
    private readonly Mock<IPassivoPatrimonialRepository> _passivoRepoMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private static readonly Guid UserId = Guid.NewGuid();

    public DeleteAtivoPatrimonialCommandHandlerTests()
    {
        _currentUserMock.Setup(c => c.UserId).Returns(UserId);
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _passivoRepoMock.Setup(r => r.GetByUsuarioAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PassivoPatrimonial>());
    }

    private DeleteAtivoPatrimonialCommandHandler Handler() =>
        new(_repoMock.Object, _passivoRepoMock.Object, _currentUserMock.Object, _uowMock.Object);

    [Fact]
    public async Task Handle_ExistingAtivo_ShouldRemoveAndSave()
    {
        var ativo = new AtivoPatrimonial(UserId, "Apartamento", TipoAtivo.Imovel, MoedaPatrimonio.BRL, 500_000m, null);
        _repoMock.Setup(r => r.GetByIdAsync(ativo.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ativo);

        var handler = Handler();

        await handler.Handle(new DeleteAtivoPatrimonialCommand(ativo.Id), CancellationToken.None);

        _repoMock.Verify(r => r.Remove(ativo), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NotFound_ShouldThrowKeyNotFoundException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((AtivoPatrimonial?)null);

        var handler = Handler();

        await handler.Invoking(h => h.Handle(new DeleteAtivoPatrimonialCommand(Guid.NewGuid()), CancellationToken.None))
            .Should().ThrowAsync<KeyNotFoundException>();

        _repoMock.Verify(r => r.Remove(It.IsAny<AtivoPatrimonial>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_OtherUsersAtivo_ShouldThrowUnauthorizedAccessException()
    {
        var outroUserId = Guid.NewGuid();
        var ativo = new AtivoPatrimonial(outroUserId, "Ativo Alheio", TipoAtivo.Imovel, MoedaPatrimonio.BRL, 1m, null);
        _repoMock.Setup(r => r.GetByIdAsync(ativo.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ativo);

        var handler = Handler();

        await handler.Invoking(h => h.Handle(new DeleteAtivoPatrimonialCommand(ativo.Id), CancellationToken.None))
            .Should().ThrowAsync<UnauthorizedAccessException>();

        _repoMock.Verify(r => r.Remove(It.IsAny<AtivoPatrimonial>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ComDividaVinculada_DesvinculaAntesDeExcluir()
    {
        var ativo = new AtivoPatrimonial(UserId, "Imóvel", TipoAtivo.Imovel, MoedaPatrimonio.BRL, 800_000m, null);
        _repoMock.Setup(r => r.GetByIdAsync(ativo.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ativo);

        var divida = new PassivoPatrimonial(UserId, "Financiamento", MoedaPatrimonio.BRL, 300_000m,
            PrazoDivida.Longo, null, null, ativo.Id);
        _passivoRepoMock.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PassivoPatrimonial> { divida });

        await Handler().Handle(new DeleteAtivoPatrimonialCommand(ativo.Id), CancellationToken.None);

        divida.AtivoVinculadoId.Should().BeNull();               // desvinculada
        _passivoRepoMock.Verify(r => r.Update(divida), Times.Once);
        _repoMock.Verify(r => r.Remove(ativo), Times.Once);
    }
}
