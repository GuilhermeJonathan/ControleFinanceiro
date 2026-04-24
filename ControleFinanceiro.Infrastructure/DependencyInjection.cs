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
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddScoped<ILancamentoRepository, LancamentoRepository>();
        services.AddScoped<ICategoriaRepository, CategoriaRepository>();
        services.AddScoped<ICartaoCreditoRepository, CartaoCreditoRepository>();
        services.AddScoped<IParcelaCartaoRepository, ParcelaCartaoRepository>();
        services.AddScoped<ISaldoContaRepository, SaldoContaRepository>();
        services.AddScoped<IHorasTrabalhadasRepository, HorasTrabalhadasRepository>();
        services.AddScoped<IReceitaRecorrenteRepository, ReceitaRecorrenteRepository>();

        return services;
    }
}
