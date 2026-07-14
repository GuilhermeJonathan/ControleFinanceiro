using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Simulacoes;
using ControleFinanceiro.Application.Simulacoes.Commands.CreateSimulacao;
using ControleFinanceiro.Application.Simulacoes.Commands.DeleteSimulacao;
using ControleFinanceiro.Application.Simulacoes.Commands.UpdateSimulacao;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Simulacoes;

public class SimulacaoCommandHandlersTests
{
    private readonly Mock<ISimulacaoPatrimonialRepository> _repoMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private static readonly Guid UserId = Guid.NewGuid();

    public SimulacaoCommandHandlersTests()
    {
        _currentUserMock.Setup(c => c.UserId).Returns(UserId);
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
    }

    private static CreateSimulacaoCommand ValidCreate(IReadOnlyList<CenarioInput>? cenarios = null) =>
        new("Aposentadoria aos 55", true, 25, 55, 100_000m, false, 2_000m, 4m, 10_000m,
            cenarios ?? []);

    // ── Create ──
    [Fact]
    public async Task Create_ValidCommand_ShouldAddWithCenariosAndReturnId()
    {
        var handler = new CreateSimulacaoCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);
        var cmd = ValidCreate(new[]
        {
            new CenarioInput("Resgate carro", TipoCenario.ResgateExtra, 80_000m, 40, null),
            new CenarioInput("Aporte bônus", TipoCenario.AporteExtra, 1_000m, 30, 40),
        });

        var id = await handler.Handle(cmd, CancellationToken.None);

        id.Should().NotBeEmpty();
        _repoMock.Verify(r => r.AddAsync(It.Is<SimulacaoPatrimonial>(sm =>
            sm.Nome == "Aposentadoria aos 55" &&
            sm.UsuarioId == UserId &&
            sm.IdadeAlvo == 55 &&
            sm.Cenarios.Count == 2), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_RepositoryError_ShouldNotSave()
    {
        _repoMock.Setup(r => r.AddAsync(It.IsAny<SimulacaoPatrimonial>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("db error"));

        var handler = new CreateSimulacaoCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);

        await handler.Invoking(h => h.Handle(ValidCreate(), CancellationToken.None)).Should().ThrowAsync<Exception>();

        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── Update ──
    [Fact]
    public async Task Update_Existing_ShouldReplaceCenariosAndSave()
    {
        var sim = new SimulacaoPatrimonial(UserId, "Antiga", false, 30, 60, 0m, true, 1_000m, 5m, 8_000m,
            new[] { new Cenario("Velho", TipoCenario.AporteExtra, 500m, 31, null) });
        _repoMock.Setup(r => r.GetByIdAsync(sim.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sim);

        var handler = new UpdateSimulacaoCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);
        await handler.Handle(new UpdateSimulacaoCommand(
            sim.Id, "Nova", true, 32, 62, 50_000m, false, 3_000m, 6m, 12_000m,
            new[] { new CenarioInput("Novo", TipoCenario.ResgateExtra, 200_000m, 45, null) }), CancellationToken.None);

        sim.Nome.Should().Be("Nova");
        sim.Cenarios.Should().ContainSingle(c => c.Nome == "Novo");
        _repoMock.Verify(r => r.Update(sim), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_NotFound_ShouldThrowAndNotSave()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((SimulacaoPatrimonial?)null);

        var handler = new UpdateSimulacaoCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);

        await handler.Invoking(h => h.Handle(new UpdateSimulacaoCommand(
            Guid.NewGuid(), "X", false, 1, 2, 0m, false, 0m, 0m, 0m, []), CancellationToken.None))
            .Should().ThrowAsync<KeyNotFoundException>();

        _repoMock.Verify(r => r.Update(It.IsAny<SimulacaoPatrimonial>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Update_OtherUsers_ShouldThrowUnauthorizedAndNotSave()
    {
        var sim = new SimulacaoPatrimonial(Guid.NewGuid(), "Alheia", false, 30, 60, 0m, false, 0m, 0m, 0m);
        _repoMock.Setup(r => r.GetByIdAsync(sim.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sim);

        var handler = new UpdateSimulacaoCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);

        await handler.Invoking(h => h.Handle(new UpdateSimulacaoCommand(
            sim.Id, "X", false, 1, 2, 0m, false, 0m, 0m, 0m, []), CancellationToken.None))
            .Should().ThrowAsync<UnauthorizedAccessException>();

        _repoMock.Verify(r => r.Update(It.IsAny<SimulacaoPatrimonial>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── Delete ──
    [Fact]
    public async Task Delete_Existing_ShouldRemoveAndSave()
    {
        var sim = new SimulacaoPatrimonial(UserId, "Sim", false, 30, 60, 0m, false, 0m, 0m, 0m);
        _repoMock.Setup(r => r.GetByIdAsync(sim.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sim);

        var handler = new DeleteSimulacaoCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);
        await handler.Handle(new DeleteSimulacaoCommand(sim.Id), CancellationToken.None);

        _repoMock.Verify(r => r.Remove(sim), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_NotFound_ShouldThrowAndNotSave()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((SimulacaoPatrimonial?)null);

        var handler = new DeleteSimulacaoCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);

        await handler.Invoking(h => h.Handle(new DeleteSimulacaoCommand(Guid.NewGuid()), CancellationToken.None))
            .Should().ThrowAsync<KeyNotFoundException>();

        _repoMock.Verify(r => r.Remove(It.IsAny<SimulacaoPatrimonial>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
