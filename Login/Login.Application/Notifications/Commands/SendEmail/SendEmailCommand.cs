using Login.Application.Common.Interfaces;
using MediatR;

namespace Login.Application.Notifications.Commands.SendEmail;

/// <summary>
/// Envia um e-mail já pronto (assunto + HTML). Usado como gateway central de e-mail:
/// outras APIs (ex.: Patrimônio) chamam este comando via endpoint interno protegido
/// por service key, em vez de manterem sua própria integração com o Resend.
/// </summary>
public record SendEmailCommand(string ToEmail, string ToName, string Subject, string HtmlBody, string? FromName = null) : IRequest;

public class SendEmailCommandHandler(IEmailService emailService) : IRequestHandler<SendEmailCommand>
{
    public async Task Handle(SendEmailCommand request, CancellationToken cancellationToken)
    {
        await emailService.SendAsync(request.ToEmail, request.ToName, request.Subject, request.HtmlBody, cancellationToken, request.FromName);
    }
}
