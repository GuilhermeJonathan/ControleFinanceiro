using Login.Application.Common.Email;
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

    private static string BuildEmail(string nome) =>
        EmailTemplateBuilder.Wrap(
            EmailTemplateBuilder.Greeting($"Oi, {nome}! 👋") +
            EmailTemplateBuilder.Paragraph(
                """Faz 30 dias que você não usa o <strong style="color:#e2e8f0">Meu FinDog</strong>. Suas finanças estão com saudade de você!""") +
            EmailTemplateBuilder.Card("""
                <p style="color:#94a3b8;font-size:13px;margin:0 0 10px">O que está te esperando:</p>
                <p style="margin:6px 0;color:#e2e8f0;font-size:14px">📊 Visão completa dos seus gastos</p>
                <p style="margin:6px 0;color:#e2e8f0;font-size:14px">🎯 Metas financeiras no seu ritmo</p>
                <p style="margin:6px 0;color:#e2e8f0;font-size:14px">💳 Controle de cartões e parcelas</p>
                <p style="margin:6px 0;color:#e2e8f0;font-size:14px">📱 Lançamentos pelo WhatsApp</p>
                """) +
            EmailTemplateBuilder.Button("Voltar a organizar minha vida 🐶", $"{EmailTemplateBuilder.AppUrl}/planos") +
            """
            <p style="color:#94a3b8;font-size:13px;line-height:1.6;text-align:center">
              Planos a partir de <strong style="color:#e2e8f0">R$ 4,90/mês</strong>. Cancele quando quiser.
            </p>
            """);
}
