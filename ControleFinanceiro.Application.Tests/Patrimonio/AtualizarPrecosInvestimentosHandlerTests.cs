using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Patrimonio.Commands.AtualizarPrecosInvestimentos;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Patrimonio;

public class AtualizarPrecosInvestimentosHandlerTests
{
    private readonly Mock<IInvestimentoRepository> _invRepo = new();
    private readonly Mock<IAssetPriceService> _priceService = new();
    private readonly Mock<IExteriorAssetPriceService> _exteriorPrice = new();
    private readonly Mock<ITipoInvestimentoParamRepository> _tipoRepo = new();
    private readonly Mock<IPrecoAtivoHistoricoRepository> _histRepo = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private static readonly Guid UserId = Guid.NewGuid();

    public AtualizarPrecosInvestimentosHandlerTests()
    {
        _currentUser.Setup(c => c.UserId).Returns(UserId);
        // Por padrão nenhum tipo é exterior → tudo roteia para a brapi (_priceService).
        _tipoRepo.Setup(r => r.GetGlobaisAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TipoInvestimentoParam>());
    }

    private AtualizarPrecosInvestimentosCommandHandler Handler() =>
        new(_invRepo.Object, _priceService.Object, _exteriorPrice.Object, _tipoRepo.Object,
            _histRepo.Object, _currentUser.Object, _uow.Object);

    private static Investimento ComTicker(string ticker, decimal? quantidade = 10m,
        TipoInvestimento tipo = TipoInvestimento.Acoes) =>
        new(UserId, ticker, tipo, MoedaPatrimonio.BRL, null, ticker, 100m, 100m, null, quantidade);

    [Fact]
    public async Task Handle_ShouldUpdatePricesAndWriteHistory()
    {
        var petr = ComTicker("PETR4");
        _invRepo.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync(new[] { petr });
        _priceService.Setup(s => s.GetPricesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase) { ["PETR4"] = 42.5m });

        var r = await Handler().Handle(new AtualizarPrecosInvestimentosCommand(true), CancellationToken.None);

        r.Atualizados.Should().Be(1);
        petr.ValorAtual.Should().Be(425m); // 10 cotas × 42,5
        petr.ValorAtualizadoEm.Should().NotBeNull();
        _histRepo.Verify(h => h.AddAsync(It.IsAny<PrecoAtivoHistorico>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_SemForcar_ComPrecoRecente_ShouldSkip()
    {
        var petr = ComTicker("PETR4");
        petr.AtualizarValorAutomatico(40m); // seta ValorAtualizadoEm = agora
        _invRepo.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync(new[] { petr });

        var r = await Handler().Handle(new AtualizarPrecosInvestimentosCommand(false), CancellationToken.None);

        r.Pulado.Should().BeTrue();
        _priceService.Verify(s => s.GetPricesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SemQuantidade_NaoAtualizaValor()
    {
        var petr = ComTicker("PETR4", quantidade: null); // sem quantidade → não dá para derivar valor da posição
        _invRepo.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync(new[] { petr });
        _priceService.Setup(s => s.GetPricesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase) { ["PETR4"] = 42.5m });

        var r = await Handler().Handle(new AtualizarPrecosInvestimentosCommand(true), CancellationToken.None);

        r.Atualizados.Should().Be(0);
        petr.ValorAtual.Should().Be(100m);      // inalterado
        petr.ValorAtualizadoEm.Should().BeNull();
    }

    [Fact]
    public async Task Handle_TipoExterior_UsaProvedorExterior_NaoBrapi()
    {
        // Tipo "Exterior" (id = enum Exterior) marcado como Exterior no catálogo.
        _tipoRepo.Setup(r => r.GetGlobaisAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TipoInvestimentoParam>
            {
                new((int)TipoInvestimento.Exterior, "Exterior", 7, isSystem: true, exterior: true),
            });

        var vwra = ComTicker("VWRA.L", quantidade: 10m, tipo: TipoInvestimento.Exterior);
        _invRepo.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync(new[] { vwra });
        _exteriorPrice.Setup(s => s.GetPricesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase) { ["VWRA.L"] = 194.42m });

        var r = await Handler().Handle(new AtualizarPrecosInvestimentosCommand(true), CancellationToken.None);

        r.Atualizados.Should().Be(1);
        vwra.ValorAtual.Should().Be(1944.20m); // 10 × 194,42
        _exteriorPrice.Verify(s => s.GetPricesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()), Times.Once);
        _priceService.Verify(s => s.GetPricesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
