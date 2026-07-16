using Login.Application.Common.Interfaces;
using Login.Application.Notifications.Commands.SendEmail;
using Moq;

namespace Login.Application.Tests.Users;

public class SendEmailCommandHandlerTests
{
    [Fact]
    public async Task Handle_RepassaParaEmailService()
    {
        var emailMock = new Mock<IEmailService>();
        var handler = new SendEmailCommandHandler(emailMock.Object);

        var cmd = new SendEmailCommand("cliente@x.com", "Cliente", "Assunto", "<b>corpo</b>");
        await handler.Handle(cmd, CancellationToken.None);

        emailMock.Verify(e => e.SendAsync("cliente@x.com", "Cliente", "Assunto", "<b>corpo</b>",
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
