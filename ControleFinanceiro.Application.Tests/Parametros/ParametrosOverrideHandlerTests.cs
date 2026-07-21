using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Parametros.Commands;
using ControleFinanceiro.Application.Parametros.Queries;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Parametros;

public class ParametrosOverrideHandlerTests
{
    private readonly Mock<ITipoAtivoParamRepository> _ativoRepo = new();
    private readonly Mock<ITipoInvestimentoParamRepository> _investRepo = new();
    private readonly Mock<IParametroOcultoRepository> _ocultoRepo = new();
    private readonly Mock<IAssessoriaOwnerResolver> _resolver = new();
    private readonly Mock<ICurrentUser> _user = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private static readonly Guid Assessor = Guid.NewGuid();

    private void ComoAdmin()   { _user.Setup(u => u.IsAdmin).Returns(true);  _user.Setup(u => u.IsAssessor).Returns(true);  _user.Setup(u => u.RealUserId).Returns(Guid.NewGuid()); }
    private void ComoAssessor() { _user.Setup(u => u.IsAdmin).Returns(false); _user.Setup(u => u.IsAssessor).Returns(true);  _user.Setup(u => u.RealUserId).Returns(Assessor); }
    private void ComoCliente()  { _user.Setup(u => u.IsAdmin).Returns(false); _user.Setup(u => u.IsAssessor).Returns(false); _user.Setup(u => u.IsCorretor).Returns(false); }

    // ── SaveTipoAtivo ─────────────────────────────────────────────────────

    [Fact]
    public async Task SaveTipoAtivo_Admin_CriaGlobal()
    {
        ComoAdmin();
        TipoAtivoParam? capturado = null;
        _ativoRepo.Setup(r => r.AddAsync(It.IsAny<TipoAtivoParam>(), It.IsAny<CancellationToken>()))
            .Callback<TipoAtivoParam, CancellationToken>((e, _) => capturado = e);

        var h = new SaveTipoAtivoCommandHandler(_ativoRepo.Object, _user.Object, _uow.Object);
        await h.Handle(new SaveTipoAtivoCommand(null, "Cripto Cold", "🧊", 9, true), CancellationToken.None);

        capturado!.AssessorId.Should().BeNull();
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SaveTipoAtivo_Assessor_CriaCustomComAssessorId()
    {
        ComoAssessor();
        TipoAtivoParam? capturado = null;
        _ativoRepo.Setup(r => r.AddAsync(It.IsAny<TipoAtivoParam>(), It.IsAny<CancellationToken>()))
            .Callback<TipoAtivoParam, CancellationToken>((e, _) => capturado = e);

        var h = new SaveTipoAtivoCommandHandler(_ativoRepo.Object, _user.Object, _uow.Object);
        await h.Handle(new SaveTipoAtivoCommand(null, "Obras de arte", "🎨", 20, true), CancellationToken.None);

        capturado!.AssessorId.Should().Be(Assessor);
    }

    [Fact]
    public async Task SaveTipoAtivo_Assessor_NaoEditaGlobal()
    {
        ComoAssessor();
        var global = new TipoAtivoParam(1, "Imóvel", 1, true); // AssessorId null
        _ativoRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(global);

        var h = new SaveTipoAtivoCommandHandler(_ativoRepo.Object, _user.Object, _uow.Object);
        var act = () => h.Handle(new SaveTipoAtivoCommand(1, "Hackeado", null, 1, true), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── DeleteTipoAtivo ───────────────────────────────────────────────────

    [Fact]
    public async Task DeleteTipoAtivo_Assessor_NaoExcluiGlobal()
    {
        ComoAssessor();
        var global = new TipoAtivoParam("Imóvel", 1); // AssessorId null
        _ativoRepo.Setup(r => r.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(global);

        var h = new DeleteTipoAtivoCommandHandler(_ativoRepo.Object, _user.Object, _uow.Object);
        var act = () => h.Handle(new DeleteTipoAtivoCommand(3), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        _ativoRepo.Verify(r => r.Remove(It.IsAny<TipoAtivoParam>()), Times.Never);
    }

    [Fact]
    public async Task DeleteTipoAtivo_Assessor_ExcluiProprioCustom()
    {
        ComoAssessor();
        var custom = new TipoAtivoParam("Obras", 20, null, Assessor);
        _ativoRepo.Setup(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(custom);

        var h = new DeleteTipoAtivoCommandHandler(_ativoRepo.Object, _user.Object, _uow.Object);
        await h.Handle(new DeleteTipoAtivoCommand(7), CancellationToken.None);

        _ativoRepo.Verify(r => r.Remove(custom), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── Moeda: somente admin ──────────────────────────────────────────────

    [Fact]
    public async Task SaveMoeda_Assessor_Proibido()
    {
        ComoAssessor();
        var h = new SaveMoedaCommandHandler(new Mock<IMoedaParamRepository>().Object, _user.Object, _uow.Object);
        var act = () => h.Handle(new SaveMoedaCommand(null, "JPY", "Iene", 0.03m, 5, true), CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task SaveMoeda_Admin_Cria()
    {
        ComoAdmin();
        var repo = new Mock<IMoedaParamRepository>();
        var h = new SaveMoedaCommandHandler(repo.Object, _user.Object, _uow.Object);
        await h.Handle(new SaveMoedaCommand(null, "JPY", "Iene", 0.03m, 5, true), CancellationToken.None);
        repo.Verify(r => r.AddAsync(It.IsAny<MoedaParam>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── Ocultar / Reexibir ────────────────────────────────────────────────

    [Fact]
    public async Task Ocultar_Assessor_OcultaGlobal()
    {
        ComoAssessor();
        _ativoRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new TipoAtivoParam(1, "Imóvel", 1, true));
        _ocultoRepo.Setup(r => r.GetAsync(Assessor, TipoParametroCatalogo.TipoAtivo, 1, It.IsAny<CancellationToken>())).ReturnsAsync((ParametroOculto?)null);

        var h = new OcultarParametroCommandHandler(_ocultoRepo.Object, _ativoRepo.Object, _investRepo.Object, _user.Object, _uow.Object);
        await h.Handle(new OcultarParametroCommand(TipoParametroCatalogo.TipoAtivo, 1), CancellationToken.None);

        _ocultoRepo.Verify(r => r.AddAsync(It.Is<ParametroOculto>(p => p.AssessorId == Assessor && p.ParametroId == 1), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Ocultar_NaoPermiteCustom()
    {
        ComoAssessor();
        _ativoRepo.Setup(r => r.GetByIdAsync(9, It.IsAny<CancellationToken>())).ReturnsAsync(new TipoAtivoParam("Custom", 9, null, Assessor));

        var h = new OcultarParametroCommandHandler(_ocultoRepo.Object, _ativoRepo.Object, _investRepo.Object, _user.Object, _uow.Object);
        var act = () => h.Handle(new OcultarParametroCommand(TipoParametroCatalogo.TipoAtivo, 9), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Ocultar_Admin_Proibido()
    {
        ComoAdmin();
        var h = new OcultarParametroCommandHandler(_ocultoRepo.Object, _ativoRepo.Object, _investRepo.Object, _user.Object, _uow.Object);
        var act = () => h.Handle(new OcultarParametroCommand(TipoParametroCatalogo.TipoAtivo, 1), CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Reexibir_RemoveOculto()
    {
        ComoAssessor();
        var oc = new ParametroOculto(Assessor, TipoParametroCatalogo.TipoAtivo, 1);
        _ocultoRepo.Setup(r => r.GetAsync(Assessor, TipoParametroCatalogo.TipoAtivo, 1, It.IsAny<CancellationToken>())).ReturnsAsync(oc);

        var h = new ReexibirParametroCommandHandler(_ocultoRepo.Object, _user.Object, _uow.Object);
        await h.Handle(new ReexibirParametroCommand(TipoParametroCatalogo.TipoAtivo, 1), CancellationToken.None);

        _ocultoRepo.Verify(r => r.Remove(oc), Times.Once);
    }

    // ── GetTiposAtivo role-aware ──────────────────────────────────────────

    [Fact]
    public async Task GetTiposAtivo_Admin_SoGlobaisPodeEditar()
    {
        ComoAdmin();
        _ativoRepo.Setup(r => r.GetGlobaisAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TipoAtivoParam> { new(1, "Imóvel", 1, true) });

        var h = new GetTiposAtivoQueryHandler(_ativoRepo.Object, _ocultoRepo.Object, _resolver.Object, _user.Object);
        var res = await h.Handle(new GetTiposAtivoQuery(), CancellationToken.None);

        res.Should().HaveCount(1);
        res[0].PodeEditar.Should().BeTrue();
        res[0].AssessorId.Should().BeNull();
    }

    [Fact]
    public async Task GetTiposAtivo_Assessor_MarcaOcultoEEditavelNoCustom()
    {
        ComoAssessor();
        _resolver.Setup(r => r.ResolveOwnerAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Assessor);
        _ativoRepo.Setup(r => r.GetGlobaisEDoAssessorAsync(Assessor, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TipoAtivoParam>
            {
                new(1, "Imóvel", 1, true),                 // global
                new(2, "Veículo", 2, true),                // global (será ocultado)
                new("Obras", 20, null, Assessor),          // custom
            });
        _ocultoRepo.Setup(r => r.GetIdsOcultosAsync(Assessor, TipoParametroCatalogo.TipoAtivo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<int> { 2 });

        var h = new GetTiposAtivoQueryHandler(_ativoRepo.Object, _ocultoRepo.Object, _resolver.Object, _user.Object);
        var res = await h.Handle(new GetTiposAtivoQuery(), CancellationToken.None);

        res.Should().HaveCount(3); // dono vê tudo, inclusive ocultos
        res.Single(x => x.Id == 2).Oculto.Should().BeTrue();
        res.Single(x => x.Nome == "Obras").PodeEditar.Should().BeTrue();
        res.Single(x => x.Id == 1).PodeEditar.Should().BeFalse();
    }

    [Fact]
    public async Task GetTiposAtivo_Cliente_EscondeOcultosSemEdicao()
    {
        ComoCliente();
        _resolver.Setup(r => r.ResolveOwnerAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Assessor);
        _ativoRepo.Setup(r => r.GetGlobaisEDoAssessorAsync(Assessor, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TipoAtivoParam>
            {
                new(1, "Imóvel", 1, true),
                new(2, "Veículo", 2, true),
                new("Obras", 20, null, Assessor),
            });
        _ocultoRepo.Setup(r => r.GetIdsOcultosAsync(Assessor, TipoParametroCatalogo.TipoAtivo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<int> { 2 });

        var h = new GetTiposAtivoQueryHandler(_ativoRepo.Object, _ocultoRepo.Object, _resolver.Object, _user.Object);
        var res = await h.Handle(new GetTiposAtivoQuery(), CancellationToken.None);

        res.Should().HaveCount(2); // oculto (id 2) removido
        res.Should().OnlyContain(x => !x.PodeEditar);
        res.Should().NotContain(x => x.Id == 2);
    }
}
