using Login.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Login.Infrastructure.Services;

/// <summary>
/// Aquece o EF Core e o pool de conexão do PostgreSQL na inicialização,
/// eliminando a latência extra do primeiro login após o app ficar parado.
/// </summary>
public class WarmupService(
    IServiceScopeFactory scopeFactory,
    ILogger<WarmupService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Compila o modelo LINQ→SQL do EF Core e abre a primeira conexão do pool
            await db.Users.AsNoTracking().AnyAsync(cancellationToken);

            logger.LogInformation("Warmup: EF Core e pool de conexão prontos.");
        }
        catch (Exception ex)
        {
            // Não bloqueia a inicialização — apenas avisa
            logger.LogWarning(ex, "Warmup falhou — o primeiro login pode ser mais lento.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
