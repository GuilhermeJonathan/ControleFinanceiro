using ControleFinanceiro.Application.Common.Interfaces;
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
    private GetPlanoAcaoQueryHandler GetHandler() =>
        new(_repoMock.Object, _currentUserMock.Object);

    private static EtapaPlanoInput Etapa(string titulo, int status = 1) =>
        new(titulo, "desc", "2027", "alvo", status);

    [Fact]
    public async Task Save_SemPlanoExistente_ShouldAddAndPersist()
    {
        _repoMock.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlanoAcao?)null);
        PlanoAcao? adicionado = null;
        _repoMock.Setup(r => r.AddAsync(It.IsAny<PlanoAcao>(), It.IsAny<CancellationToken>()))
            .Callback<PlanoAcao, CancellationToken>((p, _) => adicionado = p)
            .Returns(Task.CompletedTask);

        await SaveHandler().Handle(
            new SavePlanoAcaoCommand("Blindar patrimônio", "2028",
                new[] { Etapa("Holding", 3), Etapa("Sucessão", 2) }),
            CancellationToken.None);

        adicionado.Should().NotBeNull();
        adicionado!.UsuarioId.Should().Be(UserId);
        adicionado.Objetivo.Should().Be("Blindar patrimônio");
        adicionado.Etapas.Should().HaveCount(2);
        adicionado.Etapas.First().Ordem.Should().Be(0);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Save_ComPlanoExistente_ShouldUpdateNotAdd()
    {
        var existente = new PlanoAcao(UserId, "Antigo", null,
            new[] { new EtapaPlano(0, "Velha", null, null, null, StatusEtapa.Pendente) });
        _repoMock.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existente);

        await SaveHandler().Handle(
            new SavePlanoAcaoCommand("Novo objetivo", null, new[] { Etapa("Nova") }),
            CancellationToken.None);

        existente.Objetivo.Should().Be("Novo objetivo");
        existente.Etapas.Should().ContainSingle(e => e.Titulo == "Nova");
        _repoMock.Verify(r => r.Update(existente), Times.Once);
        _repoMock.Verify(r => r.AddAsync(It.IsAny<PlanoAcao>(), It.IsAny<CancellationToken>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Save_SemObjetivo_ShouldThrowAndNotPersist()
    {
        var act = async () => await SaveHandler().Handle(
            new SavePlanoAcaoCommand("   ", null, Array.Empty<EtapaPlanoInput>()),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _repoMock.Verify(r => r.AddAsync(It.IsAny<PlanoAcao>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Save_EtapasSemTitulo_ShouldBeIgnored()
    {
        _repoMock.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlanoAcao?)null);
        PlanoAcao? adicionado = null;
        _repoMock.Setup(r => r.AddAsync(It.IsAny<PlanoAcao>(), It.IsAny<CancellationToken>()))
            .Callback<PlanoAcao, CancellationToken>((p, _) => adicionado = p)
            .Returns(Task.CompletedTask);

        await SaveHandler().Handle(
            new SavePlanoAcaoCommand("Obj", null,
                new[] { Etapa("Válida"), new EtapaPlanoInput("  ", null, null, null, 1) }),
            CancellationToken.None);

        adicionado!.Etapas.Should().ContainSingle(e => e.Titulo == "Válida");
    }

    [Fact]
    public async Task Get_SemPlano_ShouldReturnNull()
    {
        _repoMock.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlanoAcao?)null);

        (await GetHandler().Handle(new GetPlanoAcaoQuery(), CancellationToken.None)).Should().BeNull();
    }

    [Fact]
    public async Task Get_ComPlano_ShouldMapOrderedEtapas()
    {
        var plano = new PlanoAcao(UserId, "Objetivo X", "2030", new[]
        {
            new EtapaPlano(1, "Segunda", null, "2027", null, StatusEtapa.Pendente),
            new EtapaPlano(0, "Primeira", "d", "2026", "alvo", StatusEtapa.Concluida),
        });
        _repoMock.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plano);

        var dto = await GetHandler().Handle(new GetPlanoAcaoQuery(), CancellationToken.None);

        dto.Should().NotBeNull();
        dto!.Objetivo.Should().Be("Objetivo X");
        dto.Etapas.Select(e => e.Titulo).Should().ContainInOrder("Primeira", "Segunda");
        dto.Etapas.First().Status.Should().Be((int)StatusEtapa.Concluida);
    }
}
