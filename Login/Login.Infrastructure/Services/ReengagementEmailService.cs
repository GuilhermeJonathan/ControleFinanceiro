using Login.Application.Common.Interfaces;
using Login.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Login.Infrastructure.Services;

/// <summary>
/// Serviço em background que roda diariamente e envia e-mail de reengajamento
/// para usuários cujo plano expirou há exatamente 30 dias e ainda não receberam o aviso.
/// </summary>
public class ReengagementEmailService(
    IServiceScopeFactory scopeFactory,
    ILogger<ReengagementEmailService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(45), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro no ReengagementEmailService");
            }

            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }

    internal async Task ProcessAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db    = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var email = scope.ServiceProvider.GetRequiredService<IEmailService>();

        // Brasília = UTC-3
        var hoje      = DateTime.UtcNow.AddHours(-3).Date;
        var alvoInicio = hoje.AddDays(-30);
        var alvoFim    = alvoInicio.AddDays(1);

        var usuarios = await db.Users
            .Where(u =>
                u.IsActive && !u.IsBlocked &&
                !u.ReengagementEmailSent &&
                u.PlanExpiresAt.HasValue &&
                u.PlanExpiresAt >= alvoInicio &&
                u.PlanExpiresAt < alvoFim)
            .ToListAsync(ct);

        logger.LogInformation("ReengagementEmail: {Count} usuário(s) com plano expirado há 30 dias", usuarios.Count);

        var enviados = 0;
        foreach (var user in usuarios)
        {
            try
            {
                await email.SendAsync(
                    user.Email, user.Name,
                    "🐾 Sentimos sua falta no Meu FinDog!",
                    BuildEmail(user.Name),
                    ct);

                user.MarkReengagementEmailSent();
                enviados++;
                logger.LogInformation("E-mail de reengajamento enviado para {Email}", user.Email);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Falha ao enviar e-mail de reengajamento para {Email}", user.Email);
            }
        }

        if (enviados > 0)
            await db.SaveChangesAsync(ct);
    }

    private static string BuildEmail(string nome) => $"""
        <div style="font-family:sans-serif;max-width:560px;margin:0 auto;background:#0f1117;color:#e2e8f0;border-radius:12px;overflow:hidden">
          <div style="background:#0f1117;padding:0;border-bottom:2px solid #16a34a">
            <a href="https://app.findog.com.br" style="display:block;line-height:0">
              <img src="https://app.findog.com.br/og-image.png" alt="Meu FinDog" width="560"
                   style="display:block;width:100%;max-width:560px;height:auto;border:0" />
            </a>
          </div>
          <div style="padding:32px 24px">
            <p style="font-size:18px;font-weight:700;color:#f1f5f9">Oi, {nome}! 👋</p>
            <p style="color:#94a3b8;line-height:1.6">
              Faz 30 dias que você não usa o <strong style="color:#e2e8f0">Meu FinDog</strong>.
              Suas finanças estão com saudade de você!
            </p>
            <div style="background:#1e293b;border-radius:10px;padding:16px;margin:20px 0">
              <p style="color:#94a3b8;font-size:13px;margin:0 0 10px">O que está te esperando:</p>
              <p style="margin:6px 0;color:#e2e8f0;font-size:14px">📊 Visão completa dos seus gastos</p>
              <p style="margin:6px 0;color:#e2e8f0;font-size:14px">🎯 Metas financeiras no seu ritmo</p>
              <p style="margin:6px 0;color:#e2e8f0;font-size:14px">💳 Controle de cartões e parcelas</p>
              <p style="margin:6px 0;color:#e2e8f0;font-size:14px">📱 Lançamentos pelo WhatsApp</p>
            </div>
            <div style="text-align:center;margin:28px 0">
              <a href="https://app.findog.com.br/planos"
                 style="background:#16a34a;color:#fff;text-decoration:none;padding:14px 32px;border-radius:10px;font-weight:700;font-size:15px;display:inline-block">
                Voltar a organizar minha vida 🐶
              </a>
            </div>
            <p style="color:#94a3b8;font-size:13px;line-height:1.6;text-align:center">
              Planos a partir de <strong style="color:#e2e8f0">R$ 4,90/mês</strong>. Cancele quando quiser.
            </p>
            <p style="color:#64748b;font-size:12px;text-align:center;margin-top:24px">
              Meu FinDog · <a href="https://app.findog.com.br" style="color:#64748b">app.findog.com.br</a>
            </p>
          </div>
        </div>
        """;
}
