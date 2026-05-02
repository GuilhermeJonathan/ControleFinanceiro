using Login.Application.Common.Interfaces;
using Login.Application.Users.Commands.CreateUser;
using Login.Domain.Common;
using Login.Domain.Entities;
using Login.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace Login.Application.Tests.Users;

public class CreateUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<ICryptography> _cryptoMock = new();
    private readonly CreateUserCommandHandler _handler;

    public CreateUserCommandHandlerTests()
    {
        _cryptoMock
            .Setup(c => c.Hash(It.IsAny<string>()))
            .Returns("hashed_password");
        _userRepoMock
            .Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new CreateUserCommandHandler(
            _userRepoMock.Object,
            _uowMock.Object,
            _cryptoMock.Object);
    }

    private static CreateUserCommand BuildCommand(string email = "test@example.com") =>
        new CreateUserCommand(
            UserTypeId: (int)UserType.User,
            Document: "12345678900",
            Name: "João Silva",
            Email: email,
            Address: null,
            Cellphone: null,
            Phone: null,
            Occupation: null,
            ProfileId: null,
            HierarchyId: null,
            FreightForwarderId: null,
            CompanyDocument: null,
            CompanyName: null,
            IsBlocked: false,
            CountryId: null,
            Region: null,
            Restrictions: null);

    [Fact]
    public async Task Handle_NewUser_ShouldCreateAndReturnId()
    {
        // Arrange
        _userRepoMock
            .Setup(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = BuildCommand();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _userRepoMock.Verify(r => r.AddAsync(
            It.Is<User>(u => u.Email == "test@example.com" && u.Name == "João Silva"),
            It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ExistingEmail_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var existingUser = new User(Guid.NewGuid(), "Existing", "test@example.com", "11111111111", "hash", UserType.User);
        _userRepoMock
            .Setup(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        var command = BuildCommand();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ExistingEmail_ShouldNotCallAddAsync()
    {
        // Arrange
        var existingUser = new User(Guid.NewGuid(), "Existing", "test@example.com", "11111111111", "hash", UserType.User);
        _userRepoMock
            .Setup(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        var command = BuildCommand();

        // Act
        try { await _handler.Handle(command, CancellationToken.None); } catch { }

        // Assert
        _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_NewUser_ShouldHashPasswordBeforeCreating()
    {
        // Arrange
        _userRepoMock
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = BuildCommand();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _cryptoMock.Verify(c => c.Hash(It.IsAny<string>()), Times.Once);
        _userRepoMock.Verify(r => r.AddAsync(
            It.Is<User>(u => u.PasswordHash == "hashed_password"),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
