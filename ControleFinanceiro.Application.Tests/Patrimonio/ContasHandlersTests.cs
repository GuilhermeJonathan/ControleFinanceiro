using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Patrimonio.Commands.Contas;
using ControleFinanceiro.Application.Patrimonio.Queries.GetContas;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Patrimonio;

public class ContasHandlersTests
{
    private readonly Mock<IContaFinanceiraRepository> _repo = new();
    private readonly Mock<IEstruturaRepository> _estruturaRepo = new();
    private readonly Mock<IInvestimentoRepository> _invRepo = new();
    private readonly Mock<IFxRateResolver> _fx = new();
    private readonly Mock<ICurrentUser> _user = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private static readonly Guid UserId = Guid.NewGuid();

    public ContasHandlersTests()
    {
        _user.Setup(u => u.UserId).Returns(UserId);
        _fx.Setup(f => f.GetRatesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase) { ["BRL"] = 1m, ["USD"] = 5m });
    }

    // ── Save ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Save_CriaConta()
    {
        _estruturaRepo.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<Estrutura>());
        var h = new SaveContaCommandHandler(_repo.Object, _estruturaRepo.Object, _user.Object, _uow.Object);

        var id = await h.Handle(new SaveContaCommand(null, "Conta Itaú", TipoContaFinanceira.Corrente,
            MoedaPatrimonio.BRL, 10_000m, "Itaú", "Brasil", "0001/12345-6", null), CancellationToken.None);

        id.Should().NotBeEmpty();
        _repo.Verify(r => r.AddAsync(It.Is<ContaFinanceira>(c => c.Nome == "Conta Itaú" && c.Saldo == 10_000m), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Save_SemNome_Falha()
    {
        var h = new SaveContaCommandHandler(_repo.Object, _estruturaRepo.Object, _user.Object, _uow.Object);
        var act = () => h.Handle(new SaveContaCommand(null, "  ", TipoContaFinanceira.Corrente, MoedaPatrimonio.BRL, 0m, null, null, null, null), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        _repo.Verify(r => r.AddAsync(It.IsAny<ContaFinanceira>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Save_UpdateNaoEncontrado_Falha()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((ContaFinanceira?)null);
        var h = new SaveContaCommandHandler(_repo.Object, _estruturaRepo.Object, _user.Object, _uow.Object);

        var act = () => h.Handle(new SaveContaCommand(Guid.NewGuid(), "X", TipoContaFinanceira.Corrente, MoedaPatrimonio.BRL, 0m, null, null, null, null), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── Delete ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_SoltaInvestimentosERemove()
    {
        var contaId = Guid.NewGuid();
        var conta = new ContaFinanceira(UserId, "Custódia", TipoContaFinanceira.InvestimentoCustodia, MoedaPatrimonio.USD, 0m);
        typeof(ContaFinanceira).GetProperty(nameof(ContaFinanceira.Id))!.SetValue(conta, contaId);
        _repo.Setup(r => r.GetByIdAsync(contaId, It.IsAny<CancellationToken>())).ReturnsAsync(conta);

        var inv = new Investimento(UserId, "AAPL", TipoInvestimento.Exterior, MoedaPatrimonio.USD, "Avenue", "AAPL", 100m, 120m, contaId: contaId);
        _invRepo.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync(new[] { inv });

        var h = new DeleteContaCommandHandler(_repo.Object, _invRepo.Object, _user.Object, _uow.Object);
        await h.Handle(new DeleteContaCommand(contaId), CancellationToken.None);

        inv.ContaId.Should().BeNull();
        _repo.Verify(r => r.Remove(conta), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_NaoEncontrado_Falha()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((ContaFinanceira?)null);
        var h = new DeleteContaCommandHandler(_repo.Object, _invRepo.Object, _user.Object, _uow.Object);

        var act = () => h.Handle(new DeleteContaCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
        _repo.Verify(r => r.Remove(It.IsAny<ContaFinanceira>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── GetContas: valor derivado ─────────────────────────────────────────

    [Fact]
    public async Task GetContas_CaixaUsaSaldo_CustodiaAgregaInvestimentos()
    {
        var caixaId = Guid.NewGuid();
        var custodiaId = Guid.NewGuid();
        var caixa = new ContaFinanceira(UserId, "Corrente", TipoContaFinanceira.Corrente, MoedaPatrimonio.BRL, 15_000m);
        typeof(ContaFinanceira).GetProperty(nameof(ContaFinanceira.Id))!.SetValue(caixa, caixaId);
        var custodia = new ContaFinanceira(UserId, "Custódia USD", TipoContaFinanceira.InvestimentoCustodia, MoedaPatrimonio.USD, 999m);
        typeof(ContaFinanceira).GetProperty(nameof(ContaFinanceira.Id))!.SetValue(custodia, custodiaId);
        _repo.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<ContaFinanceira> { caixa, custodia });

        // 2 investimentos em USD ligados à custódia: (100 + 200) * 5 (câmbio) = 1500 BRL.
        var i1 = new Investimento(UserId, "AAPL", TipoInvestimento.Exterior, MoedaPatrimonio.USD, null, "AAPL", 0m, 100m, contaId: custodiaId);
        var i2 = new Investimento(UserId, "MSFT", TipoInvestimento.Exterior, MoedaPatrimonio.USD, null, "MSFT", 0m, 200m, contaId: custodiaId);
        _invRepo.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync(new[] { i1, i2 });
        _estruturaRepo.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<Estrutura>());

        var h = new GetContasQueryHandler(_repo.Object, _invRepo.Object, _estruturaRepo.Object, _fx.Object, _user.Object);
        var r = await h.Handle(new GetContasQuery(), CancellationToken.None);

        r.Contas.Single(c => c.Id == caixaId).ValorBRL.Should().Be(15_000m);          // saldo manual
        var cust = r.Contas.Single(c => c.Id == custodiaId);
        cust.ValorBRL.Should().Be(1_500m);                                            // (100+200)×5
        cust.QtdInvestimentos.Should().Be(2);
        cust.AgregaInvestimentos.Should().BeTrue();
        r.TotalBRL.Should().Be(16_500m);
    }
}
