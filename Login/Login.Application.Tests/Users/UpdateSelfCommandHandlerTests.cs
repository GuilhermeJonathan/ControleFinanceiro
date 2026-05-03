using Login.Application.Common.Interfaces;
using Login.Application.Users.Commands.UpdateSelf;
using Login.Domain.Common;
using Login.Domain.Entities;
using Login.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace Login.Application.Tests.Users;

public class UpdateSelfCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IUserAccessor> _userAccessorMock = new();
    private readonly UpdateSelfCommandHandler _handler;

    private readonly Guid _userId = Guid.NewGuid();

    public UpdateSelfCommandHandlerTests()
    {
        _userAccessorMock.Setup(a => a.UserId).Returns(_userId);

        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new UpdateSelfCommandHandler(
            _userRepoMock.Object,
            _uowMock.Object,
            _userAccessorMock.Object);
    }

    private User BuildUser() =>
        new(_userId, "Nome Antigo", "user@example.com", "12345678900", "hash", UserType.User);

    [Fact]
    public async Task Handle_ValidUser_ShouldUpdateNameAndSave()
    {
        // Arrange
        var user = BuildUser();
        _userRepoMock
            .Setup(r => r.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new UpdateSelfCommand("Nome Novo", null, null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        user.Name.Should().Be("Nome Novo");
        _userRepoMock.Verify(r => r.Update(user), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCellphone_ShouldUpdateCellphone()
    {
        // Arrange
        var user = BuildUser();
        _userRepoMock
            .Setup(r => r.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new UpdateSelfCommand("Nome Novo", "11999999999", null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        user.Cellphone.Should().Be("11999999999");
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDocument_ShouldUpdateDocument()
    {
        // Arrange
        var user = BuildUser();
        _userRepoMock
            .Setup(r => r.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new UpdateSelfCommand("Nome Novo", null, "98765432100");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        user.Document.Should().Be("98765432100");
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        _userRepoMock
            .Setup(r => r.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new UpdateSelfCommand("Nome Novo", null, null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldNotUpdateOrSave()
    {
        // Arrange
        _userRepoMock
            .Setup(r => r.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new UpdateSelfCommand("Nome Novo", null, null);

        // Act
        try { await _handler.Handle(command, CancellationToken.None); } catch { }

        // Assert
        _userRepoMock.Verify(r => r.Update(It.IsAny<User>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
