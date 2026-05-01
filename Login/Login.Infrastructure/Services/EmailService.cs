using System.Net;
using System.Net.Mail;
using Login.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Login.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly string _host;
    private readonly int    _port;
    private readonly bool   _enableSsl;
    private readonly string _username;
    private readonly string _password;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _logger = logger;
        var s = configuration.GetSection("SmtpSettings");
        _host      = s["Host"]      ?? throw new InvalidOperationException("SmtpSettings:Host não configurado.");
        _port      = int.Parse(s["Port"] ?? "587");
        _enableSsl = bool.Parse(s["EnableSsl"] ?? "true");
        _username  = s["Username"]  ?? throw new InvalidOperationException("SmtpSettings:Username não configurado.");
        _password  = s["Password"]  ?? throw new InvalidOperationException("SmtpSettings:Password não configurado.");
        _fromEmail = s["FromEmail"] ?? _username;
        _fromName  = s["FromName"]  ?? "Meu FinDog";
    }

    public async Task SendAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new SmtpClient(_host, _port)
            {
                EnableSsl      = _enableSsl,
                Credentials    = new NetworkCredential(_username, _password),
                DeliveryMethod = SmtpDeliveryMethod.Network,
            };

            using var message = new MailMessage
            {
                From       = new MailAddress(_fromEmail, _fromName),
                Subject    = subject,
                Body       = htmlBody,
                IsBodyHtml = true,
            };
            message.To.Add(new MailAddress(toEmail, toName));

            await client.SendMailAsync(message, cancellationToken);

            _logger.LogInformation("E-mail enviado para {ToEmail} — assunto: {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao enviar e-mail para {ToEmail} via {Host}:{Port}", toEmail, _host, _port);
            throw;
        }
    }
}
