using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Imoveis.Commands.AddComentario;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Imoveis;

public class AddComentarioCommandHandlerTests
{
    private readonly Mock<IImovelRepository> _repoMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly AddComentarioCommandHandler _handler;

    private static readonly Guid _userId = Guid.NewGuid();

    public AddComentarioCommandHandlerTests()
    {
        _currentUserMock.Setup(u => u.UserId).Returns(_userId);
        _currentUserMock.Setup(u => u.PodeVerImoveis).Returns(false);
        _repoMock
            .Setup(r => r.AddComentarioAsync(It.IsAny<ImovelComentario>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new AddComentarioCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingImovel_ShouldAddComentarioAndReturnId()
    {
        var imovelId = Guid.NewGuid();
        var imovel = new Imovel("Casa", 400_000m, [], [], 6,
            new DateTime(2026, 4, 1), null, null, null, null, _userId);
        _repoMock
            .Setup(r => r.GetByIdAsync(imovelId, _userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(imovel);

        var comentarioId = await _handler.Handle(new AddComentarioCommand(imovelId, "Boa localizaÃ§Ã£o"), CancellationToken.None);

        comentarioId.Should().NotBeEmpty();
        _repoMock.Verify(r => r.AddComentarioAsync(It.IsAny<ImovelComentario>(), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistingImovel_ShouldThrowKeyNotFoundException()
    {
        var imovelId = Guid.NewGuid();
        _repoMock
            .Setup(r => r.GetByIdAsync(imovelId, _userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Imovel?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(new AddComentarioCommand(imovelId, "Texto"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NonExistingImovel_ShouldNotAddComentarioOrSave()
    {
        var imovelId = Guid.NewGuid();
        _repoMock
            .Setup(r => r.GetByIdAsync(imovelId, _userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Imovel?)null);

        try { await _handler.Handle(new AddComentarioCommand(imovelId, "Texto"), CancellationToken.None); } catch { }

        _repoMock.Verify(r => r.AddComentarioAsync(It.IsAny<ImovelComentario>(), It.IsAny<CancellationToken>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}


