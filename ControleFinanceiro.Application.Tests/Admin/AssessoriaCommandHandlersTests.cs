using ControleFinanceiro.Application.Admin.Commands;
using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Admin;

public class AssessoriaCommandHandlersTests
{
    private readonly Mock<ILoginProvisionClient> _provision = new();
    private readonly Mock<IConsultoriaConfigRepository> _consultoria = new();
    private readonly Mock<ICurrentUser> _user = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    public AssessoriaCommandHandlersTests() => _user.Setup(u => u.IsAdmin).Returns(true);

    private CriarAssessoriaCommandHandler CriarHandler() => new(_provision.Object, _consultoria.Object, _user.Object, _uow.Object);

    [Fact]
    public async Task Criar_HappyPath_ProvisionaESemeiaConsultoria()
    {
        var novoId = Guid.NewGuid();
        _provision.Setup(p => p.ProvisionAsync("Adriel", "a@x.com", "secret1", null, 3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProvisionContaResult("token", novoId, true));
        _consultoria.Setup(r => r.GetByUsuarioAsync(novoId, It.IsAny<CancellationToken>())).ReturnsAsync((ConsultoriaConfig?)null);

        var id = await CriarHandler().Handle(new CriarAssessoriaCommand("Adriel", "a@x.com", "secret1", "Aurea Capital"), CancellationToken.None);

        id.Should().Be(novoId);
        _consultoria.Verify(r => r.AddAsync(It.Is<ConsultoriaConfig>(c => c.UsuarioId == novoId && c.NomeConsultoria == "Aurea Capital"), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Criar_NaoAdmin_LancaEnaoProvisiona()
    {
        _user.Setup(u => u.IsAdmin).Returns(false);

        await CriarHandler().Invoking(h => h.Handle(new CriarAssessoriaCommand("A", "a@x.com", "secret1", null), CancellationToken.None))
            .Should().ThrowAsync<UnauthorizedAccessException>();

        _provision.Verify(p => p.ProvisionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Criar_SenhaCurta_LancaEnaoProvisiona()
    {
        await CriarHandler().Invoking(h => h.Handle(new CriarAssessoriaCommand("A", "a@x.com", "123", null), CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>();

        _provision.Verify(p => p.ProvisionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Atualizar_Existente_AtualizaNome()
    {
        var assessorId = Guid.NewGuid();
        var cfg = new ConsultoriaConfig(assessorId, "Antigo", null, null, null, null);
        _consultoria.Setup(r => r.GetByUsuarioAsync(assessorId, It.IsAny<CancellationToken>())).ReturnsAsync(cfg);

        var h = new AtualizarAssessoriaCommandHandler(_consultoria.Object, _user.Object, _uow.Object);
        await h.Handle(new AtualizarAssessoriaCommand(assessorId, "Novo Nome", null, "#2563eb", null, null), CancellationToken.None);

        cfg.NomeConsultoria.Should().Be("Novo Nome");
        cfg.CorMarca.Should().Be("#2563eb");
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Atualizar_NaoAdmin_Lanca()
    {
        _user.Setup(u => u.IsAdmin).Returns(false);
        var h = new AtualizarAssessoriaCommandHandler(_consultoria.Object, _user.Object, _uow.Object);

        await h.Invoking(x => x.Handle(new AtualizarAssessoriaCommand(Guid.NewGuid(), "X", null, null, null, null), CancellationToken.None))
            .Should().ThrowAsync<UnauthorizedAccessException>();

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
