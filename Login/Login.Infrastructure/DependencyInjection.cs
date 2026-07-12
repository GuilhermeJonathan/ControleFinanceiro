using Login.Application.Common.Interfaces;
using Login.Domain.Common;
using Login.Domain.Repositories;
using Login.Infrastructure.Persistence;
using Login.Infrastructure.Persistence.Repositories;
using Login.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Login.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                o => o.EnableRetryOnFailure(3));
            // Falso positivo do EF quando o snapshot foi gerado por tooling de versão
            // diferente do runtime — o diff real é vazio (verificado via migration vazia)
            options.ConfigureWarnings(w =>
                w.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        // UnitOfWork
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

        // Repositórios
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProfileRepository, ProfileRepository>();
        services.AddScoped<IModuleRepository, ModuleRepository>();
        services.AddScoped<ITermRepository, TermRepository>();
        services.AddScoped<IInviteRepository, InviteRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();

        // Mercado Pago
        services.AddHttpClient<IMercadoPagoService, MercadoPagoService>();

        // Serviços cross-cutting
        services.AddScoped<ITokenManager, JwtTokenManager>();
        services.AddScoped<IResetTokenManager, ResetTokenManager>();
        services.AddScoped<ICryptography, BcryptCryptography>();
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }
}
