using System.Net.Http.Json;
using ControleFinanceiro.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ControleFinanceiro.Infrastructure.Services;

/// <summary>
/// Implementação de IEmailService que NÃO envia direto pelo Resend — delega para a
/// API de Login (gateway central de e-mail), via /internal/email com service key.
/// Assim a chave do Resend e o domínio remetente ficam num único lugar (Login).
/// Envio é best-effort: falha de e-mail não interrompe o fluxo que a disparou.
/// </summary>
public class LoginEmailGateway(
    HttpClient http,
    IConfiguration configuration,
    ILogger<LoginEmailGateway> logger) : IEmailService
{
    public async Task SendAsync(string toEmail, string toName, string subject, string htmlBody, CancellationToken cancellationToken = default, string? fromName = null)
    {
        var baseUrl = configuration["LoginApi:BaseUrl"]?.TrimEnd('/');
        var serviceKey = configuration["ServiceAuth:ApiKey"];
        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(serviceKey))
        {
            logger.LogWarning("[LoginEmailGateway] LoginApi:BaseUrl ou ServiceAuth:ApiKey não configurados — e-mail para {to} ignorado.", toEmail);
            return;
        }

        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/internal/email");
            req.Headers.Add("X-Service-Key", serviceKey);
            req.Content = JsonContent.Create(new { toEmail, toName, subject, htmlBody, fromName });

            using var resp = await http.SendAsync(req, cancellationToken);
            if (!resp.IsSuccessStatusCode)
                logger.LogWarning("[LoginEmailGateway] Falha ao enviar e-mail para {to}: {status}", toEmail, resp.StatusCode);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[LoginEmailGateway] Erro ao enviar e-mail para {to}", toEmail);
        }
    }
}
