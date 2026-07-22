using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Patrimonio.Queries.GetContas;
using ControleFinanceiro.Application.Patrimonio.Queries.GetEstruturas;
using ControleFinanceiro.Application.Patrimonio.Queries.GetPlanoAcao;
using ControleFinanceiro.Application.Patrimonio.Queries.GetSucessao;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using MediatR;
using Moq;

namespace ControleFinanceiro.Application.Tests.Patrimonio;

public class IndicadoresSucessaoHandlersTests
{
    private readonly Mock<IIndicadoresSucessaoRepository> _repo = new();
    private readonly Mock<IMediator> _mediator = new();
    private readonly Mock<ICurrentUser> _user = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private static readonly Guid UserId = Guid.NewGuid();

    public IndicadoresSucessaoHandlersTests()
    {
        _user.Setup(u => u.UserId).Returns(UserId);
        _mediator.Setup(m => m.Send(It.IsAny<GetEstruturasQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new GrafoEstruturasDto());
        _mediator.Setup(m => m.Send(It.IsAny<GetSucessaoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new SucessaoDto());
        _mediator.Setup(m => m.Send(It.IsAny<GetContasQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ContasResultDto([], 0m));
        _mediator.Setup(m => m.Send(It.IsAny<GetPlanosAcaoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(Enumerable.Empty<PlanoAcaoDto>());
    }

    [Fact]
    public async Task Get_SemRegistro_UsaCalculado()
    {
        _repo.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync((IndicadoresSucessao?)null);
        var h = new GetIndicadoresSucessaoQueryHandler(_repo.Object, _mediator.Object, _user.Object);

        var r = await h.Handle(new GetIndicadoresSucessaoQuery(), CancellationToken.None);

        r.GovernancaOverride.Should().BeNull();
        r.GovernancaScore.Should().Be(r.GovernancaCalculado); // sem override → usa calculado
    }

    [Fact]
    public async Task Get_ComOverride_UsaOverride()
    {
        _repo.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync(new IndicadoresSucessao(UserId, 88, 77));
        var h = new GetIndicadoresSucessaoQueryHandler(_repo.Object, _mediator.Object, _user.Object);

        var r = await h.Handle(new GetIndicadoresSucessaoQuery(), CancellationToken.None);

        r.GovernancaScore.Should().Be(88);
        r.ConformidadeScore.Should().Be(77);
        r.GovernancaOverride.Should().Be(88);
    }

    [Fact]
    public async Task Save_SemRegistro_Cria()
    {
        _repo.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync((IndicadoresSucessao?)null);
        var h = new SaveIndicadoresSucessaoCommandHandler(_repo.Object, _user.Object, _uow.Object);

        await h.Handle(new SaveIndicadoresSucessaoCommand(90, 120), CancellationToken.None);

        _repo.Verify(r => r.AddAsync(It.Is<IndicadoresSucessao>(i => i.GovernancaScore == 90 && i.ConformidadeScore == 100), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Save_ComRegistro_Atualiza()
    {
        var existente = new IndicadoresSucessao(UserId, 10, 10);
        _repo.Setup(r => r.GetByUsuarioAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync(existente);
        var h = new SaveIndicadoresSucessaoCommandHandler(_repo.Object, _user.Object, _uow.Object);

        await h.Handle(new SaveIndicadoresSucessaoCommand(88, 95), CancellationToken.None);

        existente.GovernancaScore.Should().Be(88);
        existente.ConformidadeScore.Should().Be(95);
        _repo.Verify(r => r.AddAsync(It.IsAny<IndicadoresSucessao>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
