using FluentAssertions;
using Login.Application.Common.Interfaces;
using Login.Application.Payments.Commands.ProcessWebhook;
using Login.Domain.Common;
using Login.Domain.Entities;
using Login.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace Login.Application.Tests.Payments;

public class ProcessWebhookCommandHandlerTests
{
    private readonly Mock<IMercadoPagoService> _mpMock = new();
    private readonly Mock<ISubscriptionRepository> _subscriptionRepoMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IPaymentTransactionRepository> _txRepoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IEmailService> _emailServiceMock = new();
    private readonly Mock<IConfiguration> _configMock = new();
    private readonly Mock<ILogger<ProcessWebhookCommandHandler>> _loggerMock = new();
    private readonly ProcessWebhookCommandHandler _handler;

    public ProcessWebhookCommandHandlerTests()
    {
        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _txRepoMock
            .Setup(r => r.AddAsync(It.IsAny<PaymentTransaction>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _emailServiceMock
            .Setup(e => e.SendAsync(
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _configMock
            .Setup(c => c["AdminEmail"])
            .Returns("admin@findog.com.br");

        _handler = new ProcessWebhookCommandHandler(
            _mpMock.Object,
            _subscriptionRepoMock.Object,
            _userRepoMock.Object,
            _txRepoMock.Object,
            _uowMock.Object,
            _emailServiceMock.Object,
            _configMock.Object,
            _loggerMock.Object);
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private static MercadoPagoSubscription BuildSubscription(
        Guid userId,
        PlanType planType = PlanType.Monthly,
        string mpSubId = "sub_123")
    {
        var sub = new MercadoPagoSubscription(Guid.NewGuid(), userId, mpSubId, planType);
        return sub;
    }

    private static User BuildUser(Guid id)
        => new User(id, "Test User", "test@example.com", "12345678900", "hash", UserType.User);

    // ── happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_Authorized_ShouldActivatePlanSaveTransactionAndSendEmails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mpSubId = "sub_authorized";
        var user = BuildUser(userId);
        var subscription = BuildSubscription(userId, PlanType.Monthly, mpSubId);

        _mpMock
            .Setup(m => m.GetSubscriptionAsync(mpSubId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MpSubscriptionDetail(
                mpSubId, "authorized", userId.ToString(), "pay_001", 4.90m));

        _subscriptionRepoMock
            .Setup(r => r.GetByMpIdAsync(mpSubId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        _userRepoMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new ProcessWebhookCommand("subscription_preapproval", mpSubId, null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert — plan activated
        user.PlanType.Should().Be(PlanType.Monthly);
        user.IsPaying.Should().BeTrue();

        // Assert — transaction saved
        _txRepoMock.Verify(r => r.AddAsync(
            It.Is<PaymentTransaction>(t =>
                t.UserId == userId &&
                t.UserEmail == user.Email &&
                t.PlanType == PlanType.Monthly &&
                t.Status == "authorized"),
            It.IsAny<CancellationToken>()), Times.Once);

        // Assert — SaveChanges called
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        // Assert — two emails sent (customer + admin)
        _emailServiceMock.Verify(e => e.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    // ── subscription not found locally ────────────────────────────────────────

    [Fact]
    public async Task Handle_SubscriptionNotFound_ShouldNotSaveOrSendEmail()
    {
        // Arrange
        var mpSubId = "sub_missing";

        _mpMock
            .Setup(m => m.GetSubscriptionAsync(mpSubId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MpSubscriptionDetail(
                mpSubId, "authorized", Guid.NewGuid().ToString(), null, 4.90m));

        _subscriptionRepoMock
            .Setup(r => r.GetByMpIdAsync(mpSubId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MercadoPagoSubscription?)null);

        var command = new ProcessWebhookCommand("subscription_preapproval", mpSubId, null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _txRepoMock.Verify(r => r.AddAsync(It.IsAny<PaymentTransaction>(), It.IsAny<CancellationToken>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _emailServiceMock.Verify(e => e.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── user not found ────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_UserNotFound_ShouldNotSaveOrSendEmail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mpSubId = "sub_nouser";
        var subscription = BuildSubscription(userId, PlanType.Monthly, mpSubId);

        _mpMock
            .Setup(m => m.GetSubscriptionAsync(mpSubId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MpSubscriptionDetail(
                mpSubId, "authorized", userId.ToString(), null, 4.90m));

        _subscriptionRepoMock
            .Setup(r => r.GetByMpIdAsync(mpSubId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        _userRepoMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new ProcessWebhookCommand("subscription_preapproval", mpSubId, null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _txRepoMock.Verify(r => r.AddAsync(It.IsAny<PaymentTransaction>(), It.IsAny<CancellationToken>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _emailServiceMock.Verify(e => e.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── cancelled status ──────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_Cancelled_ShouldCancelSubscriptionAndNotSendEmail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mpSubId = "sub_cancelled";
        var subscription = BuildSubscription(userId, PlanType.Monthly, mpSubId);

        _mpMock
            .Setup(m => m.GetSubscriptionAsync(mpSubId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MpSubscriptionDetail(
                mpSubId, "cancelled", userId.ToString(), null, 4.90m));

        _subscriptionRepoMock
            .Setup(r => r.GetByMpIdAsync(mpSubId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        _subscriptionRepoMock
            .Setup(r => r.Update(It.IsAny<MercadoPagoSubscription>()));

        var command = new ProcessWebhookCommand("subscription_preapproval", mpSubId, null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert — subscription cancelled
        subscription.Status.Should().Be("cancelled");

        // Assert — save called once for the cancel
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        // Assert — no transaction saved, no emails
        _txRepoMock.Verify(r => r.AddAsync(It.IsAny<PaymentTransaction>(), It.IsAny<CancellationToken>()), Times.Never);
        _emailServiceMock.Verify(e => e.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── email failure is non-fatal ─────────────────────────────────────────────

    [Fact]
    public async Task Handle_EmailFailure_ShouldStillSavePlanAndTransaction()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mpSubId = "sub_emailfail";
        var user = BuildUser(userId);
        var subscription = BuildSubscription(userId, PlanType.Annual, mpSubId);

        _mpMock
            .Setup(m => m.GetSubscriptionAsync(mpSubId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MpSubscriptionDetail(
                mpSubId, "authorized", userId.ToString(), "pay_002", 39.90m));

        _subscriptionRepoMock
            .Setup(r => r.GetByMpIdAsync(mpSubId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        _userRepoMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Email throws for both calls
        _emailServiceMock
            .Setup(e => e.SendAsync(
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("SMTP error"));

        var command = new ProcessWebhookCommand("subscription_preapproval", mpSubId, null);

        // Act — should not throw
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().NotThrowAsync();

        // Assert — plan still activated and transaction still saved
        user.IsPaying.Should().BeTrue();
        user.PlanType.Should().Be(PlanType.Annual);

        _txRepoMock.Verify(r => r.AddAsync(
            It.IsAny<PaymentTransaction>(),
            It.IsAny<CancellationToken>()), Times.Once);

        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
