using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Patrimonio.Commands.CreateAtivo;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Patrimonio;

public class CreateAtivoPatrimonialCommandHandlerTests
{
    private readonly Mock<IAtivoPatrimonialRepository> _repoMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private static readonly Guid UserId = Guid.NewGuid();

    public CreateAtivoPatrimonialCommandHandlerTests()
    {
        _currentUserMock.Setup(c => c.UserId).Returns(UserId);
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldAddAtivoAndReturnId()
    {
        var handler = new CreateAtivoPatrimonialCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);
        var command = new CreateAtivoPatrimonialCommand("Apartamento SP", TipoAtivo.Imovel, MoedaPatrimonio.BRL, 1_500_000m, 8m);

        var id = await handler.Handle(command, CancellationToken.None);

        id.Should().NotBeEmpty();
        _repoMock.Verify(r => r.AddAsync(It.Is<AtivoPatrimonial>(a =>
            a.Nome == "Apartamento SP" &&
            a.Tipo == TipoAtivo.Imovel &&
            a.Moeda == MoedaPatrimonio.BRL &&
            a.ValorAtual == 1_500_000m &&
            a.UsuarioId == UserId), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldNotSaveOnRepositoryError()
    {
        _repoMock.Setup(r => r.AddAsync(It.IsAny<AtivoPatrimonial>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("db error"));

        var handler = new CreateAtivoPatrimonialCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);
        var command = new CreateAtivoPatrimonialCommand("X", TipoAtivo.Outro, MoedaPatrimonio.USD, 100m, null);

        await handler.Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>();

        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
