using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Consultoria.Commands.SaveConsultoriaConfig;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Consultoria;

public class SaveConsultoriaConfigCommandHandlerTests
{
    private readonly Mock<IConsultoriaConfigRepository> _repoMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private static readonly Guid AssessorId = Guid.NewGuid();

    public SaveConsultoriaConfigCommandHandlerTests()
    {
        _currentUserMock.Setup(c => c.RealUserId).Returns(AssessorId);
        _currentUserMock.Setup(c => c.IsAssessor).Returns(true);
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
    }

    private SaveConsultoriaConfigCommandHandler CreateHandler() =>
        new(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);

    private static SaveConsultoriaConfigCommand Cmd() =>
        new("Matrin Wealth", "data:image/png;base64,AAAA", "#16a34a", "11999999999", "Confidencial");

    [Fact]
    public async Task Handle_SemConfig_ShouldAddAndSave()
    {
        _repoMock.Setup(r => r.GetByUsuarioAsync(AssessorId, It.IsAny<CancellationToken>())).ReturnsAsync((ConsultoriaConfig?)null);

        await CreateHandler().Handle(Cmd(), CancellationToken.None);

        _repoMock.Verify(r => r.AddAsync(It.Is<ConsultoriaConfig>(c =>
            c.UsuarioId == AssessorId && c.NomeConsultoria == "Matrin Wealth" && c.WhatsApp == "11999999999"),
            It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ComConfig_ShouldUpdateAndSave()
    {
        var existing = new ConsultoriaConfig(AssessorId, "Antiga", null, null, null, null);
        _repoMock.Setup(r => r.GetByUsuarioAsync(AssessorId, It.IsAny<CancellationToken>())).ReturnsAsync(existing);

        await CreateHandler().Handle(Cmd(), CancellationToken.None);

        existing.NomeConsultoria.Should().Be("Matrin Wealth");
        existing.CorMarca.Should().Be("#16a34a");
        _repoMock.Verify(r => r.Update(existing), Times.Once);
        _repoMock.Verify(r => r.AddAsync(It.IsAny<ConsultoriaConfig>(), It.IsAny<CancellationToken>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NaoAssessor_ShouldThrowAndNotSave()
    {
        _currentUserMock.Setup(c => c.IsAssessor).Returns(false);

        await CreateHandler().Invoking(h => h.Handle(Cmd(), CancellationToken.None))
            .Should().ThrowAsync<UnauthorizedAccessException>();

        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
