using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Imoveis.Commands.RemoveFoto;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Imoveis;

public class RemoveFotoCommandHandlerTests
{
    private readonly Mock<IImovelRepository> _repoMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly RemoveFotoCommandHandler _handler;

    private static readonly Guid _userId = Guid.NewGuid();

    public RemoveFotoCommandHandlerTests()
    {
        _currentUserMock.Setup(u => u.UserId).Returns(_userId);
        _currentUserMock.Setup(u => u.PodeVerImoveis).Returns(false);
        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new RemoveFotoCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingFoto_ShouldDeleteAndSave()
    {
        var fotoId = Guid.NewGuid();
        var foto = new ImovelFoto(Guid.NewGuid(), "data:image/png;base64,XYZ", 0);
        _repoMock
            .Setup(r => r.GetFotoAsync(fotoId, _userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(foto);

        await _handler.Handle(new RemoveFotoCommand(fotoId), CancellationToken.None);

        _repoMock.Verify(r => r.DeleteFoto(foto), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistingFoto_ShouldThrowKeyNotFoundException()
    {
        var fotoId = Guid.NewGuid();
        _repoMock
            .Setup(r => r.GetFotoAsync(fotoId, _userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ImovelFoto?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(new RemoveFotoCommand(fotoId), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NonExistingFoto_ShouldNotCallDeleteOrSave()
    {
        var fotoId = Guid.NewGuid();
        _repoMock
            .Setup(r => r.GetFotoAsync(fotoId, _userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ImovelFoto?)null);

        try { await _handler.Handle(new RemoveFotoCommand(fotoId), CancellationToken.None); } catch { }

        _repoMock.Verify(r => r.DeleteFoto(It.IsAny<ImovelFoto>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}


