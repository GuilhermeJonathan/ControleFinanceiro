using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Patrimonio.Commands.SaveAlocacaoAlvo;
using ControleFinanceiro.Application.Patrimonio.Queries.GetRebalanceamento;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Patrimonio;

public class AlocacaoAlvoHandlersTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private readonly Mock<IInvestimentoRepository> _invRepo = new();
    private readonly Mock<IAlocacaoAlvoRepository> _alvoRepo = new();
    private readonly Mock<IMoedaParamRepository> _moedaRepo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<ICurrentUser> _currentUser = new();

    public AlocacaoAlvoHandlersTests()
    {
        _currentUser.Setup(c => c.UserId).Returns(UserId);
        _moedaRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MoedaParam> { new(1, "BRL", "Real", 1, true, 1m) });
    }

    private static Investimento Inv(TipoInvestimento tipo, decimal valorAtual) =>
        new(UserId, "Inv", tipo, MoedaPatrimonio.BRL, null, null, valorAtual, valorAtual);

    [Fact]
    public async Task Rebalanceamento_CalculaAtualPctEDesvio()
    {
        // 60k Ações + 40k Renda Fixa = 100k total; alvo Ações 50%, RF 50%.
        _invRepo.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { Inv(TipoInvestimento.Acoes, 60000m), Inv(TipoInvestimento.RendaFixa, 40000m) });
        _alvoRepo.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                AlocacaoAlvo.Criar(UserId, TipoInvestimento.Acoes, 50m),
                AlocacaoAlvo.Criar(UserId, TipoInvestimento.RendaFixa, 50m),
            });

        var handler = new GetRebalanceamentoQueryHandler(_invRepo.Object, _alvoRepo.Object, _moedaRepo.Object, _currentUser.Object);
        var result = await handler.Handle(new GetRebalanceamentoQuery(), CancellationToken.None);

        result.TotalBRL.Should().Be(100000m);
        result.TemAlvo.Should().BeTrue();
        var acoes = result.Classes.Single(c => c.Tipo == (int)TipoInvestimento.Acoes);
        acoes.AtualPct.Should().Be(60m);
        acoes.DesvioPct.Should().Be(10m);   // 60% atual - 50% alvo → 10% acima
        var rf = result.Classes.Single(c => c.Tipo == (int)TipoInvestimento.RendaFixa);
        rf.DesvioPct.Should().Be(-10m);      // 40% - 50% → 10% abaixo
    }

    [Fact]
    public async Task Save_SubstituiAlvosEPersiste()
    {
        var handler = new SaveAlocacaoAlvoCommandHandler(_alvoRepo.Object, _currentUser.Object, _uow.Object);

        await handler.Handle(new SaveAlocacaoAlvoCommand(new[]
        {
            new AlvoItem((int)TipoInvestimento.Acoes, 60m),
            new AlvoItem((int)TipoInvestimento.RendaFixa, 40m),
            new AlvoItem((int)TipoInvestimento.FII, 0m),   // ignorado (0%)
        }), CancellationToken.None);

        _alvoRepo.Verify(r => r.RemoveByUsuarioAsync(UserId, It.IsAny<CancellationToken>()), Times.Once);
        _alvoRepo.Verify(r => r.AddRangeAsync(
            It.Is<IEnumerable<AlocacaoAlvo>>(l => l.Count() == 2), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
