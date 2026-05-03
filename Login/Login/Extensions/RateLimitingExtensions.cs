using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Login.Extensions;

public static class RateLimitingExtensions
{
    /// <summary>
    /// Registra as políticas de rate limiting da API de autenticação.
    /// <list type="bullet">
    ///   <item><b>login</b> — 10 req/min por IP (POST /user/authenticate)</item>
    ///   <item><b>register</b> — 5 req/min por IP (POST /user/selfregister, POST /user/forgotPassword)</item>
    /// </list>
    /// </summary>
    public static IServiceCollection AddLoginRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // POST /user/authenticate — 10 requisições por minuto por IP
            options.AddPolicy("login", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetClientIp(context),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit          = 10,
                        Window               = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit           = 0,
                    }));

            // POST /user/selfregister e POST /user/forgotPassword — 5 req/min por IP
            options.AddPolicy("register", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetClientIp(context),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit          = 5,
                        Window               = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit           = 0,
                    }));

            options.OnRejected = async (ctx, ct) =>
            {
                ctx.HttpContext.Response.StatusCode          = StatusCodes.Status429TooManyRequests;
                ctx.HttpContext.Response.ContentType         = "application/json";
                ctx.HttpContext.Response.Headers["Retry-After"] = "60";
                await ctx.HttpContext.Response.WriteAsync(
                    "{\"error\":\"Muitas tentativas. Tente novamente em breve.\"}",
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
