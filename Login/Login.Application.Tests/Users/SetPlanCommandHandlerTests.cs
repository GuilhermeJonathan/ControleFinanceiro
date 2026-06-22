using Login.Application.Common.Interfaces;
using Login.Application.Users.Commands.SetPlan;
using Login.Domain.Common;
using Login.Domain.Entities;
using Login.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace Login.Application.Tests.Users;

public class SetPlanCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IEmailService> _emailMock = new();
    private readonly SetPlanCommandHandler _handler;

    private static readonly Guid UserId = Guid.NewGuid();

    private static User BuildUser() =>
        new User(UserId, "Maria Silva", "maria@example.com", "12345678900", "hash", UserType.User);

    public SetPlanCommandHandlerTests()
    {
        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _emailMock
            .Setup(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new SetPlanCommandHandler(
            _userRepoMock.Object,
            _uowMock.Object,
            _emailMock.Object);
    }

    [Fact]
    public async Task Handle_Monthly_ShouldSetPlanAndSendEmail()
    {
        // Arrange
        var user = BuildUser();
        _userRepoMock.Setup(r => r.GetByIdAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var command = new SetPlanCommand(UserId, (int)PlanType.Monthly, null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        user.PlanType.Should().Be(PlanType.Monthly);
        user.IsPaying.Should().BeTrue();
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _emailMock.Verify(e => e.SendAsync(
            user.Email, user.Name,
            It.Is<string>(s => s.Contains("ativado")),
            It.Is<string>(b => b.Contains("Mensal")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Annual_ShouldSetPlanAndSendEmail()
    {
        // Arrange
        var user = BuildUser();
        _userRepoMock.Setup(r => r.GetByIdAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var command = new SetPlanCommand(UserId, (int)PlanType.Annual, null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        user.PlanType.Should().Be(PlanType.Annual);
        _emailMock.Verify(e => e.SendAsync(
            user.Email, user.Name,
            It.IsAny<string>(),
            It.Is<string>(b => b.Contains("Anual")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Trial_ShouldSetTrialAndSendEmail()
    {
        // Arrange
        var user = BuildUser();
        _userRepoMock.Setup(r => r.GetByIdAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var command = new SetPlanCommand(UserId, (int)PlanType.Trial, 15);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        user.PlanType.Should().Be(PlanType.Trial);
        user.PlanExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(15), TimeSpan.FromSeconds(5));
        _emailMock.Verify(e => e.SendAsync(
            user.Email, user.Name,
            It.IsAny<string>(),
            It.Is<string>(b => b.Contains("Trial")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_None_ShouldClearPlanAndNotSendEmail()
    {
        // Arrange
        var user = BuildUser();
        user.SetPlan(PlanType.Monthly, DateTime.UtcNow.AddDays(30));
        _userRepoMock.Setup(r => r.GetByIdAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var command = new SetPlanCommand(UserId, (int)PlanType.None, null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        user.PlanType.Should().Be(PlanType.None);
        user.PlanExpiresAt.Should().BeNull();
        _emailMock.Verify(e => e.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByIdAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var command = new SetPlanCommand(UserId, (int)PlanType.Monthly, null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldNotCallSaveChanges()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByIdAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var command = new SetPlanCommand(UserId, (int)PlanType.Monthly, null);

        // Act
        try { await _handler.Handle(command, CancellationToken.None); } catch { }

        // Assert
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _emailMock.Verify(e => e.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }
}
