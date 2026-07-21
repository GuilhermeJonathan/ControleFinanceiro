using ControleFinanceiro.Application.Admin.Queries.GetAdminOverview;
using ControleFinanceiro.Application.Common.Interfaces;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Admin;

public class GetAdminOverviewQueryHandlerTests
{
    private readonly Mock<ICurrentUser> _user = new();
    private readonly Mock<IAdminOverviewProvider> _provider = new();

    private GetAdminOverviewQueryHandler Handler() => new(_user.Object, _provider.Object);

    [Fact]
    public async Task Handle_NaoAdmin_ShouldThrow()
    {
        _user.Setup(u => u.IsAdmin).Returns(false);

        var act = () => Handler().Handle(new GetAdminOverviewQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        _provider.Verify(p => p.GetAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Admin_RetornaResumoDoProvider()
    {
        _user.Setup(u => u.IsAdmin).Returns(true);
        var esperado = new AdminOverviewDto(
            QtdAssessorias: 2, QtdClientes: 5, QtdCorretores: 1, AumTotalBRL: 1_000_000m, QtdParametrosGlobais: 12,
            Assessorias: new[] { new AssessoriaResumoDto(Guid.NewGuid(), "Consultoria X", 3, 1, 800_000m) });
        _provider.Setup(p => p.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync(esperado);

        var r = await Handler().Handle(new GetAdminOverviewQuery(), CancellationToken.None);

        r.Should().BeSameAs(esperado);
        r.QtdAssessorias.Should().Be(2);
    }
}
