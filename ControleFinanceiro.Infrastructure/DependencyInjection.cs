using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Repositories;
using ControleFinanceiro.Infrastructure.Persistence;
using ControleFinanceiro.Infrastructure.Persistence.Repositories;
using ControleFinanceiro.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), npgsql =>
            {
                npgsql.CommandTimeout(120);
                npgsql.EnableRetryOnFailure(maxRetryCount: 3);
            });
            // Falso positivo do EF quando o snapshot foi gerado por tooling de versão
            // diferente do runtime — o diff real é vazio (verificado via migration vazia)
            options.ConfigureWarnings(w =>
                w.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddScoped<ILancamentoRepository, LancamentoRepository>();
        services.AddScoped<ICategoriaRepository, CategoriaRepository>();
        services.AddScoped<ICartaoCreditoRepository, CartaoCreditoRepository>();
        services.AddScoped<IParcelaCartaoRepository, ParcelaCartaoRepository>();
        services.AddScoped<ISaldoContaRepository, SaldoContaRepository>();
        services.AddScoped<IHorasTrabalhadasRepository, HorasTrabalhadasRepository>();
        services.AddScoped<IReceitaRecorrenteRepository, ReceitaRecorrenteRepository>();
        services.AddScoped<IVinculoFamiliarRepository, VinculoFamiliarRepository>();
        services.AddScoped<IVinculoAssessoriaRepository, VinculoAssessoriaRepository>();
        services.AddScoped<IRecomendacaoRepository, RecomendacaoRepository>();
        services.AddScoped<IAtivoPatrimonialRepository, AtivoPatrimonialRepository>();
        services.AddScoped<IPassivoPatrimonialRepository, PassivoPatrimonialRepository>();
        services.AddScoped<ISimulacaoPatrimonialRepository, SimulacaoPatrimonialRepository>();
        services.AddScoped<IPatrimonioSnapshotRepository, PatrimonioSnapshotRepository>();
        services.AddScoped<IAlocacaoAlvoRepository, AlocacaoAlvoRepository>();
        services.AddScoped<IPlanoAcaoRepository, PlanoAcaoRepository>();
        services.AddScoped<IParametrosSaudeRepository, ParametrosSaudeRepository>();
        services.AddScoped<IInvestimentoRepository, InvestimentoRepository>();
        services.AddScoped<ITipoAtivoParamRepository, TipoAtivoParamRepository>();
        services.AddScoped<ITipoInvestimentoParamRepository, TipoInvestimentoParamRepository>();
        services.AddScoped<IMoedaParamRepository, MoedaParamRepository>();
        services.AddScoped<IParametroOcultoRepository, ParametroOcultoRepository>();
        services.AddScoped<IAssessoriaOwnerResolver, AssessoriaOwnerResolver>();
        services.AddScoped<ICotacaoHistoricoRepository, CotacaoHistoricoRepository>();
        services.AddScoped<IPrecoAtivoHistoricoRepository, PrecoAtivoHistoricoRepository>();
        services.AddScoped<IConsultoriaConfigRepository, ConsultoriaConfigRepository>();
        services.AddScoped<IMetaRepository, MetaRepository>();
        services.AddScoped<IWhatsAppVinculoRepository, WhatsAppVinculoRepository>();
        services.AddScoped<IProdutoRepository, ProdutoRepository>();
        services.AddScoped<IVendaRepository, VendaRepository>();
        services.AddScoped<IVinculoCorretorRepository, VinculoCorretorRepository>();
        services.AddScoped<IDelegacaoCarteiraRepository, DelegacaoCarteiraRepository>();

        services.AddScoped<IUserNameLookup, UserNameLookupService>();
        // E-mail centralizado: a API de Patrimônio não envia direto pelo Resend —
        // o gateway repassa para a API de Login (único sender real do ecossistema).
        services.AddHttpClient<IEmailService, LoginEmailGateway>();
        services.AddHttpClient<ILoginProvisionClient, LoginProvisionClient>();
        services.AddHttpClient<ICurrencyRateService, AwesomeApiCurrencyRateService>();
        services.AddHttpClient<IAssetPriceService, BrapiAssetPriceService>();
        services.AddScoped<ControleFinanceiro.Application.Relatorios.IRelatorioPatrimonialGenerator, RelatorioPatrimonialGenerator>();

        // Nota: MetaContribuicaoService (background) é registrado no Program.cs,
        // gateado por IsProduction() junto com os demais processos.

        return services;
    }
}
