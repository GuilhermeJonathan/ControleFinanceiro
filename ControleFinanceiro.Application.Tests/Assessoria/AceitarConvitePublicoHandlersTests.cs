using ControleFinanceiro.Application.Assessoria.Commands.AceitarConvitePublico;
using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Corretores.Commands;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Assessoria;

public class AceitarConvitePublicoHandlersTests
{
    private static readonly Guid AssessorId = Guid.NewGuid();
    private static readonly Guid NovoUserId = Guid.NewGuid();

    private static Mock<ILoginProvisionClient> ProvisionOk() =>
        MockProvision(new ProvisionContaResult("jwt_token", NovoUserId, true));

    private static Mock<ILoginProvisionClient> MockProvision(ProvisionContaResult r)
    {
        var m = new Mock<ILoginProvisionClient>();
        m.Setup(c => c.ProvisionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(r);
        return m;
    }

    // ── Assessoria ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Assessoria_ConviteValido_CriaContaEAceita()
    {
        var vinculo = VinculoAssessoria.Criar(AssessorId, "COD123", "Assessor", "cliente@x.com");
        var repo = new Mock<IVinculoAssessoriaRepository>();
        repo.Setup(r => r.GetByCodigoAsync("COD123", It.IsAny<CancellationToken>())).ReturnsAsync(vinculo);
        repo.Setup(r => r.GetByClienteAsync(NovoUserId, It.IsAny<CancellationToken>())).ReturnsAsync((VinculoAssessoria?)null);
        var uow = new Mock<IUnitOfWork>();

        var handler = new AceitarConvitePublicoAssessoriaCommandHandler(repo.Object, ProvisionOk().Object, uow.Object);
        var result = await handler.Handle(new AceitarConvitePublicoAssessoriaCommand("COD123", "Fulano", "senha123"), CancellationToken.None);

        result.AccessToken.Should().Be("jwt_token");
        vinculo.AceitoEm.Should().NotBeNull();
        vinculo.ClienteId.Should().Be(NovoUserId);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Assessoria_CodigoInvalido_LancaKeyNotFound()
    {
        var repo = new Mock<IVinculoAssessoriaRepository>();
        repo.Setup(r => r.GetByCodigoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((VinculoAssessoria?)null);
        var uow = new Mock<IUnitOfWork>();

        var handler = new AceitarConvitePublicoAssessoriaCommandHandler(repo.Object, ProvisionOk().Object, uow.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new AceitarConvitePublicoAssessoriaCommand("XXX", "Fulano", "senha123"), CancellationToken.None));
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Assessoria_ConviteSemEmail_LancaInvalidOperationENaoProvisiona()
    {
        var vinculo = VinculoAssessoria.Criar(AssessorId, "COD123", "Assessor"); // sem e-mail
        var repo = new Mock<IVinculoAssessoriaRepository>();
        repo.Setup(r => r.GetByCodigoAsync("COD123", It.IsAny<CancellationToken>())).ReturnsAsync(vinculo);
        var login = ProvisionOk();
        var uow = new Mock<IUnitOfWork>();

        var handler = new AceitarConvitePublicoAssessoriaCommandHandler(repo.Object, login.Object, uow.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new AceitarConvitePublicoAssessoriaCommand("COD123", "Fulano", "senha123"), CancellationToken.None));
        login.Verify(c => c.ProvisionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── Corretor ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Corretor_ConviteValido_CriaContaCorretorEAceita()
    {
        var vinculo = VinculoCorretor.Criar(AssessorId, "COR777", "Assessor", "corretor@x.com");
        var repo = new Mock<IVinculoCorretorRepository>();
        repo.Setup(r => r.GetByCodigoAsync("COR777", It.IsAny<CancellationToken>())).ReturnsAsync(vinculo);
        repo.Setup(r => r.GetByCorretorAsync(NovoUserId, It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<VinculoCorretor>());
        var login = ProvisionOk();
        var uow = new Mock<IUnitOfWork>();

        var handler = new AceitarConvitePublicoCorretorCommandHandler(repo.Object, login.Object, uow.Object);
        var result = await handler.Handle(new AceitarConvitePublicoCorretorCommand("COR777", "Corretor Y", "senha123"), CancellationToken.None);

        result.AccessToken.Should().Be("jwt_token");
        vinculo.AceitoEm.Should().NotBeNull();
        // Provisiona como Corretor (userType 4)
        login.Verify(c => c.ProvisionAsync(It.IsAny<string>(), "corretor@x.com", It.IsAny<string>(),
            It.IsAny<string?>(), (int)UserTypeConvite.Corretor, It.IsAny<CancellationToken>()), Times.Once);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
