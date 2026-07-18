using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Patrimonio.Commands.ImportarInvestimentos;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Patrimonio;

public class ImportarInvestimentosHandlerTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private readonly Mock<IInvestimentoRepository> _repo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly List<Investimento> _capturados = new();

    public ImportarInvestimentosHandlerTests()
    {
        _currentUser.Setup(c => c.UserId).Returns(UserId);
        _repo.Setup(r => r.AddAsync(It.IsAny<Investimento>(), It.IsAny<CancellationToken>()))
            .Callback<Investimento, CancellationToken>((i, _) => _capturados.Add(i))
            .Returns(Task.CompletedTask);
    }

    private ImportarInvestimentosCommandHandler Build() => new(_repo.Object, _currentUser.Object, _uow.Object);

    [Fact]
    public async Task Handle_CsvValido_CriaInvestimentosEParseiaTipoValorMoeda()
    {
        var csv = string.Join('\n',
            "nome;tipo;corretora;ticker;valorAplicado;valorAtual;moeda",
            "ETF S&P500;ETF;XP;IVVB11;50.000,00;55.000,00;BRL",
            "Bitcoin;Cripto;Binance;BTC;10000;12000;USD");

        var result = await Build().Handle(new ImportarInvestimentosCommand(csv), CancellationToken.None);

        result.Importados.Should().Be(2);
        result.Erros.Should().BeEmpty();
        _capturados.Should().HaveCount(2);
        var etf = _capturados.Single(i => i.Nome == "ETF S&P500");
        etf.Tipo.Should().Be(TipoInvestimento.ETF);
        etf.ValorAplicado.Should().Be(50000m);
        etf.ValorAtual.Should().Be(55000m);
        etf.Moeda.Should().Be(MoedaPatrimonio.BRL);
        var btc = _capturados.Single(i => i.Nome == "Bitcoin");
        btc.Tipo.Should().Be(TipoInvestimento.Cripto);
        btc.Moeda.Should().Be(MoedaPatrimonio.USD);
        btc.ValorAtual.Should().Be(12000m);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_LinhaInvalida_ReportaErroEImportaOResto()
    {
        var csv = string.Join('\n',
            "nome,valorAtual",
            "Tesouro,abc",   // valor inválido
            "CDB,1000");

        var result = await Build().Handle(new ImportarInvestimentosCommand(csv), CancellationToken.None);

        result.Importados.Should().Be(1);
        result.Erros.Should().HaveCount(1);
        _capturados.Single().Nome.Should().Be("CDB");
    }

    [Fact]
    public async Task Handle_SemColunaObrigatoria_RetornaErro()
    {
        var result = await Build().Handle(new ImportarInvestimentosCommand("foo;bar\n1;2"), CancellationToken.None);
        result.Importados.Should().Be(0);
        result.Erros.Should().NotBeEmpty();
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
