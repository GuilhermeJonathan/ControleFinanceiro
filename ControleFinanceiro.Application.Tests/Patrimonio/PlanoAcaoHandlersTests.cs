using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Patrimonio.Commands.DeletePlanoAcao;
using ControleFinanceiro.Application.Patrimonio.Commands.SavePlanoAcao;
using ControleFinanceiro.Application.Patrimonio.Queries.GetPlanoAcao;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Patrimonio;

public class PlanoAcaoHandlersTests
{
    private readonly Mock<IPlanoAcaoRepository> _repoMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private static readonly Guid UserId = Guid.NewGuid();

    public PlanoAcaoHandlersTests()
    {
        _currentUserMock.Setup(c => c.UserId).Returns(UserId);
    }

    private SavePlanoAcaoCommandHandler SaveHandler() =>
        new(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);
    private GetPlanosAcaoQueryHandler GetHandler() =>
        new(_repoMock.Object, _currentUserMock.Object);
    private DeletePlanoAcaoCommandHandler DeleteHandler() =>
        new(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);

    private static EtapaPlanoInput Etapa(string titulo, int status = 1) =>
        new(titulo, "desc", "mar/2027", "alvo", status);

    [Fact]
    public async Task Save_SemId_ShouldAddNewPlan()
    {
        PlanoAcao? adicionado = null;
        _repoMock.Setup(r => r.AddAsync(It.IsAny<PlanoAcao>(), It.IsAny<CancellationToken>()))
            .Callback<PlanoAcao, CancellationToken>((p, _) => adicionado = p)
            .Returns(Task.CompletedTask);

        var id = await SaveHandler().Handle(
            new SavePlanoAcaoCommand(null, "Blindar patrimônio", "2028",
                new[] { Etapa("Holding", 3), Etapa("Sucessão", 2) }),
            CancellationToken.None);

        adicionado.Should().NotBeNull();
        adicionado!.UsuarioId.Should().Be(UserId);
        adicionado.Etapas.Should().HaveCount(2);
        id.Should().Be(adicionado.Id);
        _repoMock.Verify(r => r.Update(It.IsAny<PlanoAcao>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Save_ComId_ShouldUpdateExisting()
    {
        var existente = new PlanoAcao(UserId, "Antigo", null,
            new[] { new EtapaPlano(0, "Velha", null, null, null, StatusEtapa.Pendente) });
        _repoMock.Setup(r => r.GetByIdAsync(existente.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existente);

        await SaveHandler().Handle(
            new SavePlanoAcaoCommand(existente.Id, "Novo objetivo", null, new[] { Etapa("Nova") }),
            CancellationToken.None);

        existente.Objetivo.Should().Be("Novo objetivo");
        existente.Etapas.Should().ContainSingle(e => e.Titulo == "Nova");
        _repoMock.Verify(r => r.Update(existente), Times.Once);
        _repoMock.Verify(r => r.AddAsync(It.IsAny<PlanoAcao>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Save_ComId_DeOutroUsuario_ShouldThrowAndNotPersist()
    {
        var alheio = new PlanoAcao(Guid.NewGuid(), "Alheio", null);
        _repoMock.Setup(r => r.GetByIdAsync(alheio.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(alheio);

        var act = async () => await SaveHandler().Handle(
            new SavePlanoAcaoCommand(alheio.Id, "Hack", null, Array.Empty<EtapaPlanoInput>()),
            CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Save_SemObjetivo_ShouldThrow()
    {
        var act = async () => await SaveHandler().Handle(
            new SavePlanoAcaoCommand(null, "   ", null, Array.Empty<EtapaPlanoInput>()),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        _repoMock.Verify(r => r.AddAsync(It.IsAny<PlanoAcao>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Get_ShouldReturnAllPlansOfUser_OrderedEtapas()
    {
        var p1 = new PlanoAcao(UserId, "Plano A", "2030", new[]
        {
            new EtapaPlano(1, "Segunda", null, "2027", null, StatusEtapa.Pendente),
            new EtapaPlano(0, "Primeira", "d", "2026", "alvo", StatusEtapa.Concluida),
        });
        var p2 = new PlanoAcao(UserId, "Plano B", null);
        _repoMock.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { p1, p2 });

        var lista = (await GetHandler().Handle(new GetPlanosAcaoQuery(), CancellationToken.None)).ToList();

        lista.Should().HaveCount(2);
        var a = lista.First(x => x.Objetivo == "Plano A");
        a.Etapas.Select(e => e.Titulo).Should().ContainInOrder("Primeira", "Segunda");
    }

    [Fact]
    public async Task Delete_DoProprioUsuario_ShouldRemoveAndPersist()
    {
        var plano = new PlanoAcao(UserId, "Plano", null);
        _repoMock.Setup(r => r.GetByIdAsync(plano.Id, It.IsAny<CancellationToken>())).ReturnsAsync(plano);

        await DeleteHandler().Handle(new DeletePlanoAcaoCommand(plano.Id), CancellationToken.None);

        _repoMock.Verify(r => r.Remove(plano), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_DeOutroUsuario_ShouldThrowAndNotRemove()
    {
        var alheio = new PlanoAcao(Guid.NewGuid(), "Alheio", null);
        _repoMock.Setup(r => r.GetByIdAsync(alheio.Id, It.IsAny<CancellationToken>())).ReturnsAsync(alheio);

        var act = async () => await DeleteHandler().Handle(new DeletePlanoAcaoCommand(alheio.Id), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
        _repoMock.Verify(r => r.Remove(It.IsAny<PlanoAcao>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
