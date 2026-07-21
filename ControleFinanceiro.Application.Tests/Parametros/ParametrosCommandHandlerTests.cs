using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Parametros.Commands;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Parametros;

public class ParametrosCommandHandlerTests
{
    private readonly Mock<ITipoAtivoParamRepository>        _tipoAtivoRepo   = new();
    private readonly Mock<ITipoInvestimentoParamRepository> _tipoInvRepo     = new();
    private readonly Mock<IMoedaParamRepository>            _moedaRepo       = new();
    private readonly Mock<ICurrentUser>                     _currentUser     = new();
    private readonly Mock<IUnitOfWork>                      _uow             = new();

    public ParametrosCommandHandlerTests()
    {
        // Estes testes exercitam a gestão do catálogo GLOBAL (moedas + tipos com AssessorId=null),
        // que agora pertence ao admin. Assessor/admin ambos passam pelo gate IsAssessor.
        _currentUser.Setup(c => c.IsAssessor).Returns(true);
        _currentUser.Setup(c => c.IsAdmin).Returns(true);
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
    }

    // ── TipoAtivo ─────────────────────────────────────────────────────────

    [Fact]
    public async Task SaveTipoAtivo_Create_ShouldAddAndReturnId()
    {
        var handler = new SaveTipoAtivoCommandHandler(_tipoAtivoRepo.Object, _currentUser.Object, _uow.Object);
        var cmd = new SaveTipoAtivoCommand(null, "Novo Tipo", null, 10, true);

        await handler.Handle(cmd, CancellationToken.None);

        _tipoAtivoRepo.Verify(r => r.AddAsync(It.Is<TipoAtivoParam>(x => x.Nome == "Novo Tipo"), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SaveTipoAtivo_Update_ShouldModifyExisting()
    {
        var existing = new TipoAtivoParam("Antigo", 1);
        _tipoAtivoRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existing);

        var handler = new SaveTipoAtivoCommandHandler(_tipoAtivoRepo.Object, _currentUser.Object, _uow.Object);
        await handler.Handle(new SaveTipoAtivoCommand(1, "Atualizado", null, 2, true), CancellationToken.None);

        existing.Nome.Should().Be("Atualizado");
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SaveTipoAtivo_NaoAssessor_ShouldThrowUnauthorized()
    {
        _currentUser.Setup(c => c.IsAssessor).Returns(false);
        var handler = new SaveTipoAtivoCommandHandler(_tipoAtivoRepo.Object, _currentUser.Object, _uow.Object);

        await handler.Invoking(h => h.Handle(new SaveTipoAtivoCommand(null, "X", null, 1, true), CancellationToken.None))
            .Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task DeleteTipoAtivo_SystemItem_ShouldThrow()
    {
        var system = new TipoAtivoParam(1, "Imóvel", 1, true);
        _tipoAtivoRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(system);

        var handler = new DeleteTipoAtivoCommandHandler(_tipoAtivoRepo.Object, _currentUser.Object, _uow.Object);

        await handler.Invoking(h => h.Handle(new DeleteTipoAtivoCommand(1), CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    // ── Moeda ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task SaveMoeda_Create_ShouldAddAndReturnId()
    {
        var handler = new SaveMoedaCommandHandler(_moedaRepo.Object, _currentUser.Object, _uow.Object);
        await handler.Handle(new SaveMoedaCommand(null, "JPY", "Iene Japones", 0.035m, 6, true), CancellationToken.None);

        _moedaRepo.Verify(r => r.AddAsync(It.Is<MoedaParam>(x => x.Codigo == "JPY"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteMoeda_SystemItem_ShouldThrow()
    {
        var system = new MoedaParam(1, "BRL", "Real Brasileiro", 1, true);
        _moedaRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(system);

        var handler = new DeleteMoedaCommandHandler(_moedaRepo.Object, _currentUser.Object, _uow.Object);

        await handler.Invoking(h => h.Handle(new DeleteMoedaCommand(1), CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    // ── TipoInvestimento ──────────────────────────────────────────────────

    [Fact]
    public async Task SaveTipoInvestimento_Create_ShouldAdd()
    {
        var handler = new SaveTipoInvestimentoCommandHandler(_tipoInvRepo.Object, _currentUser.Object, _uow.Object);
        await handler.Handle(new SaveTipoInvestimentoCommand(null, "Previdencia", null, 8, true), CancellationToken.None);

        _tipoInvRepo.Verify(r => r.AddAsync(It.Is<TipoInvestimentoParam>(x => x.Nome == "Previdencia"), It.IsAny<CancellationToken>()), Times.Once);
    }
}
