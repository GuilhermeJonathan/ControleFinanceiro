using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Patrimonio.Commands.CreatePassivo;
using ControleFinanceiro.Application.Patrimonio.Commands.DeletePassivo;
using ControleFinanceiro.Application.Patrimonio.Commands.UpdatePassivo;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Patrimonio;

public class PassivoPatrimonialCommandHandlersTests
{
    private readonly Mock<IPassivoPatrimonialRepository> _repoMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private static readonly Guid UserId = Guid.NewGuid();

    public PassivoPatrimonialCommandHandlersTests()
    {
        _currentUserMock.Setup(c => c.UserId).Returns(UserId);
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
    }

    // ── Create ──
    [Fact]
    public async Task Create_ValidCommand_ShouldAddPassivoAndReturnId()
    {
        var handler = new CreatePassivoPatrimonialCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);
        var command = new CreatePassivoPatrimonialCommand("Lombard loan", MoedaPatrimonio.EUR, 180_000m, PrazoDivida.Curto, 6m, 120);

        var id = await handler.Handle(command, CancellationToken.None);

        id.Should().NotBeEmpty();
        _repoMock.Verify(r => r.AddAsync(It.Is<PassivoPatrimonial>(p =>
            p.Nome == "Lombard loan" &&
            p.Moeda == MoedaPatrimonio.EUR &&
            p.Valor == 180_000m &&
            p.Prazo == PrazoDivida.Curto &&
            p.UsuarioId == UserId), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_RepositoryError_ShouldNotSave()
    {
        _repoMock.Setup(r => r.AddAsync(It.IsAny<PassivoPatrimonial>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("db error"));

        var handler = new CreatePassivoPatrimonialCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);
        var command = new CreatePassivoPatrimonialCommand("X", MoedaPatrimonio.BRL, 100m, PrazoDivida.Longo);

        await handler.Invoking(h => h.Handle(command, CancellationToken.None)).Should().ThrowAsync<Exception>();

        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── Update ──
    [Fact]
    public async Task Update_Existing_ShouldUpdateAndSave()
    {
        var passivo = new PassivoPatrimonial(UserId, "Antigo", MoedaPatrimonio.BRL, 50_000m, PrazoDivida.Curto);
        _repoMock.Setup(r => r.GetByIdAsync(passivo.Id, It.IsAny<CancellationToken>())).ReturnsAsync(passivo);

        var handler = new UpdatePassivoPatrimonialCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);
        await handler.Handle(new UpdatePassivoPatrimonialCommand(
            passivo.Id, "Novo", MoedaPatrimonio.USD, 60_000m, PrazoDivida.Longo, 5m, 60), CancellationToken.None);

        passivo.Nome.Should().Be("Novo");
        passivo.Moeda.Should().Be(MoedaPatrimonio.USD);
        passivo.Prazo.Should().Be(PrazoDivida.Longo);
        _repoMock.Verify(r => r.Update(passivo), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_NotFound_ShouldThrowAndNotSave()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((PassivoPatrimonial?)null);

        var handler = new UpdatePassivoPatrimonialCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);

        await handler.Invoking(h => h.Handle(new UpdatePassivoPatrimonialCommand(
            Guid.NewGuid(), "X", MoedaPatrimonio.BRL, 1m, PrazoDivida.Curto), CancellationToken.None))
            .Should().ThrowAsync<KeyNotFoundException>();

        _repoMock.Verify(r => r.Update(It.IsAny<PassivoPatrimonial>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Update_OtherUsers_ShouldThrowUnauthorizedAndNotSave()
    {
        var passivo = new PassivoPatrimonial(Guid.NewGuid(), "Alheio", MoedaPatrimonio.BRL, 1m, PrazoDivida.Curto);
        _repoMock.Setup(r => r.GetByIdAsync(passivo.Id, It.IsAny<CancellationToken>())).ReturnsAsync(passivo);

        var handler = new UpdatePassivoPatrimonialCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);

        await handler.Invoking(h => h.Handle(new UpdatePassivoPatrimonialCommand(
            passivo.Id, "X", MoedaPatrimonio.BRL, 1m, PrazoDivida.Curto), CancellationToken.None))
            .Should().ThrowAsync<UnauthorizedAccessException>();

        _repoMock.Verify(r => r.Update(It.IsAny<PassivoPatrimonial>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── Delete ──
    [Fact]
    public async Task Delete_Existing_ShouldRemoveAndSave()
    {
        var passivo = new PassivoPatrimonial(UserId, "Financiamento", MoedaPatrimonio.BRL, 200_000m, PrazoDivida.Longo);
        _repoMock.Setup(r => r.GetByIdAsync(passivo.Id, It.IsAny<CancellationToken>())).ReturnsAsync(passivo);

        var handler = new DeletePassivoPatrimonialCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);
        await handler.Handle(new DeletePassivoPatrimonialCommand(passivo.Id), CancellationToken.None);

        _repoMock.Verify(r => r.Remove(passivo), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_NotFound_ShouldThrowAndNotSave()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((PassivoPatrimonial?)null);

        var handler = new DeletePassivoPatrimonialCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);

        await handler.Invoking(h => h.Handle(new DeletePassivoPatrimonialCommand(Guid.NewGuid()), CancellationToken.None))
            .Should().ThrowAsync<KeyNotFoundException>();

        _repoMock.Verify(r => r.Remove(It.IsAny<PassivoPatrimonial>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
