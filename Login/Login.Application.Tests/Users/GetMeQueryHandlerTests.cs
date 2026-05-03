using Login.Application.Common.Interfaces;
using Login.Application.Users.Queries.GetMe;
using Login.Domain.Common;
using Login.Domain.Entities;
using Login.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace Login.Application.Tests.Users;

public class GetMeQueryHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IUserAccessor> _userAccessorMock = new();
    private readonly GetMeQueryHandler _handler;

    private readonly Guid _userId = Guid.NewGuid();

    public GetMeQueryHandlerTests()
    {
        _userAccessorMock.Setup(a => a.UserId).Returns(_userId);

        _handler = new GetMeQueryHandler(
            _userRepoMock.Object,
            _userAccessorMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingUser_ShouldReturnMeDto()
    {
        // Arrange
        var user = new User(_userId, "Maria Silva", "maria@example.com", "12345678900", "hash", UserType.User);
        _userRepoMock
            .Setup(r => r.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(new GetMeQuery(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(_userId);
        result.Name.Should().Be("Maria Silva");
        result.Email.Should().Be("maria@example.com");
    }

    [Fact]
    public async Task Handle_ExistingUser_ShouldReturnCellphoneAndDocument()
    {
        // Arrange
        var user = new User(_userId, "Maria Silva", "maria@example.com", "12345678900", "hash", UserType.User);
        user.UpdateProfile("Maria Silva", null, null, "11988887777", null, null, null);
        _userRepoMock
            .Setup(r => r.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(new GetMeQuery(), CancellationToken.None);

        // Assert
        result.Document.Should().Be("12345678900");
        result.Cellphone.Should().Be("11988887777");
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
            _handler.Handle(new GetMeQuery(), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ExistingUser_ShouldUseCurrentUserIdFromAccessor()
    {
        // Arrange
        var user = new User(_userId, "Maria", "maria@example.com", "12345678900", "hash", UserType.User);
        _userRepoMock
            .Setup(r => r.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        await _handler.Handle(new GetMeQuery(), CancellationToken.None);

        // Assert — garante que usou o ID do usuário logado, não um ID hardcoded
        _userRepoMock.Verify(r => r.GetByIdAsync(_userId, It.IsAny<CancellationToken>()), Times.Once);
        _userAccessorMock.Verify(a => a.UserId, Times.AtLeastOnce);
    }
}
