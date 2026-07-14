using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Patrimonio.Commands.CreateInvestimento;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Patrimonio;

public class CreateInvestimentoCommandHandlerTests
{
    private readonly Mock<IInvestimentoRepository> _repoMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private static readonly Guid UserId = Guid.NewGuid();

    public CreateInvestimentoCommandHandlerTests()
    {
        _currentUserMock.Setup(c => c.UserId).Returns(UserId);
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldAddInvestimentoAndReturnId()
    {
        var handler = new CreateInvestimentoCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);
        var command = new CreateInvestimentoCommand("ETF S&P500", TipoInvestimento.ETF, MoedaPatrimonio.USD,
            "XP", "IVVB11", 50_000m, 55_000m, 12m);

        var id = await handler.Handle(command, CancellationToken.None);

        id.Should().NotBeEmpty();
        _repoMock.Verify(r => r.AddAsync(It.Is<Investimento>(i =>
            i.Nome == "ETF S&P500" &&
            i.Tipo == TipoInvestimento.ETF &&
            i.Moeda == MoedaPatrimonio.USD &&
            i.ValorAplicado == 50_000m &&
            i.ValorAtual == 55_000m &&
            i.UsuarioId == UserId), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_RepositoryError_ShouldNotSave()
    {
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Investimento>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("db error"));

        var handler = new CreateInvestimentoCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);
        var command = new CreateInvestimentoCommand("X", TipoInvestimento.Outro, MoedaPatrimonio.BRL,
            null, null, 100m, 100m, null);

        await handler.Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>();

        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
