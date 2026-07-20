using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Patrimonio.Queries.GetPlanoAcao;
using ControleFinanceiro.Application.Patrimonio.Queries.GetProjecaoDividas;
using ControleFinanceiro.Application.Patrimonio.Queries.GetResumoInvestimentos;
using ControleFinanceiro.Application.Patrimonio.Queries.GetResumoPatrimonial;
using ControleFinanceiro.Application.Relatorios;
using ControleFinanceiro.Application.Relatorios.Queries.GerarRelatorio;
using ControleFinanceiro.Application.Simulacoes.Queries.GetSimulacoes;
using FluentAssertions;
using MediatR;
using Moq;

namespace ControleFinanceiro.Application.Tests.Relatorios;

public class GerarRelatorioPatrimonialQueryHandlerTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Mock<ControleFinanceiro.Domain.Repositories.IConsultoriaConfigRepository> _consultoria = new();
    private readonly Mock<IRelatorioPatrimonialGenerator> _generator = new();
    private static readonly byte[] PdfFake = { 0x25, 0x50, 0x44, 0x46 }; // "%PDF"

    public GerarRelatorioPatrimonialQueryHandlerTests()
    {
        _currentUser.Setup(c => c.RealUserName).Returns("Assessor Teste");
        _mediator.Setup(m => m.Send(It.IsAny<GetResumoPatrimonialQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResumoPatrimonialDto());
        _mediator.Setup(m => m.Send(It.IsAny<GetProjecaoDividasQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProjecaoDividasDto());
        _mediator.Setup(m => m.Send(It.IsAny<GetResumoInvestimentosQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResumoInvestimentosDto());
        _mediator.Setup(m => m.Send(It.IsAny<GetSimulacoesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<SimulacaoDto>());
        _mediator.Setup(m => m.Send(It.IsAny<GetPlanosAcaoQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<PlanoAcaoDto>());
        _generator.Setup(g => g.Gerar(It.IsAny<RelatorioPatrimonialDados>(), It.IsAny<RelatorioBranding>()))
            .Returns(PdfFake);
    }

    private GerarRelatorioPatrimonialQueryHandler CreateHandler() =>
        new(_mediator.Object, _currentUser.Object, _consultoria.Object, _generator.Object);

    [Fact]
    public async Task Handle_ShouldGatherDataAndReturnPdfBytes()
    {
        var query = new GerarRelatorioPatrimonialQuery("Guilherme", new RelatorioBranding("Matrin", null, "#16a34a"));

        var pdf = await CreateHandler().Handle(query, CancellationToken.None);

        pdf.Should().BeEquivalentTo(PdfFake);
        _generator.Verify(g => g.Gerar(
            It.Is<RelatorioPatrimonialDados>(d => d.ClienteNome == "Guilherme" && d.AssessorNome == "Assessor Teste"),
            It.Is<RelatorioBranding>(b => b.NomeConsultoria == "Matrin")), Times.Once);
    }

    [Fact]
    public async Task Handle_SemClienteNome_ShouldFallbackToCliente()
    {
        var query = new GerarRelatorioPatrimonialQuery(null, new RelatorioBranding(null, null, null));

        await CreateHandler().Handle(query, CancellationToken.None);

        _generator.Verify(g => g.Gerar(
            It.Is<RelatorioPatrimonialDados>(d => d.ClienteNome == "Cliente"),
            It.IsAny<RelatorioBranding>()), Times.Once);
    }
}
