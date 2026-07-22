using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Parametros.Commands;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Patrimonio;

public class SubtipoInvestimentoHandlersTests
{
    private readonly Mock<ISubtipoInvestimentoParamRepository> _repo = new();
    private readonly Mock<ICurrentUser> _user = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    [Fact]
    public async Task Save_Admin_Cria()
    {
        _user.Setup(u => u.IsAdmin).Returns(true);
        var h = new SaveSubtipoInvestimentoCommandHandler(_repo.Object, _user.Object, _uow.Object);

        await h.Handle(new SaveSubtipoInvestimentoCommand(null, 4, "High Yield", 7, true), CancellationToken.None);

        _repo.Verify(r => r.AddAsync(It.Is<SubtipoInvestimentoParam>(s => s.Nome == "High Yield" && s.TipoInvestimentoId == 4), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Save_NaoAdmin_Falha()
    {
        _user.Setup(u => u.IsAdmin).Returns(false);
        var h = new SaveSubtipoInvestimentoCommandHandler(_repo.Object, _user.Object, _uow.Object);

        var act = () => h.Handle(new SaveSubtipoInvestimentoCommand(null, 4, "X", 1, true), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        _repo.Verify(r => r.AddAsync(It.IsAny<SubtipoInvestimentoParam>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Delete_System_Falha()
    {
        _user.Setup(u => u.IsAdmin).Returns(true);
        _repo.Setup(r => r.GetByIdAsync(30, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SubtipoInvestimentoParam(30, 4, "IPCA+", 1, true)); // system
        var h = new DeleteSubtipoInvestimentoCommandHandler(_repo.Object, _user.Object, _uow.Object);

        var act = () => h.Handle(new DeleteSubtipoInvestimentoCommand(30), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        _repo.Verify(r => r.Remove(It.IsAny<SubtipoInvestimentoParam>()), Times.Never);
    }

    [Fact]
    public async Task Delete_Custom_Remove()
    {
        _user.Setup(u => u.IsAdmin).Returns(true);
        var custom = new SubtipoInvestimentoParam(4, "Custom", 9); // não-system
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(custom);
        var h = new DeleteSubtipoInvestimentoCommandHandler(_repo.Object, _user.Object, _uow.Object);

        await h.Handle(new DeleteSubtipoInvestimentoCommand(1234), CancellationToken.None);

        _repo.Verify(r => r.Remove(custom), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
