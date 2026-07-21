using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Patrimonio.Commands.Estruturas;
using ControleFinanceiro.Application.Patrimonio.Queries.GetEstruturas;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Patrimonio;

public class EstruturasHandlersTests
{
    private readonly Mock<IEstruturaRepository> _repo = new();
    private readonly Mock<IAtivoPatrimonialRepository> _ativoRepo = new();
    private readonly Mock<IInvestimentoRepository> _invRepo = new();
    private readonly Mock<IFxRateResolver> _fx = new();
    private readonly Mock<ICurrentUser> _user = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private static readonly Guid UserId = Guid.NewGuid();

    public EstruturasHandlersTests()
    {
        _user.Setup(u => u.UserId).Returns(UserId);
        _fx.Setup(f => f.GetRatesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase) { ["BRL"] = 1m });
    }

    private static Estrutura Est(Guid id, string nome)
    {
        var e = new Estrutura(UserId, nome, TipoEstrutura.HoldingPatrimonial);
        typeof(Estrutura).GetProperty(nameof(Estrutura.Id))!.SetValue(e, id);
        return e;
    }

    // ── Valor derivado (grafo) ───────────────────────────────────────────

    [Fact]
    public async Task GetEstruturas_DerivaValorDosAtivosEParticipacoes()
    {
        var holdingImoveis = Guid.NewGuid();  // detém imóveis diretos
        var holdingPart    = Guid.NewGuid();  // detém 100% da holdingImoveis
        _repo.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Estrutura> { Est(holdingImoveis, "Holding Imóveis"), Est(holdingPart, "Holding Part.") });
        _repo.Setup(r => r.GetParticipacoesByUsuarioAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ParticipacaoEstrutura>
            {
                new(UserId, holdingPart, holdingImoveis, 100m, TipoRelacaoEstrutura.PropriedadeDireta),
            });

        var imovel = new AtivoPatrimonial(UserId, "Ed. SP", TipoAtivo.Imovel, MoedaPatrimonio.BRL, 1_000_000m, estruturaId: holdingImoveis);
        _ativoRepo.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync(new[] { imovel });
        _invRepo.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<Investimento>());
        _repo.Setup(r => r.GetBeneficiariosByUsuarioAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<Beneficiario>());

        var h = new GetEstruturasQueryHandler(_repo.Object, _ativoRepo.Object, _invRepo.Object, _fx.Object, _user.Object);
        var r = await h.Handle(new GetEstruturasQuery(), CancellationToken.None);

        r.Estruturas.Single(e => e.Id == holdingImoveis).ValorTotalBRL.Should().Be(1_000_000m);
        r.Estruturas.Single(e => e.Id == holdingPart).ValorDiretoBRL.Should().Be(0m);
        r.Estruturas.Single(e => e.Id == holdingPart).ValorTotalBRL.Should().Be(1_000_000m); // 100% da filha
        r.TotalEmEstruturasBRL.Should().Be(1_000_000m);
        r.TotalPessoaFisicaBRL.Should().Be(0m);
    }

    [Fact]
    public async Task GetDetalhe_ListaItensLigadosEFilhas()
    {
        var holding = Guid.NewGuid();
        var filha   = Guid.NewGuid();
        _repo.Setup(r => r.GetByIdAsync(holding, It.IsAny<CancellationToken>())).ReturnsAsync(Est(holding, "Holding"));
        _repo.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Estrutura> { Est(holding, "Holding"), Est(filha, "Filha") });
        _repo.Setup(r => r.GetParticipacoesByUsuarioAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ParticipacaoEstrutura> { new(UserId, holding, filha, 50m, TipoRelacaoEstrutura.PropriedadeDireta) });

        var imovel = new AtivoPatrimonial(UserId, "Ed. SP", TipoAtivo.Imovel, MoedaPatrimonio.BRL, 1_000_000m, estruturaId: holding);
        var invFilha = new Investimento(UserId, "Fundo", TipoInvestimento.RendaFixa, MoedaPatrimonio.BRL, null, null, 0m, 400_000m, estruturaId: filha);
        _ativoRepo.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync(new[] { imovel });
        _invRepo.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync(new[] { invFilha });

        var h = new GetEstruturaDetalheQueryHandler(_repo.Object, _ativoRepo.Object, _invRepo.Object, _fx.Object, _user.Object);
        var d = await h.Handle(new GetEstruturaDetalheQuery(holding), CancellationToken.None);

        d.Itens.Should().ContainSingle(i => i.Nome == "Ed. SP" && i.ValorBRL == 1_000_000m);
        d.ValorDiretoBRL.Should().Be(1_000_000m);
        d.Filhas.Should().ContainSingle(f => f.Id == filha);
        d.Filhas.Single().ValorParticipacaoBRL.Should().Be(200_000m); // 50% de 400k
        d.ValorTotalBRL.Should().Be(1_200_000m);                       // direto + 50% da filha
    }

    // ── Anticiclo ─────────────────────────────────────────────────────────

    [Fact]
    public async Task SaveParticipacao_Ciclo_ShouldThrow()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => Est(id, "X"));
        // Já existe A → B. Tentar B → A fecha o ciclo.
        _repo.Setup(r => r.GetParticipacoesByUsuarioAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ParticipacaoEstrutura> { new(UserId, a, b, 100m, TipoRelacaoEstrutura.PropriedadeDireta) });

        var h = new SaveParticipacaoCommandHandler(_repo.Object, _user.Object, _uow.Object);
        var act = () => h.Handle(new SaveParticipacaoCommand(b, a, 100m, TipoRelacaoEstrutura.PropriedadeDireta), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SaveParticipacao_AutoReferencia_ShouldThrow()
    {
        var a = Guid.NewGuid();
        var h = new SaveParticipacaoCommandHandler(_repo.Object, _user.Object, _uow.Object);
        var act = () => h.Handle(new SaveParticipacaoCommand(a, a, 50m, TipoRelacaoEstrutura.PropriedadeDireta), CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ── Delete solta os ativos ────────────────────────────────────────────

    [Fact]
    public async Task DeleteEstrutura_DesvinculaAtivosERemoveArestas()
    {
        var estId = Guid.NewGuid();
        _repo.Setup(r => r.GetByIdAsync(estId, It.IsAny<CancellationToken>())).ReturnsAsync(Est(estId, "Holding"));
        var ativo = new AtivoPatrimonial(UserId, "Ed.", TipoAtivo.Imovel, MoedaPatrimonio.BRL, 100m, estruturaId: estId);
        _ativoRepo.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync(new[] { ativo });
        _invRepo.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<Investimento>());
        var aresta = new ParticipacaoEstrutura(UserId, null, estId, 100m, TipoRelacaoEstrutura.PropriedadeDireta);
        _repo.Setup(r => r.GetParticipacoesByUsuarioAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<ParticipacaoEstrutura> { aresta });

        var h = new DeleteEstruturaCommandHandler(_repo.Object, _ativoRepo.Object, _invRepo.Object, _user.Object, _uow.Object);
        await h.Handle(new DeleteEstruturaCommand(estId), CancellationToken.None);

        ativo.EstruturaId.Should().BeNull();
        _repo.Verify(r => r.RemoveParticipacao(aresta), Times.Once);
        _repo.Verify(r => r.Remove(It.Is<Estrutura>(e => e.Id == estId)), Times.Once);
    }
}
