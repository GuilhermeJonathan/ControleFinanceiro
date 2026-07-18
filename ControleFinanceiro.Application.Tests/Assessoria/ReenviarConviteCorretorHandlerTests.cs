using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Corretores.Commands;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using Moq;

namespace ControleFinanceiro.Application.Tests.Assessoria;

public class ReenviarConviteCorretorHandlerTests
{
    private static readonly Guid AssessorId = Guid.NewGuid();
    private readonly Mock<IVinculoCorretorRepository> _repo = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Mock<IEmailService> _email = new();
    private readonly Mock<IConsultoriaConfigRepository> _consultoria = new();
    private readonly Mock<Microsoft.Extensions.Configuration.IConfiguration> _config = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    private ReenviarConviteCorretorCommandHandler Build() => new(
        _repo.Object, _currentUser.Object, _email.Object, _consultoria.Object, _config.Object, _uow.Object);

    public ReenviarConviteCorretorHandlerTests() => _currentUser.Setup(c => c.RealUserId).Returns(AssessorId);

    [Fact]
    public async Task Reenviar_Pendente_RenovaValidadeEEnviaEmail()
    {
        var v = VinculoCorretor.Criar(AssessorId, "COR777", "Assessor", "corretor@x.com");
        _repo.Setup(r => r.GetByIdAsync(v.Id, It.IsAny<CancellationToken>())).ReturnsAsync(v);

        await Build().Handle(new ReenviarConviteCorretorCommand(v.Id), CancellationToken.None);

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _email.Verify(e => e.SendAsync("corretor@x.com", It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<string?>()), Times.Once);
    }

    [Fact]
    public async Task Reenviar_NaoEncontrado_LancaKeyNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((VinculoCorretor?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            Build().Handle(new ReenviarConviteCorretorCommand(Guid.NewGuid()), CancellationToken.None));
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Reenviar_SemEmail_LancaInvalidOperationENaoEnvia()
    {
        var v = VinculoCorretor.Criar(AssessorId, "COR888", "Assessor"); // sem e-mail
        _repo.Setup(r => r.GetByIdAsync(v.Id, It.IsAny<CancellationToken>())).ReturnsAsync(v);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Build().Handle(new ReenviarConviteCorretorCommand(v.Id), CancellationToken.None));
        _email.Verify(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<string?>()), Times.Never);
    }
}
