using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Imoveis.Commands.RemoveComentario;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Imoveis;

public class RemoveComentarioCommandHandlerTests
{
    private readonly Mock<IImovelRepository> _repoMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly RemoveComentarioCommandHandler _handler;

    private static readonly Guid _userId = Guid.NewGuid();

    public RemoveComentarioCommandHandlerTests()
    {
        _currentUserMock.Setup(u => u.UserId).Returns(_userId);
        _currentUserMock.Setup(u => u.PodeVerImoveis).Returns(false);
        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new RemoveComentarioCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingComentario_ShouldDeleteAndSave()
    {
        var comentarioId = Guid.NewGuid();
        var imovel = new Imovel("Casa", 400_000m, [], [], 6,
            new DateTime(2026, 4, 1), null, null, null, null, _userId);
        var comentario = new ImovelComentario(imovel.Id, "Bom imÃ³vel");

        _repoMock
            .Setup(r => r.GetComentarioAsync(comentarioId, _userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comentario);

        await _handler.Handle(new RemoveComentarioCommand(comentarioId), CancellationToken.None);

        _repoMock.Verify(r => r.DeleteComentario(comentario), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistingComentario_ShouldThrowKeyNotFoundException()
    {
        var comentarioId = Guid.NewGuid();
        _repoMock
            .Setup(r => r.GetComentarioAsync(comentarioId, _userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ImovelComentario?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(new RemoveComentarioCommand(comentarioId), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NonExistingComentario_ShouldNotDeleteOrSave()
    {
        var comentarioId = Guid.NewGuid();
        _repoMock
            .Setup(r => r.GetComentarioAsync(comentarioId, _userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ImovelComentario?)null);

        try { await _handler.Handle(new RemoveComentarioCommand(comentarioId), CancellationToken.None); } catch { }

        _repoMock.Verify(r => r.DeleteComentario(It.IsAny<ImovelComentario>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}


