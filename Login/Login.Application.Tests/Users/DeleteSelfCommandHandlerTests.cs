using Login.Application.Common.Interfaces;
using Login.Application.Users.Commands.DeleteSelf;
using Login.Domain.Common;
using Login.Domain.Entities;
using Login.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace Login.Application.Tests.Users;

public class DeleteSelfCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IUserAccessor> _userAccessorMock = new();
    private readonly Mock<ITokenManager> _tokenManagerMock = new();
    private readonly DeleteSelfCommandHandler _handler;

    private readonly Guid _userId = Guid.NewGuid();

    public DeleteSelfCommandHandlerTests()
    {
        _userAccessorMock.Setup(a => a.UserId).Returns(_userId);

        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new DeleteSelfCommandHandler(
            _userRepoMock.Object,
            _uowMock.Object,
            _userAccessorMock.Object,
            _tokenManagerMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingUser_ShouldInvalidateTokenRemoveAndSave()
    {
        // Arrange
        var user = new User(_userId, "João", "joao@example.com", "12345678900", "hash", UserType.User);
        _userRepoMock
            .Setup(r => r.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        await _handler.Handle(new DeleteSelfCommand(), CancellationToken.None);

        // Assert
        _tokenManagerMock.Verify(t => t.Invalidate(_userId), Times.Once);
        _userRepoMock.Verify(r => r.Remove(user), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ExistingUser_ShouldInvalidateTokenBeforeRemoving()
    {
        // Arrange
        var callOrder = new List<string>();
        var user = new User(_userId, "João", "joao@example.com", "12345678900", "hash", UserType.User);

        _userRepoMock
            .Setup(r => r.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _tokenManagerMock
            .Setup(t => t.Invalidate(_userId))
            .Callback(() => callOrder.Add("invalidate"));
        _userRepoMock
            .Setup(r => r.Remove(user))
            .Callback(() => callOrder.Add("remove"));

        // Act
        await _handler.Handle(new DeleteSelfCommand(), CancellationToken.None);

        // Assert
        callOrder.Should().ContainInOrder("invalidate", "remove");
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        _userRepoMock
            .Setup(r => r.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(new DeleteSelfCommand(), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldNotInvalidateOrRemove()
    {
        // Arrange
        _userRepoMock
            .Setup(r => r.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        try { await _handler.Handle(new DeleteSelfCommand(), CancellationToken.None); } catch { }

        // Assert
        _tokenManagerMock.Verify(t => t.Invalidate(It.IsAny<Guid>()), Times.Never);
        _userRepoMock.Verify(r => r.Remove(It.IsAny<User>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
