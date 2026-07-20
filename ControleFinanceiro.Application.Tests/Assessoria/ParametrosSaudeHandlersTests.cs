using ControleFinanceiro.Application.Assessoria.Commands.SaveParametrosSaude;
using ControleFinanceiro.Application.Assessoria.Queries.GetParametrosSaude;
using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Assessoria;

public class ParametrosSaudeHandlersTests
{
    private readonly Mock<IParametrosSaudeRepository> _repoMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private static readonly Guid AssessorId = Guid.NewGuid();

    public ParametrosSaudeHandlersTests()
    {
        _currentUserMock.Setup(c => c.RealUserId).Returns(AssessorId);
    }

    [Fact]
    public async Task Get_SemConfig_ShouldReturnPadrao()
    {
        _repoMock.Setup(r => r.GetByAssessorAsync(AssessorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ParametrosSaude?)null);

        var dto = await new GetParametrosSaudeQueryHandler(_repoMock.Object, _currentUserMock.Object)
            .Handle(new GetParametrosSaudeQuery(), CancellationToken.None);

        dto.ScoreBoaMin.Should().Be(60);
        dto.ComprometimentoSaudavelMax.Should().Be(50);
        dto.ReservaExcelenteMinDias.Should().Be(90);
    }

    [Fact]
    public async Task Save_SemConfig_ShouldAddClamped()
    {
        _repoMock.Setup(r => r.GetByAssessorAsync(AssessorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ParametrosSaude?)null);
        ParametrosSaude? add = null;
        _repoMock.Setup(r => r.AddAsync(It.IsAny<ParametrosSaude>(), It.IsAny<CancellationToken>()))
            .Callback<ParametrosSaude, CancellationToken>((p, _) => add = p).Returns(Task.CompletedTask);

        await new SaveParametrosSaudeCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object)
            .Handle(new SaveParametrosSaudeCommand(999, 70, 45, 60, 75, 90, 120, 45, 20), CancellationToken.None);

        add.Should().NotBeNull();
        add!.AssessorId.Should().Be(AssessorId);
        add.ScoreExcelenteMin.Should().Be(100);  // 999 clampado a 100
        add.ScoreBoaMin.Should().Be(70);
        add.ComprometimentoSaudavelMax.Should().Be(60);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Save_ComConfig_ShouldUpdateNotAdd()
    {
        var existente = new ParametrosSaude(AssessorId);
        _repoMock.Setup(r => r.GetByAssessorAsync(AssessorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existente);

        await new SaveParametrosSaudeCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object)
            .Handle(new SaveParametrosSaudeCommand(85, 65, 40, 55, 72, 88, 100, 40, 20), CancellationToken.None);

        existente.ScoreBoaMin.Should().Be(65);
        _repoMock.Verify(r => r.Update(existente), Times.Once);
        _repoMock.Verify(r => r.AddAsync(It.IsAny<ParametrosSaude>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
