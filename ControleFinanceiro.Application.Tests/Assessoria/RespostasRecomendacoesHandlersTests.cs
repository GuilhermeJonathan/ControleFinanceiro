using ControleFinanceiro.Application.Assessoria.Commands.MarcarRespostasVistas;
using ControleFinanceiro.Application.Assessoria.Queries.GetRespostasRecomendacoes;
using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Assessoria;

public class RespostasRecomendacoesHandlersTests
{
    private static readonly Guid AssessorId = Guid.NewGuid();
    private readonly Mock<IRecomendacaoRepository> _repo = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Mock<IUserNameLookup> _lookup = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    public RespostasRecomendacoesHandlersTests()
    {
        _currentUser.Setup(c => c.RealUserId).Returns(AssessorId);
        _lookup.Setup(l => l.GetNomeAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync("Cliente");
    }

    private static Recomendacao Pendente() =>
        new(AssessorId, Guid.NewGuid(), TipoRecomendacao.Dica, "pendente");

    private static Recomendacao Respondida(bool vista)
    {
        var r = new Recomendacao(AssessorId, Guid.NewGuid(), TipoRecomendacao.Dica, "resp");
        r.Responder(StatusRecomendacao.Aceita, "ok");
        if (vista) r.MarcarRespostaVista();
        return r;
    }

    [Fact]
    public async Task Query_ContaNaoVistasEFiltraRespondidas()
    {
        _repo.Setup(r => r.GetByAssessorAsync(AssessorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { Pendente(), Respondida(vista: false), Respondida(vista: false), Respondida(vista: true) });

        var handler = new GetRespostasRecomendacoesQueryHandler(_repo.Object, _currentUser.Object, _lookup.Object);
        var result = await handler.Handle(new GetRespostasRecomendacoesQuery(), CancellationToken.None);

        result.Itens.Should().HaveCount(3);   // só respondidas (exclui a pendente)
        result.NaoVistas.Should().Be(2);      // duas respondidas não vistas
    }

    [Fact]
    public async Task Marcar_MarcaNaoVistasEPersiste()
    {
        var naoVista = Respondida(vista: false);
        _repo.Setup(r => r.GetByAssessorAsync(AssessorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { naoVista, Respondida(vista: true) });

        var handler = new MarcarRespostasVistasCommandHandler(_repo.Object, _currentUser.Object, _uow.Object);
        await handler.Handle(new MarcarRespostasVistasCommand(), CancellationToken.None);

        naoVista.RespostaNaoVista.Should().BeFalse();
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Marcar_SemNaoVistas_NaoPersiste()
    {
        _repo.Setup(r => r.GetByAssessorAsync(AssessorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { Respondida(vista: true), Pendente() });

        var handler = new MarcarRespostasVistasCommandHandler(_repo.Object, _currentUser.Object, _uow.Object);
        await handler.Handle(new MarcarRespostasVistasCommand(), CancellationToken.None);

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
