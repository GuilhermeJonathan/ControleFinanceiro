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
              <table width="100%" cellpadding="0" cellspacing="0" style="max-width:520px;border-radius:16px;overflow:hidden;border:1px solid #238636;">

                <!-- HEADER: banner og-image -->
                <tr>
                  <td style="background:#0d1117;padding:0;border-bottom:2px solid #238636;">
                    <a href="https://app.findog.com.br" style="display:block;line-height:0;">
                      <img src="https://app.findog.com.br/og-image.png"
                           alt="Meu FinDog · seu assistente financeiro"
                           width="520"
                           style="display:block;width:100%;max-width:520px;height:auto;border:0;" />
                    </a>
                  </td>
                </tr>

                <!-- BODY -->
                <tr>
                  <td style="background:#161b22;padding:36px 40px;">
                    <p style="color:#e6edf3;font-size:17px;font-weight:600;margin:0 0 6px 0;">Olá, {name}! 👋</p>
                    <p style="color:#8b949e;font-size:14px;line-height:1.7;margin:0 0 32px 0;">
                      Recebemos uma solicitação para redefinir a senha da sua conta.<br>
                      Clique no botão abaixo para criar uma nova senha:
                    </p>

                    <!-- BOTÃO -->
                    <table cellpadding="0" cellspacing="0" style="margin:0 auto 32px;">
                      <tr>
                        <td style="background:#3fb950;border-radius:12px;">
                          <a href="{resetLink}"
                             style="display:block;color:#0d1117;font-size:15px;font-weight:800;
                                    padding:15px 40px;text-decoration:none;letter-spacing:0.4px;">
                            🔑&nbsp; Redefinir minha senha
                          </a>
                        </td>
                      </tr>
                    </table>

                    <!-- AVISOS -->
                    <table cellpadding="0" cellspacing="0" width="100%" style="background:#0d1117;border-radius:10px;border:1px solid #30363d;margin-bottom:28px;">
                      <tr>
                        <td style="padding:16px 20px;">
                          <p style="color:#8b949e;font-size:13px;margin:0 0 8px 0;">
                            ⏱&nbsp; Este link expira em <strong style="color:#e6edf3;">2 horas</strong>.
                          </p>
                          <p style="color:#8b949e;font-size:13px;margin:0;">
                            🔒&nbsp; Se você não solicitou esta redefinição, ignore este e-mail — sua senha permanece a mesma.
                          </p>
                        </td>
                      </tr>
                    </table>

                    <p style="color:#484f58;font-size:12px;margin:0;border-top:1px solid #30363d;padding-top:20px;line-height:1.7;">
                      Se o botão não funcionar, copie e cole este link no navegador:<br>
                      <a href="{resetLink}" style="color:#3fb950;word-break:break-all;text-decoration:none;">{resetLink}</a>
                    </p>
                  </td>
                </tr>

                <!-- FOOTER -->
                <tr>
                  <td style="background:#0d1117;padding:18px 40px;text-align:center;border-top:1px solid #30363d;">
                    <p style="color:#484f58;font-size:12px;margin:0;">
                      © {DateTime.UtcNow.Year} Meu FinDog &nbsp;·&nbsp; <a href="https://app.findog.com.br" style="color:#484f58;text-decoration:none;">app.findog.com.br</a>
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
