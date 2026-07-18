using ControleFinanceiro.Application.Patrimonio.Queries.GetInsightsPatrimonio;
using ControleFinanceiro.Application.Patrimonio.Queries.GetRebalanceamento;
using ControleFinanceiro.Application.Patrimonio.Queries.GetResumoInvestimentos;
using ControleFinanceiro.Application.Patrimonio.Queries.GetResumoPatrimonial;
using FluentAssertions;
using MediatR;
using Moq;

namespace ControleFinanceiro.Application.Tests.Patrimonio;

public class GetInsightsPatrimonioHandlerTests
{
    private readonly Mock<ISender> _mediator = new();

    private void Setup(ResumoPatrimonialDto patr, ResumoInvestimentosDto inv, RebalanceamentoDto reb)
    {
        _mediator.Setup(m => m.Send(It.IsAny<GetResumoPatrimonialQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(patr);
        _mediator.Setup(m => m.Send(It.IsAny<GetResumoInvestimentosQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(inv);
        _mediator.Setup(m => m.Send(It.IsAny<GetRebalanceamentoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(reb);
    }

    [Fact]
    public async Task Handle_ConcentracaoAlavancagemENegativo_GeraAlertas()
    {
        var patr = new ResumoPatrimonialDto() with
        {
            AlavancagemPct = 60m,
            SaldoLiquidoMensalBRL = 0m,
            Composicao = new[] { new CategoriaComposicaoDto("Imóveis", 5_000_000m, 70m, null) },
        };
        var inv = new ResumoInvestimentosDto() with { RentabilidadePct = -5m, TotalAtualBRL = 100_000m };
        var reb = new RebalanceamentoDto(0m, false, Array.Empty<RebalanceamentoClasseDto>());
        Setup(patr, inv, reb);

        var result = (await new GetInsightsPatrimonioQueryHandler(_mediator.Object)
            .Handle(new GetInsightsPatrimonioQuery(), CancellationToken.None)).ToList();

        var titulos = result.Select(i => i.Titulo).ToList();
        titulos.Should().Contain("Patrimônio concentrado");
        titulos.Should().Contain("Alavancagem alta");
        titulos.Should().Contain("Investimentos no negativo");
        result.Should().OnlyContain(i => !string.IsNullOrWhiteSpace(i.RecomendacaoSugerida));
    }

    [Fact]
    public async Task Handle_CarteiraEquilibrada_RetornaInsightPositivo()
    {
        var patr = new ResumoPatrimonialDto() with
        {
            AlavancagemPct = 10m,
            SaldoLiquidoMensalBRL = 500m,
            Composicao = new[]
            {
                new CategoriaComposicaoDto("Imóveis", 3_000_000m, 40m, null),
                new CategoriaComposicaoDto("Investimentos", 4_000_000m, 45m, null),
            },
        };
        var inv = new ResumoInvestimentosDto() with { RentabilidadePct = 8m, TotalAtualBRL = 0m };
        var reb = new RebalanceamentoDto(0m, false, Array.Empty<RebalanceamentoClasseDto>());
        Setup(patr, inv, reb);

        var result = (await new GetInsightsPatrimonioQueryHandler(_mediator.Object)
            .Handle(new GetInsightsPatrimonioQuery(), CancellationToken.None)).ToList();

        result.Should().ContainSingle();
        result[0].Severidade.Should().Be("positivo");
    }
}
