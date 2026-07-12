using ControleFinanceiro.Application.Assessoria.Commands.AceitarConviteAssessoria;
using ControleFinanceiro.Application.Assessoria.Commands.GerarConviteAssessoria;
using ControleFinanceiro.Application.Assessoria.Commands.RevogarVinculoAssessoria;
using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Assessoria;

public class AssessoriaCommandHandlersTests
{
    private readonly Mock<IVinculoAssessoriaRepository> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();

    private static readonly Guid AssessorId = Guid.NewGuid();
    private static readonly Guid ClienteId = Guid.NewGuid();

    public AssessoriaCommandHandlersTests()
    {
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _currentUserMock.Setup(c => c.RealUserId).Returns(AssessorId);
        _currentUserMock.Setup(c => c.RealUserName).Returns("Assessor Teste");
        _currentUserMock.Setup(c => c.IsAssessor).Returns(true);
        _currentUserMock.Setup(c => c.TemPlanoAssessor).Returns(true);
        _repoMock.Setup(r => r.GetByAssessorAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<VinculoAssessoria>());
    }

    // ── GerarConvite ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GerarConvite_Assessor_ShouldCreateVinculoAndReturnCodigo()
    {
        _repoMock.Setup(r => r.GetByCodigoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VinculoAssessoria?)null);

        var handler = new GerarConviteAssessoriaCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);
        var codigo = await handler.Handle(new GerarConviteAssessoriaCommand(), CancellationToken.None);

        codigo.Should().HaveLength(6);
        _repoMock.Verify(r => r.AddAsync(
            It.Is<VinculoAssessoria>(v => v.AssessorId == AssessorId && v.NomeAssessor == "Assessor Teste"),
            It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GerarConvite_SemPlanoNoLimite_ShouldThrowInvalidOperation()
    {
        _currentUserMock.Setup(c => c.TemPlanoAssessor).Returns(false);
        var carteiraCheia = Enumerable.Range(0, GerarConviteAssessoriaCommandHandler.MaxClientesSemPlano)
            .Select(i => VinculoAssessoria.Criar(AssessorId, $"COD{i:D3}"))
            .ToArray();
        _repoMock.Setup(r => r.GetByAssessorAsync(AssessorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(carteiraCheia);

        var handler = new GerarConviteAssessoriaCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new GerarConviteAssessoriaCommand(), CancellationToken.None));
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GerarConvite_NaoAssessor_ShouldThrowAndNotSave()
    {
        _currentUserMock.Setup(c => c.IsAssessor).Returns(false);

        var handler = new GerarConviteAssessoriaCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new GerarConviteAssessoriaCommand(), CancellationToken.None));
        _repoMock.Verify(r => r.AddAsync(It.IsAny<VinculoAssessoria>(), It.IsAny<CancellationToken>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── AceitarConvite ───────────────────────────────────────────────────────

    [Fact]
    public async Task Aceitar_CodigoValido_ShouldActivateVinculo()
    {
        var vinculo = VinculoAssessoria.Criar(AssessorId, "ABC123", "Assessor Teste");
        _repoMock.Setup(r => r.GetByCodigoAsync("ABC123", It.IsAny<CancellationToken>())).ReturnsAsync(vinculo);
        _repoMock.Setup(r => r.GetByClienteAsync(ClienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((VinculoAssessoria?)null);
        _currentUserMock.Setup(c => c.RealUserId).Returns(ClienteId);

        var handler = new AceitarConviteAssessoriaCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);
        await handler.Handle(new AceitarConviteAssessoriaCommand("ABC123", "Cliente Teste"), CancellationToken.None);

        vinculo.Ativo.Should().BeTrue();
        vinculo.ClienteId.Should().Be(ClienteId);
        vinculo.NomeCliente.Should().Be("Cliente Teste");
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Aceitar_CodigoInvalido_ShouldThrowKeyNotFoundAndNotSave()
    {
        _repoMock.Setup(r => r.GetByCodigoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VinculoAssessoria?)null);

        var handler = new AceitarConviteAssessoriaCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new AceitarConviteAssessoriaCommand("XXXXXX", "Cliente"), CancellationToken.None));
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Aceitar_ClienteJaTemAssessor_ShouldThrowInvalidOperation()
    {
        var vinculoNovo = VinculoAssessoria.Criar(AssessorId, "ABC123");
        var vinculoExistente = VinculoAssessoria.Criar(Guid.NewGuid(), "ZZZ999");
        vinculoExistente.Aceitar(ClienteId, "Cliente");

        _repoMock.Setup(r => r.GetByCodigoAsync("ABC123", It.IsAny<CancellationToken>())).ReturnsAsync(vinculoNovo);
        _repoMock.Setup(r => r.GetByClienteAsync(ClienteId, It.IsAny<CancellationToken>())).ReturnsAsync(vinculoExistente);
        _currentUserMock.Setup(c => c.RealUserId).Returns(ClienteId);

        var handler = new AceitarConviteAssessoriaCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new AceitarConviteAssessoriaCommand("ABC123", "Cliente"), CancellationToken.None));
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Aceitar_ProprioConvite_ShouldThrowInvalidOperation()
    {
        var vinculo = VinculoAssessoria.Criar(AssessorId, "ABC123");
        _repoMock.Setup(r => r.GetByCodigoAsync("ABC123", It.IsAny<CancellationToken>())).ReturnsAsync(vinculo);
        _repoMock.Setup(r => r.GetByClienteAsync(AssessorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((VinculoAssessoria?)null);
        // currentUser.RealUserId == AssessorId (setup do construtor)

        var handler = new AceitarConviteAssessoriaCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new AceitarConviteAssessoriaCommand("ABC123", "Eu Mesmo"), CancellationToken.None));
    }

    // ── Revogar ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Revogar_PeloCliente_ShouldRevokeVinculo()
    {
        var vinculo = VinculoAssessoria.Criar(AssessorId, "ABC123");
        vinculo.Aceitar(ClienteId, "Cliente");
        _repoMock.Setup(r => r.GetByIdAsync(vinculo.Id, It.IsAny<CancellationToken>())).ReturnsAsync(vinculo);
        _currentUserMock.Setup(c => c.RealUserId).Returns(ClienteId);

        var handler = new RevogarVinculoAssessoriaCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);
        await handler.Handle(new RevogarVinculoAssessoriaCommand(vinculo.Id), CancellationToken.None);

        vinculo.Ativo.Should().BeFalse();
        vinculo.RevogadoEm.Should().NotBeNull();
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Revogar_PorTerceiro_ShouldThrowUnauthorizedAndNotSave()
    {
        var vinculo = VinculoAssessoria.Criar(AssessorId, "ABC123");
        vinculo.Aceitar(ClienteId, "Cliente");
        _repoMock.Setup(r => r.GetByIdAsync(vinculo.Id, It.IsAny<CancellationToken>())).ReturnsAsync(vinculo);
        _currentUserMock.Setup(c => c.RealUserId).Returns(Guid.NewGuid());

        var handler = new RevogarVinculoAssessoriaCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new RevogarVinculoAssessoriaCommand(vinculo.Id), CancellationToken.None));
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Revogar_VinculoInexistente_ShouldThrowKeyNotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VinculoAssessoria?)null);

        var handler = new RevogarVinculoAssessoriaCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new RevogarVinculoAssessoriaCommand(Guid.NewGuid()), CancellationToken.None));
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
