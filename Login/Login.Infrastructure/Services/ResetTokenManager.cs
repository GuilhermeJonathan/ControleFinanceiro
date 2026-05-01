using Login.Application.Common.Interfaces;
using Login.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace Login.Infrastructure.Services;

public class ResetTokenManager : IResetTokenManager
{
    private static readonly Dictionary<Guid, (string Token, DateTime Expires)> _tokens = new();

    private readonly IEmailService _emailService;
    private readonly string _appUrl;

    public ResetTokenManager(IEmailService emailService, IConfiguration configuration)
    {
        _emailService = emailService;
        _appUrl = configuration["SmtpSettings:AppUrl"] ?? "https://app.findog.com.br";
    }

    public async Task GenerateAndSendAsync(User user, CancellationToken cancellationToken = default)
    {
        var token = Guid.NewGuid().ToString("N");
        _tokens[user.Id] = (token, DateTime.UtcNow.AddHours(2));

        var resetLink = $"{_appUrl}/reset-password?token={token}&email={Uri.EscapeDataString(user.Email)}";
        var html = BuildEmailHtml(user.Name, resetLink);

        await _emailService.SendAsync(
            toEmail: user.Email,
            toName: user.Name,
            subject: "Redefinição de senha — Meu FinDog",
            htmlBody: html,
            cancellationToken: cancellationToken);
    }

    public Task<bool> ValidateAsync(Guid userId, string token, CancellationToken cancellationToken = default)
    {
        if (!_tokens.TryGetValue(userId, out var entry))
            return Task.FromResult(false);

        if (entry.Expires < DateTime.UtcNow)
        {
            _tokens.Remove(userId);
            return Task.FromResult(false);
        }

        var valid = entry.Token == token;
        if (valid) _tokens.Remove(userId);

        return Task.FromResult(valid);
    }

    private static string BuildEmailHtml(string name, string resetLink) => $"""
        <!DOCTYPE html>
        <html lang="pt-BR">
        <head><meta charset="UTF-8"><meta name="viewport" content="width=device-width,initial-scale=1"></head>
        <body style="margin:0;padding:0;background:#0d1117;font-family:'Segoe UI',Arial,sans-serif;">
          <table width="100%" cellpadding="0" cellspacing="0" style="background:#0d1117;padding:40px 16px;">
            <tr><td align="center">
              <table width="100%" cellpadding="0" cellspacing="0" style="max-width:520px;background:#161b22;border-radius:16px;border:1px solid #30363d;overflow:hidden;">
                <tr>
                  <td style="background:#1f2937;padding:32px 40px;text-align:center;border-bottom:1px solid #30363d;">
                    <div style="font-size:48px;margin-bottom:8px;">🐕</div>
                    <div style="color:#3fb950;font-size:22px;font-weight:800;letter-spacing:0.5px;">Meu FinDog</div>
                    <div style="color:#8b949e;font-size:13px;margin-top:4px;">Seu assistente financeiro</div>
                  </td>
                </tr>
                <tr>
                  <td style="padding:40px;">
                    <p style="color:#c9d1d9;font-size:16px;margin:0 0 8px 0;">Olá, <strong style="color:#e6edf3;">{name}</strong>!</p>
                    <p style="color:#8b949e;font-size:14px;line-height:1.6;margin:0 0 28px 0;">
                      Recebemos uma solicitação para redefinir a senha da sua conta.<br>
                      Clique no botão abaixo para criar uma nova senha:
                    </p>
                    <div style="text-align:center;margin-bottom:32px;">
                      <a href="{resetLink}"
                         style="display:inline-block;background:#3fb950;color:#0d1117;font-size:15px;
                                font-weight:700;padding:14px 36px;border-radius:10px;text-decoration:none;
                                letter-spacing:0.3px;">
                        🔑 Redefinir minha senha
                      </a>
                    </div>
                    <div style="background:#1c2128;border:1px solid #30363d;border-radius:8px;padding:16px;margin-bottom:28px;">
                      <p style="color:#8b949e;font-size:12px;margin:0 0 6px 0;">
                        ⏱️ Este link expira em <strong style="color:#c9d1d9;">2 horas</strong>.
                      </p>
                      <p style="color:#8b949e;font-size:12px;margin:0;">
                        🔒 Se você não solicitou esta redefinição, ignore este e-mail — sua senha permanece a mesma.
                      </p>
                    </div>
                    <p style="color:#6e7681;font-size:12px;margin:0;border-top:1px solid #30363d;padding-top:20px;line-height:1.6;">
                      Se o botão não funcionar, copie e cole este link no seu navegador:<br>
                      <span style="color:#3fb950;word-break:break-all;">{resetLink}</span>
                    </p>
                  </td>
                </tr>
                <tr>
                  <td style="background:#0d1117;padding:20px 40px;text-align:center;border-top:1px solid #30363d;">
                    <p style="color:#484f58;font-size:12px;margin:0;">
                      © {DateTime.UtcNow.Year} Meu FinDog · Você está recebendo este e-mail porque solicitou a redefinição de senha.
                    </p>
                  </td>
                </tr>
              </table>
            </td></tr>
          </table>
        </body>
        </html>
        """;
}
