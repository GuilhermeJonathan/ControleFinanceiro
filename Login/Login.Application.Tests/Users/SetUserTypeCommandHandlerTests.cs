using Login.Application.Users.Commands.SetUserType;
using Login.Domain.Common;
using Login.Domain.Entities;
using Login.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace Login.Application.Tests.Users;

public class SetUserTypeCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly SetUserTypeCommandHandler _handler;

    private static readonly Guid UserId = Guid.NewGuid();

    private static User BuildUser() =>
        new User(UserId, "Carlos Souza", "carlos@example.com", "12345678900", "hash", UserType.User);

    public SetUserTypeCommandHandlerTests()
    {
        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new SetUserTypeCommandHandler(_userRepoMock.Object, _uowMock.Object);
    }

    [Fact]
    public async Task Handle_PromoteToAssessor_ShouldUpdateTypeAndSave()
    {
        // Arrange
        var user = BuildUser();
        _userRepoMock.Setup(r => r.GetByIdAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var command = new SetUserTypeCommand(UserId, (int)UserType.Assessor);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        user.UserTypeId.Should().Be(UserType.Assessor);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidUserType_ShouldThrowArgumentException()
    {
        // Arrange
        var command = new SetUserTypeCommand(UserId, 99);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_InvalidUserType_ShouldNotCallSaveChanges()
    {
        // Arrange
        var command = new SetUserTypeCommand(UserId, 99);

        // Act
        try { await _handler.Handle(command, CancellationToken.None); } catch { }

        // Assert
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByIdAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var command = new SetUserTypeCommand(UserId, (int)UserType.Assessor);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
