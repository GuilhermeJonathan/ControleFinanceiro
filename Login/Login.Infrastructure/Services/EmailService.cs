using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Login.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Login.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly string _apiKey;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly ILogger<EmailService> _logger;
    private static readonly HttpClient _http = new();

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _logger = logger;
        var s = configuration.GetSection("SmtpSettings");
        _apiKey    = s["Password"]   ?? throw new InvalidOperationException("SmtpSettings:Password (API key) não configurado.");
        _fromEmail = s["FromEmail"]  ?? "noreply@findog.com.br";
        _fromName  = s["FromName"]   ?? "Meu FinDog";
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
            var payload = new
            {
                from    = $"{_fromName} <{_fromEmail}>",
                to      = new[] { toEmail },
                subject = subject,
                html    = htmlBody,
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails")
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json"),
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var response = await _http.SendAsync(request, cancellationToken);
            var body     = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"Resend API error {(int)response.StatusCode}: {body}");

            _logger.LogInformation("E-mail enviado para {ToEmail} — assunto: {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao enviar e-mail para {ToEmail}", toEmail);
            throw;
        }
    }
}
