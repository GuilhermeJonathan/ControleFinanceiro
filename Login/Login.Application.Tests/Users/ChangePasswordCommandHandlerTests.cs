using Login.Application.Common.Interfaces;
using Login.Application.Users.Commands.ChangePassword;
using Login.Domain.Common;
using Login.Domain.Entities;
using Login.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace Login.Application.Tests.Users;

public class ChangePasswordCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<ICryptography> _cryptoMock = new();
    private readonly Mock<IUserAccessor> _userAccessorMock = new();
    private readonly ChangePasswordCommandHandler _handler;

    private static readonly Guid _userId = Guid.NewGuid();

    public ChangePasswordCommandHandlerTests()
    {
        _userAccessorMock.Setup(a => a.UserId).Returns(_userId);
        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new ChangePasswordCommandHandler(
            _userRepoMock.Object,
            _uowMock.Object,
            _cryptoMock.Object,
            _userAccessorMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCurrentPassword_ShouldChangePasswordAndSave()
    {
        // Arrange
        var user = new User(_userId, "João", "joao@example.com", "12345678900", "old_hash", UserType.User);
        _userRepoMock
            .Setup(r => r.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _cryptoMock
            .Setup(c => c.VerifyAsync("oldpassword", "old_hash"))
            .ReturnsAsync(true);
        _cryptoMock
            .Setup(c => c.Hash("newpassword123"))
            .Returns("new_hash");

        var command = new ChangePasswordCommand("oldpassword", "newpassword123");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        user.PasswordHash.Should().Be("new_hash");
        _userRepoMock.Verify(r => r.Update(user), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WrongCurrentPassword_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var user = new User(_userId, "João", "joao@example.com", "12345678900", "old_hash", UserType.User);
        _userRepoMock
            .Setup(r => r.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _cryptoMock
            .Setup(c => c.VerifyAsync("wrongpassword", "old_hash"))
            .ReturnsAsync(false);

        var command = new ChangePasswordCommand("wrongpassword", "newpassword123");

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NewPasswordTooShort_ShouldThrowArgumentException()
    {
        // Arrange
        var user = new User(_userId, "João", "joao@example.com", "12345678900", "old_hash", UserType.User);
        _userRepoMock
            .Setup(r => r.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _cryptoMock
            .Setup(c => c.VerifyAsync("oldpassword", "old_hash"))
            .ReturnsAsync(true);

        var command = new ChangePasswordCommand("oldpassword", "123");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        _userRepoMock
            .Setup(r => r.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new ChangePasswordCommand("oldpassword", "newpassword123");

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ValidChange_ShouldRevokeTokensToForceReLogin()
    {
        // Arrange
        var user = new User(_userId, "João", "joao@example.com", "12345678900", "old_hash", UserType.User);
        _userRepoMock
            .Setup(r => r.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _cryptoMock
            .Setup(c => c.VerifyAsync("oldpassword", "old_hash"))
            .ReturnsAsync(true);
        _cryptoMock
            .Setup(c => c.Hash(It.IsAny<string>()))
            .Returns("new_hash");

        var command = new ChangePasswordCommand("oldpassword", "newpassword123");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        user.TokenRevokedAt.Should().NotBeNull();
    }
}
