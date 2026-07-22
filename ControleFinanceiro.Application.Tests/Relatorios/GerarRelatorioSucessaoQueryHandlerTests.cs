using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Patrimonio.Queries.GetContas;
using ControleFinanceiro.Application.Patrimonio.Queries.GetEstruturas;
using ControleFinanceiro.Application.Patrimonio.Queries.GetPlanoAcao;
using ControleFinanceiro.Application.Patrimonio.Queries.GetSucessao;
using ControleFinanceiro.Application.Relatorios;
using ControleFinanceiro.Application.Relatorios.Queries.GerarRelatorio;
using FluentAssertions;
using MediatR;
using Moq;

namespace ControleFinanceiro.Application.Tests.Relatorios;

public class GerarRelatorioSucessaoQueryHandlerTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Mock<ControleFinanceiro.Domain.Repositories.IConsultoriaConfigRepository> _consultoria = new();
    private readonly Mock<IRelatorioSucessaoGenerator> _generator = new();
    private static readonly byte[] PdfFake = { 0x25, 0x50, 0x44, 0x46 }; // "%PDF"

    public GerarRelatorioSucessaoQueryHandlerTests()
    {
        _currentUser.Setup(c => c.RealUserName).Returns("Assessor Teste");
        _mediator.Setup(m => m.Send(It.IsAny<GetEstruturasQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GrafoEstruturasDto());
        _mediator.Setup(m => m.Send(It.IsAny<GetSucessaoQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SucessaoDto());
        _mediator.Setup(m => m.Send(It.IsAny<GetContasQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContasResultDto([], 0m));
        _mediator.Setup(m => m.Send(It.IsAny<GetPlanosAcaoQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<PlanoAcaoDto>());
        _generator.Setup(g => g.Gerar(It.IsAny<RelatorioSucessaoDados>(), It.IsAny<RelatorioBranding>()))
            .Returns(PdfFake);
    }

    private GerarRelatorioSucessaoQueryHandler CreateHandler() =>
        new(_mediator.Object, _currentUser.Object, _consultoria.Object, _generator.Object);

    [Fact]
    public async Task Handle_ShouldGatherDataAndReturnPdfBytes()
    {
        var query = new GerarRelatorioSucessaoQuery("Marina", new RelatorioBranding("Matrin", null, "#16a34a"));

        var pdf = await CreateHandler().Handle(query, CancellationToken.None);

        pdf.Should().BeEquivalentTo(PdfFake);
        _generator.Verify(g => g.Gerar(
            It.Is<RelatorioSucessaoDados>(d => d.ClienteNome == "Marina" && d.AssessorNome == "Assessor Teste"),
            It.Is<RelatorioBranding>(b => b.NomeConsultoria == "Matrin")), Times.Once);
    }

    [Fact]
    public async Task Handle_SemClienteNome_ShouldFallbackToCliente()
    {
        var query = new GerarRelatorioSucessaoQuery(null, new RelatorioBranding(null, null, null));

        await CreateHandler().Handle(query, CancellationToken.None);

        _generator.Verify(g => g.Gerar(
            It.Is<RelatorioSucessaoDados>(d => d.ClienteNome == "Cliente"),
            It.IsAny<RelatorioBranding>()), Times.Once);
    }
}
