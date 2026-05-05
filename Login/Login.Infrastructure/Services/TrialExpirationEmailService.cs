using Login.Application.Common.Interfaces;
using Login.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Login.Infrastructure.Services;

/// <summary>
/// Serviço em background que roda diariamente e envia e-mails de aviso
/// para usuários cujo trial expira em 7 dias (D-7) ou 1 dia (D-1).
/// </summary>
public class TrialExpirationEmailService(
    IServiceScopeFactory scopeFactory,
    ILogger<TrialExpirationEmailService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Aguarda 30s após start para o app estar totalmente inicializado
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro no TrialExpirationEmailService");
            }

            // Aguarda até a próxima execução (24h)
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }

    internal async Task ProcessAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db    = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var email = scope.ServiceProvider.GetRequiredService<IEmailService>();

        // Brasília = UTC-3
        var hoje = DateTime.UtcNow.AddHours(-3).Date;
        var d7   = hoje.AddDays(7);
        var d1   = hoje.AddDays(1);

        // Busca usuários em trial ativo que ainda não receberam o e-mail correspondente
        var usuarios = await db.Users
            .Where(u =>
                u.IsActive && !u.IsBlocked &&
                u.PlanType == Login.Domain.Entities.PlanType.Trial &&
                u.PlanExpiresAt.HasValue &&
                (
                    (!u.TrialD7EmailSent && u.PlanExpiresAt.Value.Date == d7) ||
                    (!u.TrialD1EmailSent && u.PlanExpiresAt.Value.Date == d1)
                ))
            .ToListAsync(ct);

        logger.LogInformation("TrialExpirationEmail: {Count} usuário(s) encontrado(s)", usuarios.Count);

        foreach (var user in usuarios)
        {
            var expiresAt = user.PlanExpiresAt!.Value.Date;
            var diasRestantes = (expiresAt - hoje).Days;

            var isD7 = diasRestantes == 7 && !user.TrialD7EmailSent;
            var isD1 = diasRestantes == 1 && !user.TrialD1EmailSent;

            if (!isD7 && !isD1) continue;

            try
            {
                if (isD7)
                {
                    await email.SendAsync(
                        user.Email, user.Name,
                        "⏳ Seu trial do Meu FinDog termina em 7 dias",
                        BuildD7Email(user.Name, expiresAt),
                        ct);
                    user.MarkTrialD7EmailSent();
                    logger.LogInformation("E-mail D-7 enviado para {Email}", user.Email);
                }

                if (isD1)
                {
                    await email.SendAsync(
                        user.Email, user.Name,
                        "🚨 Último dia de trial — assine o Meu FinDog hoje",
                        BuildD1Email(user.Name, expiresAt),
                        ct);
                    user.MarkTrialD1EmailSent();
                    logger.LogInformation("E-mail D-1 enviado para {Email}", user.Email);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Falha ao enviar e-mail de trial para {Email}", user.Email);
                // Não marca como enviado — tentará novamente amanhã
            }
        }

        if (usuarios.Any(u => u.TrialD7EmailSent || u.TrialD1EmailSent))
            await db.SaveChangesAsync(ct);
    }

    private static string BuildD7Email(string nome, DateTime expira) => $"""
        <div style="font-family:sans-serif;max-width:560px;margin:0 auto;background:#0f1117;color:#e2e8f0;border-radius:12px;overflow:hidden">
          <div style="background:#16a34a;padding:32px 24px;text-align:center">
            <p style="font-size:48px;margin:0">🐾</p>
            <h1 style="color:#fff;font-size:22px;margin:12px 0 4px">Meu FinDog</h1>
            <p style="color:#bbf7d0;margin:0;font-size:14px">Seu parceiro financeiro</p>
          </div>
          <div style="padding:32px 24px">
            <p style="font-size:18px;font-weight:700;color:#f1f5f9">Olá, {nome}! 👋</p>
            <p style="color:#94a3b8;line-height:1.6">
              Seu período de trial gratuito termina em <strong style="color:#fbbf24">7 dias</strong>
              ({expira:dd/MM/yyyy}). Não perca acesso às suas finanças!
            </p>
            <div style="background:#1e293b;border-radius:10px;padding:16px;margin:20px 0">
              <p style="color:#94a3b8;font-size:13px;margin:0 0 10px">O que você vai continuar tendo:</p>
              <p style="margin:6px 0;color:#e2e8f0;font-size:14px">✅ Lançamentos ilimitados</p>
              <p style="margin:6px 0;color:#e2e8f0;font-size:14px">✅ Integração com WhatsApp</p>
              <p style="margin:6px 0;color:#e2e8f0;font-size:14px">✅ Relatórios e gráficos</p>
              <p style="margin:6px 0;color:#e2e8f0;font-size:14px">✅ Metas financeiras</p>
            </div>
            <div style="text-align:center;margin:28px 0">
              <a href="https://app.findog.com.br/planos"
                 style="background:#16a34a;color:#fff;text-decoration:none;padding:14px 32px;border-radius:10px;font-weight:700;font-size:15px;display:inline-block">
                Ver planos — a partir de R$ 4,90/mês
              </a>
            </div>
            <p style="color:#64748b;font-size:12px;text-align:center;margin-top:24px">
              Meu FinDog · <a href="https://app.findog.com.br" style="color:#64748b">app.findog.com.br</a>
            </p>
          </div>
        </div>
        """;

    private static string BuildD1Email(string nome, DateTime expira) => $"""
        <div style="font-family:sans-serif;max-width:560px;margin:0 auto;background:#0f1117;color:#e2e8f0;border-radius:12px;overflow:hidden">
          <div style="background:#dc2626;padding:32px 24px;text-align:center">
            <p style="font-size:48px;margin:0">🚨</p>
            <h1 style="color:#fff;font-size:22px;margin:12px 0 4px">Último dia de trial!</h1>
            <p style="color:#fecaca;margin:0;font-size:14px">Seu acesso expira amanhã ({expira:dd/MM/yyyy})</p>
          </div>
          <div style="padding:32px 24px">
            <p style="font-size:18px;font-weight:700;color:#f1f5f9">Oi, {nome}!</p>
            <p style="color:#94a3b8;line-height:1.6">
              Amanhã é o último dia do seu trial gratuito. Assine agora para não perder acesso
              aos seus lançamentos, metas e relatórios.
            </p>
            <div style="background:#7f1d1d22;border:1px solid #dc2626;border-radius:10px;padding:16px;margin:20px 0;text-align:center">
              <p style="color:#fca5a5;font-weight:700;font-size:16px;margin:0">
                ⏰ Seu trial expira em menos de 24 horas
              </p>
            </div>
            <div style="text-align:center;margin:28px 0">
              <a href="https://app.findog.com.br/planos"
                 style="background:#16a34a;color:#fff;text-decoration:none;padding:14px 32px;border-radius:10px;font-weight:700;font-size:15px;display:inline-block">
                🐾 Assinar agora — R$ 4,90/mês
              </a>
            </div>
            <p style="color:#94a3b8;font-size:13px;line-height:1.6;text-align:center">
              Cancele quando quiser. Seus dados ficam salvos por 30 dias após o cancelamento.
            </p>
            <p style="color:#64748b;font-size:12px;text-align:center;margin-top:24px">
              Meu FinDog · <a href="https://app.findog.com.br" style="color:#64748b">app.findog.com.br</a>
            </p>
          </div>
        </div>
        """;
}
