using FluentAssertions;
using Login.Application.Common.Interfaces;
using Login.Domain.Entities;
using Login.Infrastructure.Persistence;
using Login.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Login.Application.Tests.Trial;

public class TrialExpirationEmailServiceTests
{
    // ── helpers ──────────────────────────────────────────────────────────────

    private static AppDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    /// <summary>
    /// Builds a minimal User in Trial with the given PlanExpiresAt (UTC).
    /// The helpers sets TrialStartedAt via reflection-free path: AdminSetTrial
    /// uses AddDays from UtcNow, so we use AdminSetTrial then override PlanExpiresAt
    /// by calling SetPlan won't work (changes PlanType). Instead we expose the
    /// constructor + AdminSetTrial and then patch via the public method.
    /// Simpler: use SetPlan is wrong, use constructor + AdminSetTrial approach.
    /// We call AdminSetTrial(1) to set PlanType=Trial then patch the expiry
    /// through a dedicated helper that creates the user with the exact desired date.
    /// </summary>
    private static User BuildTrialUser(
        DateTime planExpiresAtUtc,
        bool trialD7Sent = false,
        bool trialD1Sent = false,
        bool isActive = true,
        bool isBlocked = false,
        PlanType planType = PlanType.Trial)
    {
        var user = new User(
            Guid.NewGuid(),
            "Test User",
            $"test-{Guid.NewGuid():N}@example.com",
            "12345678900",
            "hash",
            UserType.User);

        if (!isActive)
            user.Deactivate();

        if (isBlocked)
            user.Block();

        if (planType == PlanType.Trial)
        {
            // AdminSetTrial sets PlanType=Trial and PlanExpiresAt = UtcNow + days.
            // We need exact date, so compute days from now to target (may be fractional).
            var daysUntilExpiry = (planExpiresAtUtc - DateTime.UtcNow).TotalDays;
            user.AdminSetTrial((int)Math.Round(daysUntilExpiry));

            // AdminSetTrial uses integer days from UtcNow, which may be off by <1 day.
            // Override via SetPlan (changes IsPaying), but we need Trial, not Monthly.
            // Workaround: use reflection to set PlanExpiresAt precisely.
            var prop = typeof(User).GetProperty(
                nameof(User.PlanExpiresAt),
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            prop!.SetValue(user, planExpiresAtUtc);
        }
        else if (planType != PlanType.None)
        {
            user.SetPlan(planType, planExpiresAtUtc);
        }

        if (trialD7Sent) user.MarkTrialD7EmailSent();
        if (trialD1Sent) user.MarkTrialD1EmailSent();

        return user;
    }

    private static (TrialExpirationEmailService svc, Mock<IEmailService> emailMock)
        BuildService(AppDbContext db)
    {
        var emailMock = new Mock<IEmailService>();
        emailMock
            .Setup(e => e.SendAsync(
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Build a real ServiceProvider with the in-memory db and mock email
        var services = new ServiceCollection();
        services.AddSingleton(db);
        services.AddSingleton<AppDbContext>(db);
        services.AddSingleton(emailMock.Object);

        var provider = services.BuildServiceProvider();

        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        var scopeMock = new Mock<IServiceScope>();

        scopeMock.Setup(s => s.ServiceProvider).Returns(provider);
        scopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);

        var svc = new TrialExpirationEmailService(
            scopeFactoryMock.Object,
            NullLogger<TrialExpirationEmailService>.Instance);

        return (svc, emailMock);
    }

    // ── tests ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ProcessAsync_UserExpiresInSevenDays_TrialD7NotSent_SendsD7Email()
    {
        // Arrange — PlanExpiresAt = today (Brasilia) + 7 days, expressed in UTC
        var hoje = DateTime.UtcNow.AddHours(-3).Date;
        var expiry = hoje.AddDays(7).ToUniversalTime();

        using var db = CreateInMemoryDb();
        var user = BuildTrialUser(expiry);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var (svc, emailMock) = BuildService(db);

        // Act
        await svc.ProcessAsync(CancellationToken.None);

        // Assert — e-mail D-7 sent
        emailMock.Verify(e => e.SendAsync(
            user.Email, user.Name,
            It.Is<string>(s => s.Contains("7 dias")),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);

        // Assert — flag set
        user.TrialD7EmailSent.Should().BeTrue();
        user.TrialD1EmailSent.Should().BeFalse();
    }

    [Fact]
    public async Task ProcessAsync_UserExpiresInOneDay_TrialD1NotSent_SendsD1Email()
    {
        // Arrange
        var hoje = DateTime.UtcNow.AddHours(-3).Date;
        var expiry = hoje.AddDays(1).ToUniversalTime();

        using var db = CreateInMemoryDb();
        var user = BuildTrialUser(expiry);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var (svc, emailMock) = BuildService(db);

        // Act
        await svc.ProcessAsync(CancellationToken.None);

        // Assert — e-mail D-1 sent
        emailMock.Verify(e => e.SendAsync(
            user.Email, user.Name,
            It.Is<string>(s => s.Contains("Último dia")),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);

        user.TrialD1EmailSent.Should().BeTrue();
        user.TrialD7EmailSent.Should().BeFalse();
    }

    [Fact]
    public async Task ProcessAsync_UserExpiresInSevenDays_TrialD7AlreadySent_DoesNotSendAgain()
    {
        // Arrange
        var hoje = DateTime.UtcNow.AddHours(-3).Date;
        var expiry = hoje.AddDays(7).ToUniversalTime();

        using var db = CreateInMemoryDb();
        var user = BuildTrialUser(expiry, trialD7Sent: true);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var (svc, emailMock) = BuildService(db);

        // Act
        await svc.ProcessAsync(CancellationToken.None);

        // Assert — no email sent
        emailMock.Verify(e => e.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_UserWithPaidPlan_DoesNotSendEmail()
    {
        // Arrange
        var hoje = DateTime.UtcNow.AddHours(-3).Date;
        var expiry = hoje.AddDays(7).ToUniversalTime();

        using var db = CreateInMemoryDb();
        var user = BuildTrialUser(expiry, planType: PlanType.Monthly);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var (svc, emailMock) = BuildService(db);

        // Act
        await svc.ProcessAsync(CancellationToken.None);

        // Assert
        emailMock.Verify(e => e.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_InactiveUser_DoesNotSendEmail()
    {
        // Arrange
        var hoje = DateTime.UtcNow.AddHours(-3).Date;
        var expiry = hoje.AddDays(7).ToUniversalTime();

        using var db = CreateInMemoryDb();
        var user = BuildTrialUser(expiry, isActive: false);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var (svc, emailMock) = BuildService(db);

        // Act
        await svc.ProcessAsync(CancellationToken.None);

        // Assert
        emailMock.Verify(e => e.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_BlockedUser_DoesNotSendEmail()
    {
        // Arrange
        var hoje = DateTime.UtcNow.AddHours(-3).Date;
        var expiry = hoje.AddDays(7).ToUniversalTime();

        using var db = CreateInMemoryDb();
        var user = BuildTrialUser(expiry, isBlocked: true);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var (svc, emailMock) = BuildService(db);

        // Act
        await svc.ProcessAsync(CancellationToken.None);

        // Assert
        emailMock.Verify(e => e.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_EmailThrows_DoesNotMarkFlagAndDoesNotThrow()
    {
        // Arrange
        var hoje = DateTime.UtcNow.AddHours(-3).Date;
        var expiry = hoje.AddDays(7).ToUniversalTime();

        using var db = CreateInMemoryDb();
        var user = BuildTrialUser(expiry);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var emailMock = new Mock<IEmailService>();
        emailMock
            .Setup(e => e.SendAsync(
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("SMTP error"));

        var services = new ServiceCollection();
        services.AddSingleton(db);
        services.AddSingleton<AppDbContext>(db);
        services.AddSingleton(emailMock.Object);
        var provider = services.BuildServiceProvider();

        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        var scopeMock = new Mock<IServiceScope>();
        scopeMock.Setup(s => s.ServiceProvider).Returns(provider);
        scopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);

        var svc = new TrialExpirationEmailService(
            scopeFactoryMock.Object,
            NullLogger<TrialExpirationEmailService>.Instance);

        // Act — should not throw
        var act = async () => await svc.ProcessAsync(CancellationToken.None);
        await act.Should().NotThrowAsync();

        // Assert — flag NOT set (email failed)
        user.TrialD7EmailSent.Should().BeFalse();
    }
}
