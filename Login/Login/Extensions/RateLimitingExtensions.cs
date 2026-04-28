using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Login.Extensions;

public static class RateLimitingExtensions
{
    /// <summary>
    /// Registra as políticas de rate limiting da API de autenticação.
    /// <list type="bullet">
    ///   <item><b>login</b> — 5 tentativas por IP a cada 15 min (POST /user/authenticate)</item>
    ///   <item><b>register</b> — 5 tentativas por IP por hora (register, forgotPassword)</item>
    /// </list>
    /// </summary>
    public static IServiceCollection AddLoginRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddPolicy("login", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetClientIp(context),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit          = 5,
                        Window               = TimeSpan.FromMinutes(15),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit           = 0,
                    }));

            options.AddPolicy("register", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetClientIp(context),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit          = 5,
                        Window               = TimeSpan.FromHours(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit           = 0,
                    }));

            options.OnRejected = async (ctx, ct) =>
            {
                ctx.HttpContext.Response.StatusCode          = StatusCodes.Status429TooManyRequests;
                ctx.HttpContext.Response.ContentType         = "application/json";
                ctx.HttpContext.Response.Headers["Retry-After"] = "900";
                await ctx.HttpContext.Response.WriteAsync(
                    "{\"error\":\"Muitas tentativas. Aguarde 15 minutos antes de tentar novamente.\"}",
                    ct);
            };
        });

        return services;
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Retorna o IP real do cliente, respeitando o header X-Forwarded-For
    /// quando a API está atrás de um proxy reverso (Render, nginx, etc.).
    /// </summary>
    private static string GetClientIp(HttpContext ctx) =>
        ctx.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',')[0].Trim()
        ?? ctx.Connection.RemoteIpAddress?.ToString()
        ?? "unknown";
}
