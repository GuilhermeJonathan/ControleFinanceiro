using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Imoveis.Commands.DeleteImovel;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace ControleFinanceiro.Application.Tests.Imoveis;

public class DeleteImovelCommandHandlerTests
{
    private readonly Mock<IImovelRepository> _repoMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly DeleteImovelCommandHandler _handler;

    private static readonly Guid _userId = Guid.NewGuid();

    public DeleteImovelCommandHandlerTests()
    {
        _currentUserMock.Setup(u => u.UserId).Returns(_userId);
        _currentUserMock.Setup(u => u.PodeVerImoveis).Returns(false);
        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new DeleteImovelCommandHandler(_repoMock.Object, _currentUserMock.Object, _uowMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingImovel_ShouldDeleteAndSave()
    {
        var imovelId = Guid.NewGuid();
        var imovel = new Imovel("Apto", 300_000m, [], [], 7,
            new DateTime(2026, 4, 1), null, null, null, null, _userId);
        _repoMock
            .Setup(r => r.GetByIdAsync(imovelId, _userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(imovel);

        await _handler.Handle(new DeleteImovelCommand(imovelId), CancellationToken.None);

        _repoMock.Verify(r => r.Delete(imovel), Times.Once);
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
            _handler.Handle(new DeleteImovelCommand(imovelId), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NonExistingImovel_ShouldNotCallDeleteOrSave()
    {
        var imovelId = Guid.NewGuid();
        _repoMock
            .Setup(r => r.GetByIdAsync(imovelId, _userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Imovel?)null);

        try { await _handler.Handle(new DeleteImovelCommand(imovelId), CancellationToken.None); } catch { }

        _repoMock.Verify(r => r.Delete(It.IsAny<Imovel>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}


