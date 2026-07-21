using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Patrimonio.Commands.Estruturas;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Patrimonio;

public class BeneficiariosHandlersTests
{
    private readonly Mock<IEstruturaRepository> _repo = new();
    private readonly Mock<ICurrentUser> _user = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid EstId = Guid.NewGuid();

    public BeneficiariosHandlersTests()
    {
        _user.Setup(u => u.UserId).Returns(UserId);
        var e = new Estrutura(UserId, "Trust", TipoEstrutura.Trust);
        typeof(Estrutura).GetProperty(nameof(Estrutura.Id))!.SetValue(e, EstId);
        _repo.Setup(r => r.GetByIdAsync(EstId, It.IsAny<CancellationToken>())).ReturnsAsync(e);
    }

    [Fact]
    public async Task SaveBeneficiario_Cria()
    {
        Beneficiario? cap = null;
        _repo.Setup(r => r.AddBeneficiarioAsync(It.IsAny<Beneficiario>(), It.IsAny<CancellationToken>()))
            .Callback<Beneficiario, CancellationToken>((b, _) => cap = b);

        var h = new SaveBeneficiarioCommandHandler(_repo.Object, _user.Object, _uow.Object);
        await h.Handle(new SaveBeneficiarioCommand(null, "Cônjuge", PapelBeneficiario.Conjuge, 20m, "aos 25 anos"), CancellationToken.None);

        cap!.Nome.Should().Be("Cônjuge");
        cap.UsuarioId.Should().Be(UserId);
        cap.PercentualDistribuicao.Should().Be(20m);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SaveBeneficiario_PctInvalido_ShouldThrow()
    {
        var h = new SaveBeneficiarioCommandHandler(_repo.Object, _user.Object, _uow.Object);
        var act = () => h.Handle(new SaveBeneficiarioCommand(null, "X", PapelBeneficiario.Filho, 150m, null), CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SaveDistribuicao_ValorInvalido_ShouldThrow()
    {
        var h = new SaveDistribuicaoCommandHandler(_repo.Object, _user.Object, _uow.Object);
        var act = () => h.Handle(new SaveDistribuicaoCommand(null, DateTime.UtcNow, 0m, MoedaPatrimonio.BRL, null, null, null), CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SaveDistribuicao_ComEstruturaDeOrigem_Cria()
    {
        Distribuicao? cap = null;
        _repo.Setup(r => r.AddDistribuicaoAsync(It.IsAny<Distribuicao>(), It.IsAny<CancellationToken>()))
            .Callback<Distribuicao, CancellationToken>((d, _) => cap = d);

        var h = new SaveDistribuicaoCommandHandler(_repo.Object, _user.Object, _uow.Object);
        await h.Handle(new SaveDistribuicaoCommand(null, new DateTime(2025, 1, 10), 480000m, MoedaPatrimonio.USD, EstId, null, "Distribuição anual"), CancellationToken.None);

        cap!.Valor.Should().Be(480000m);
        cap.Moeda.Should().Be(MoedaPatrimonio.USD);
        cap.EstruturaId.Should().Be(EstId);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
