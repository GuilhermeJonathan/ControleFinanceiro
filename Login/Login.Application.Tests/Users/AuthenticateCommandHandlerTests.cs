using Login.Application.Common.Interfaces;
using Login.Application.Users.Commands.Authenticate;
using Login.Domain.Common;
using Login.Domain.Entities;
using Login.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace Login.Application.Tests.Users;

public class AuthenticateCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepoMock = new();
    private readonly Mock<ICryptography> _cryptoMock = new();
    private readonly Mock<ITokenManager> _tokenManagerMock = new();
    private readonly Mock<IModuleRepository> _moduleRepoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly AuthenticateCommandHandler _handler;

    private static readonly Guid _userId = Guid.NewGuid();

    public AuthenticateCommandHandlerTests()
    {
        _refreshTokenRepoMock
            .Setup(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _tokenManagerMock
            .Setup(t => t.Generate(It.IsAny<User>()))
            .Returns("access_token_value");

        _handler = new AuthenticateCommandHandler(
            _userRepoMock.Object,
            _refreshTokenRepoMock.Object,
            _cryptoMock.Object,
            _tokenManagerMock.Object,
            _moduleRepoMock.Object,
            _uowMock.Object);
    }

    private User BuildActiveUser() =>
        new User(_userId, "João", "joao@example.com", "12345678900", "hashed_pass", UserType.User);

    [Fact]
    public async Task Handle_ValidCredentials_ShouldReturnTokens()
    {
        // Arrange
        var user = BuildActiveUser();
        _userRepoMock
            .Setup(r => r.GetByEmailAsync("joao@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _cryptoMock
            .Setup(c => c.Verify("password123", "hashed_pass"))
            .Returns(true);

        var command = new AuthenticateCommand("joao@example.com", "password123", null, null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.AccessToken.Should().Be("access_token_value");
        result.RefreshToken.Should().NotBeNullOrEmpty();
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        _userRepoMock
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new AuthenticateCommand("notfound@example.com", "password", null, null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WrongPassword_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var user = BuildActiveUser();
        _userRepoMock
            .Setup(r => r.GetByEmailAsync("joao@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _cryptoMock
            .Setup(c => c.Verify("wrongpassword", "hashed_pass"))
            .Returns(false);

        var command = new AuthenticateCommand("joao@example.com", "wrongpassword", null, null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_BlockedUser_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var user = BuildActiveUser();
        user.Block();
        _userRepoMock
            .Setup(r => r.GetByEmailAsync("joao@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _cryptoMock
            .Setup(c => c.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        var command = new AuthenticateCommand("joao@example.com", "password123", null, null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ValidCredentials_ShouldRegisterLoginAndPersistRefreshToken()
    {
        // Arrange
        var user = BuildActiveUser();
        _userRepoMock
            .Setup(r => r.GetByEmailAsync("joao@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _cryptoMock
            .Setup(c => c.Verify("password123", "hashed_pass"))
            .Returns(true);

        var command = new AuthenticateCommand("joao@example.com", "password123", null, null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        user.UltimoLogin.Should().NotBeNull();
        _refreshTokenRepoMock.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
        _userRepoMock.Verify(r => r.Update(user), Times.Once);
    }
}
