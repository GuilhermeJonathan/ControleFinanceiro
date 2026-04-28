using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Repositories;
using ControleFinanceiro.Infrastructure.Persistence;
using ControleFinanceiro.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ControleFinanceiro.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configura comportamento legacy do timestamp para aceitar DateTime com qualquer Kind
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), npgsql =>
            {
                npgsql.CommandTimeout(120);
                npgsql.EnableRetryOnFailure(maxRetryCount: 3);
            }));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddScoped<ILancamentoRepository, LancamentoRepository>();
        services.AddScoped<ICategoriaRepository, CategoriaRepository>();
        services.AddScoped<ICartaoCreditoRepository, CartaoCreditoRepository>();
        services.AddScoped<IParcelaCartaoRepository, ParcelaCartaoRepository>();
        services.AddScoped<ISaldoContaRepository, SaldoContaRepository>();
        services.AddScoped<IHorasTrabalhadasRepository, HorasTrabalhadasRepository>();
        services.AddScoped<IReceitaRecorrenteRepository, ReceitaRecorrenteRepository>();
        services.AddScoped<IVinculoFamiliarRepository, VinculoFamiliarRepository>();
        services.AddScoped<IMetaRepository, MetaRepository>();
        services.AddScoped<IWhatsAppVinculoRepository, WhatsAppVinculoRepository>();

        return services;
    }
}
