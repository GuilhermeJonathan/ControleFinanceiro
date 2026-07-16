using ControleFinanceiro.Application.Assessoria.Commands.CriarRecomendacao;
using ControleFinanceiro.Application.Assessoria.Commands.ExcluirRecomendacao;
using ControleFinanceiro.Application.Assessoria.Commands.ResponderRecomendacao;
using ControleFinanceiro.Application.Assessoria.Queries.GetRecomendacoes;
using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Assessoria;

public class RecomendacaoHandlersTests
{
    private readonly Mock<IRecomendacaoRepository> _repoMock = new();
    private readonly Mock<IVinculoAssessoriaRepository> _vinculoRepoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly Mock<IUserNameLookup> _lookupMock = new();
    private readonly Mock<IEmailService> _emailMock = new();
    private readonly Mock<IConsultoriaConfigRepository> _consultoriaMock = new();
    private readonly Mock<Microsoft.Extensions.Configuration.IConfiguration> _configMock = new();

    private CriarRecomendacaoCommandHandler BuildCriarHandler() => new(
        _repoMock.Object, _vinculoRepoMock.Object, _currentUserMock.Object, _uowMock.Object,
        _lookupMock.Object, _emailMock.Object, _consultoriaMock.Object, _configMock.Object,
        NullLogger<CriarRecomendacaoCommandHandler>.Instance);

    private static readonly Guid AssessorId = Guid.NewGuid();
    private static readonly Guid ClienteId = Guid.NewGuid();

    public RecomendacaoHandlersTests()
    {
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _currentUserMock.Setup(c => c.RealUserId).Returns(AssessorId);
    }

    private static VinculoAssessoria VinculoAtivo()
    {
        var v = VinculoAssessoria.Criar(AssessorId, "ABC123");
        v.Aceitar(ClienteId, "Cliente");
        return v;
    }

    // ── Criar ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Criar_ComVinculoAtivo_ShouldPersist()
    {
        _vinculoRepoMock.Setup(r => r.GetVinculoAtivoAsync(AssessorId, ClienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(VinculoAtivo());
        _lookupMock.Setup(l => l.GetContatoAsync(ClienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserContato("Cliente", "cliente@test.com"));

        var handler = BuildCriarHandler();

        var id = await handler.Handle(
            new CriarRecomendacaoCommand(ClienteId, (int)TipoRecomendacao.Dica, "Reduza gastos com lazer", null),
            CancellationToken.None);

        id.Should().NotBeEmpty();
        _repoMock.Verify(r => r.AddAsync(
            It.Is<Recomendacao>(x => x.ClienteId == ClienteId && x.Status == StatusRecomendacao.Pendente),
            It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Criar_SemVinculo_ShouldThrowUnauthorizedAndNotSave()
    {
        _vinculoRepoMock.Setup(r => r.GetVinculoAtivoAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VinculoAssessoria?)null);

        var handler = BuildCriarHandler();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new CriarRecomendacaoCommand(ClienteId, 2, "Texto", null), CancellationToken.None));
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Recomendacao>(), It.IsAny<CancellationToken>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Criar_ShouldSendEmailToCliente()
    {
        _vinculoRepoMock.Setup(r => r.GetVinculoAtivoAsync(AssessorId, ClienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(VinculoAtivo());
        _lookupMock.Setup(l => l.GetContatoAsync(ClienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserContato("Cliente", "cliente@test.com"));

        var handler = BuildCriarHandler();
        await handler.Handle(new CriarRecomendacaoCommand(ClienteId, 2, "Dica", null), CancellationToken.None);

        _emailMock.Verify(e => e.SendAsync(
            "cliente@test.com", It.IsAny<string>(),
            It.Is<string>(s => s.Contains("recomendação")),
            It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Criar_EmailFalha_ShouldStillPersistRecomendacao()
    {
        _vinculoRepoMock.Setup(r => r.GetVinculoAtivoAsync(AssessorId, ClienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(VinculoAtivo());
        _lookupMock.Setup(l => l.GetContatoAsync(ClienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserContato("Cliente", "cliente@test.com"));
        _emailMock.Setup(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("resend down"));

        var handler = BuildCriarHandler();
        var id = await handler.Handle(new CriarRecomendacaoCommand(ClienteId, 2, "Dica", null), CancellationToken.None);

        id.Should().NotBeEmpty();
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── Responder ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Responder_PeloCliente_ShouldAcceptAndSave()
    {
        var rec = new Recomendacao(AssessorId, ClienteId, TipoRecomendacao.Dica, "Texto");
        _repoMock.Setup(r => r.GetByIdAsync(rec.Id, It.IsAny<CancellationToken>())).ReturnsAsync(rec);
        _currentUserMock.Setup(c => c.RealUserId).Returns(ClienteId);

        var handler = new ResponderRecomendacaoCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);
        await handler.Handle(new ResponderRecomendacaoCommand(rec.Id, true, "Vou aplicar!"), CancellationToken.None);

        rec.Status.Should().Be(StatusRecomendacao.Aceita);
        rec.RespostaCliente.Should().Be("Vou aplicar!");
        rec.RespondidoEm.Should().NotBeNull();
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Responder_PorOutroUsuario_ShouldThrowUnauthorized()
    {
        var rec = new Recomendacao(AssessorId, ClienteId, TipoRecomendacao.Dica, "Texto");
        _repoMock.Setup(r => r.GetByIdAsync(rec.Id, It.IsAny<CancellationToken>())).ReturnsAsync(rec);
        _currentUserMock.Setup(c => c.RealUserId).Returns(Guid.NewGuid());

        var handler = new ResponderRecomendacaoCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new ResponderRecomendacaoCommand(rec.Id, true, null), CancellationToken.None));
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Responder_JaRespondida_ShouldThrowInvalidOperation()
    {
        var rec = new Recomendacao(AssessorId, ClienteId, TipoRecomendacao.Dica, "Texto");
        rec.Responder(StatusRecomendacao.Aceita, null);
        _repoMock.Setup(r => r.GetByIdAsync(rec.Id, It.IsAny<CancellationToken>())).ReturnsAsync(rec);
        _currentUserMock.Setup(c => c.RealUserId).Returns(ClienteId);

        var handler = new ResponderRecomendacaoCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new ResponderRecomendacaoCommand(rec.Id, false, null), CancellationToken.None));
    }

    [Fact]
    public async Task Responder_Inexistente_ShouldThrowKeyNotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Recomendacao?)null);

        var handler = new ResponderRecomendacaoCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new ResponderRecomendacaoCommand(Guid.NewGuid(), true, null), CancellationToken.None));
    }

    // ── Excluir ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Excluir_PendentePeloAutor_ShouldRemove()
    {
        var rec = new Recomendacao(AssessorId, ClienteId, TipoRecomendacao.Alerta, "Texto");
        _repoMock.Setup(r => r.GetByIdAsync(rec.Id, It.IsAny<CancellationToken>())).ReturnsAsync(rec);

        var handler = new ExcluirRecomendacaoCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);
        await handler.Handle(new ExcluirRecomendacaoCommand(rec.Id), CancellationToken.None);

        _repoMock.Verify(r => r.Remove(rec), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Excluir_JaRespondida_ShouldThrowInvalidOperationAndNotRemove()
    {
        var rec = new Recomendacao(AssessorId, ClienteId, TipoRecomendacao.Alerta, "Texto");
        rec.Responder(StatusRecomendacao.Recusada, null);
        _repoMock.Setup(r => r.GetByIdAsync(rec.Id, It.IsAny<CancellationToken>())).ReturnsAsync(rec);

        var handler = new ExcluirRecomendacaoCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new ExcluirRecomendacaoCommand(rec.Id), CancellationToken.None));
        _repoMock.Verify(r => r.Remove(It.IsAny<Recomendacao>()), Times.Never);
    }

    // ── Queries ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetCliente_ShouldReturnDtos()
    {
        _currentUserMock.Setup(c => c.RealUserId).Returns(ClienteId);
        _repoMock.Setup(r => r.GetByClienteAsync(ClienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { new Recomendacao(AssessorId, ClienteId, TipoRecomendacao.Dica, "Dica 1") });

        var handler = new GetRecomendacoesClienteQueryHandler(_repoMock.Object, _currentUserMock.Object);
        var result = (await handler.Handle(new GetRecomendacoesClienteQuery(), CancellationToken.None)).ToList();

        result.Should().HaveCount(1);
        result[0].Texto.Should().Be("Dica 1");
        result[0].Status.Should().Be((int)StatusRecomendacao.Pendente);
    }

    [Fact]
    public async Task GetAssessor_ShouldReturnDtosDoCliente()
    {
        _repoMock.Setup(r => r.GetByAssessorEClienteAsync(AssessorId, ClienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { new Recomendacao(AssessorId, ClienteId, TipoRecomendacao.Alerta, "Alerta 1") });

        var handler = new GetRecomendacoesAssessorQueryHandler(_repoMock.Object, _currentUserMock.Object);
        var result = (await handler.Handle(new GetRecomendacoesAssessorQuery(ClienteId), CancellationToken.None)).ToList();

        result.Should().HaveCount(1);
        result[0].Tipo.Should().Be((int)TipoRecomendacao.Alerta);
    }
}
