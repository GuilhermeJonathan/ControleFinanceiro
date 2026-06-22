using Login.Application.Common.Interfaces;
using Login.Domain.Entities;
using Login.Infrastructure.Persistence;
using Login.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using FluentAssertions;
using Moq;

namespace Login.Application.Tests.Users;

public class ReengagementEmailServiceTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static User BuildUser(string email = "u@test.com") =>
        new User(Guid.NewGuid(), "Ana Lima", email, "12345678900", "hash", UserType.User);

    private static void SetPlanExpires(User user, DateTime value) =>
        user.SetPlan(PlanType.Monthly, value);

    [Fact]
    public async Task ProcessAsync_UserExpired30DaysAgo_ShouldSendEmailAndMark()
    {
        // Arrange
        await using var db = CreateDb();
        var emailMock = new Mock<IEmailService>();
        emailMock.Setup(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var user = BuildUser();
        var expiresAt = DateTime.UtcNow.AddHours(-3).Date.AddDays(-30);
        SetPlanExpires(user, expiresAt);

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var factory = CreateScopeFactory(db, emailMock.Object);
        var service = new ReengagementEmailService(factory, NullLogger<ReengagementEmailService>.Instance);

        // Act
        await service.ProcessAsync(CancellationToken.None);

        // Assert
        emailMock.Verify(e => e.SendAsync(
            user.Email, user.Name,
            It.Is<string>(s => s.Contains("saudade") || s.Contains("falta")),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);

        var updated = await db.Users.FindAsync(user.Id);
        updated!.ReengagementEmailSent.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessAsync_UserAlreadySentEmail_ShouldNotSendAgain()
    {
        // Arrange
        await using var db = CreateDb();
        var emailMock = new Mock<IEmailService>();

        var user = BuildUser();
        var expiresAt = DateTime.UtcNow.AddHours(-3).Date.AddDays(-30);
        SetPlanExpires(user, expiresAt);
        user.MarkReengagementEmailSent();

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var factory = CreateScopeFactory(db, emailMock.Object);
        var service = new ReengagementEmailService(factory, NullLogger<ReengagementEmailService>.Instance);

        // Act
        await service.ProcessAsync(CancellationToken.None);

        // Assert
        emailMock.Verify(e => e.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_UserExpired29DaysAgo_ShouldNotSend()
    {
        // Arrange
        await using var db = CreateDb();
        var emailMock = new Mock<IEmailService>();

        var user = BuildUser();
        SetPlanExpires(user, DateTime.UtcNow.AddHours(-3).Date.AddDays(-29));

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var factory = CreateScopeFactory(db, emailMock.Object);
        var service = new ReengagementEmailService(factory, NullLogger<ReengagementEmailService>.Instance);

        // Act
        await service.ProcessAsync(CancellationToken.None);

        // Assert
        emailMock.Verify(e => e.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_BlockedUser_ShouldNotSend()
    {
        // Arrange
        await using var db = CreateDb();
        var emailMock = new Mock<IEmailService>();

        var user = BuildUser();
        SetPlanExpires(user, DateTime.UtcNow.AddHours(-3).Date.AddDays(-30));
        user.Block();

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var factory = CreateScopeFactory(db, emailMock.Object);
        var service = new ReengagementEmailService(factory, NullLogger<ReengagementEmailService>.Instance);

        // Act
        await service.ProcessAsync(CancellationToken.None);

        // Assert
        emailMock.Verify(e => e.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_EmailFails_ShouldNotMarkAsSent()
    {
        // Arrange
        await using var db = CreateDb();
        var emailMock = new Mock<IEmailService>();
        emailMock.Setup(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("smtp error"));

        var user = BuildUser();
        SetPlanExpires(user, DateTime.UtcNow.AddHours(-3).Date.AddDays(-30));

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var factory = CreateScopeFactory(db, emailMock.Object);
        var service = new ReengagementEmailService(factory, NullLogger<ReengagementEmailService>.Instance);

        // Act — não deve lançar exceção
        await service.ProcessAsync(CancellationToken.None);

        // Assert — flag não deve ser marcado
        var updated = await db.Users.FindAsync(user.Id);
        updated!.ReengagementEmailSent.Should().BeFalse();
    }

    private static IServiceScopeFactory CreateScopeFactory(AppDbContext db, IEmailService emailService)
    {
        var services = new ServiceCollection();
        services.AddSingleton<AppDbContext>(db);
        services.AddSingleton<IEmailService>(emailService);
        return services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();
    }
}
