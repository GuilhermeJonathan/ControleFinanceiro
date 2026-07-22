using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Patrimonio.Queries.GetContas;
using ControleFinanceiro.Application.Patrimonio.Queries.GetEstruturas;
using ControleFinanceiro.Application.Patrimonio.Queries.GetPlanoAcao;
using ControleFinanceiro.Application.Patrimonio.Queries.GetProjecaoDividas;
using ControleFinanceiro.Application.Patrimonio.Queries.GetResumoInvestimentos;
using ControleFinanceiro.Application.Patrimonio.Queries.GetResumoPatrimonial;
using ControleFinanceiro.Application.Patrimonio.Queries.GetSucessao;
using ControleFinanceiro.Application.Relatorios;
using ControleFinanceiro.Application.Relatorios.Queries.GerarRelatorio;
using ControleFinanceiro.Application.Simulacoes.Queries.GetSimulacoes;
using FluentAssertions;
using MediatR;
using Moq;

namespace ControleFinanceiro.Application.Tests.Relatorios;

public class GerarRelatorioCompletoQueryHandlerTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Mock<ControleFinanceiro.Domain.Repositories.IConsultoriaConfigRepository> _consultoria = new();
    private readonly Mock<IRelatorioCompletoGenerator> _generator = new();
    private static readonly byte[] PdfFake = { 0x25, 0x50, 0x44, 0x46 };

    public GerarRelatorioCompletoQueryHandlerTests()
    {
        _currentUser.Setup(c => c.RealUserName).Returns("Assessor Teste");
        _mediator.Setup(m => m.Send(It.IsAny<GetResumoPatrimonialQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ResumoPatrimonialDto());
        _mediator.Setup(m => m.Send(It.IsAny<GetProjecaoDividasQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ProjecaoDividasDto());
        _mediator.Setup(m => m.Send(It.IsAny<GetResumoInvestimentosQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ResumoInvestimentosDto());
        _mediator.Setup(m => m.Send(It.IsAny<GetSimulacoesQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(Enumerable.Empty<SimulacaoDto>());
        _mediator.Setup(m => m.Send(It.IsAny<GetPlanosAcaoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(Enumerable.Empty<PlanoAcaoDto>());
        _mediator.Setup(m => m.Send(It.IsAny<GetEstruturasQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new GrafoEstruturasDto());
        _mediator.Setup(m => m.Send(It.IsAny<GetSucessaoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new SucessaoDto());
        _mediator.Setup(m => m.Send(It.IsAny<GetContasQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ContasResultDto([], 0m));
        _mediator.Setup(m => m.Send(It.IsAny<GetIndicadoresSucessaoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new IndicadoresSucessaoDto());
        _generator.Setup(g => g.Gerar(It.IsAny<RelatorioPatrimonialDados>(), It.IsAny<RelatorioSucessaoDados>(), It.IsAny<RelatorioBranding>())).Returns(PdfFake);
    }

    [Fact]
    public async Task Handle_GeraPdfComOsDoisConjuntos()
    {
        var h = new GerarRelatorioCompletoQueryHandler(_mediator.Object, _currentUser.Object, _consultoria.Object, _generator.Object);

        var pdf = await h.Handle(new GerarRelatorioCompletoQuery("Marina", new RelatorioBranding("Aurea", null, "#16a34a")), CancellationToken.None);

        pdf.Should().BeEquivalentTo(PdfFake);
        _generator.Verify(g => g.Gerar(
            It.Is<RelatorioPatrimonialDados>(d => d.ClienteNome == "Marina"),
            It.Is<RelatorioSucessaoDados>(d => d.ClienteNome == "Marina"),
            It.IsAny<RelatorioBranding>()), Times.Once);
    }
}
