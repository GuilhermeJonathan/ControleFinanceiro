using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Parametros.Commands;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Parametros;

public class AtualizarCotacoesMoedasHandlerTests
{
    private readonly Mock<IMoedaParamRepository> _moedaRepo = new();
    private readonly Mock<ICurrencyRateService> _rateService = new();
    private readonly Mock<ICotacaoHistoricoRepository> _histRepo = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    private AtualizarCotacoesMoedasCommandHandler Handler() =>
        new(_moedaRepo.Object, _rateService.Object, _histRepo.Object, _uow.Object);

    [Fact]
    public async Task Handle_ShouldUpdateRatesAndWriteHistory()
    {
        var usd = new MoedaParam("USD", "Dólar", 1, 5.0m);
        var eur = new MoedaParam("EUR", "Euro", 2, 6.0m);
        var brl = new MoedaParam("BRL", "Real", 0, 1m);
        _moedaRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<MoedaParam> { usd, eur, brl });
        _rateService.Setup(r => r.GetRatesVsBrlAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase) { ["USD"] = 5.42m, ["EUR"] = 6.15m });

        var r = await Handler().Handle(new AtualizarCotacoesMoedasCommand(true), CancellationToken.None);

        r.Atualizadas.Should().Be(2);
        r.Pulado.Should().BeFalse();
        usd.CotacaoBRL.Should().Be(5.42m);
        eur.CotacaoBRL.Should().Be(6.15m);
        _histRepo.Verify(h => h.AddAsync(It.IsAny<CotacaoHistorico>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_SemForcar_ComCotacaoRecente_ShouldSkip()
    {
        var usd = new MoedaParam("USD", "Dólar", 1, 5.0m);
        usd.AtualizarCotacao(5.4m); // seta CotacaoAtualizadaEm = agora
        _moedaRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<MoedaParam> { usd });

        var r = await Handler().Handle(new AtualizarCotacoesMoedasCommand(false), CancellationToken.None);

        r.Pulado.Should().BeTrue();
        _rateService.Verify(s => s.GetRatesVsBrlAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
