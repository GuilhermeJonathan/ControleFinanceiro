using System.Reflection;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Persistence;

public class AppDbContext : DbContext, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Lancamento> Lancamentos => Set<Lancamento>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<CartaoCredito> CartoesCredito => Set<CartaoCredito>();
    public DbSet<ParcelaCartao> ParcelasCartao => Set<ParcelaCartao>();
    public DbSet<SaldoConta> SaldosContas => Set<SaldoConta>();
    public DbSet<HorasTrabalhadas> HorasTrabalhadas => Set<HorasTrabalhadas>();
    public DbSet<ReceitaRecorrente> ReceitasRecorrentes => Set<ReceitaRecorrente>();
    public DbSet<VinculoFamiliar> VinculosFamiliares => Set<VinculoFamiliar>();
    public DbSet<VinculoAssessoria> VinculosAssessoria => Set<VinculoAssessoria>();
    public DbSet<VinculoCorretor> VinculosCorretor => Set<VinculoCorretor>();
    public DbSet<DelegacaoCarteira> DelegacoesCarteira => Set<DelegacaoCarteira>();
    public DbSet<Recomendacao> Recomendacoes => Set<Recomendacao>();
    public DbSet<AtivoPatrimonial> AtivosPatrimoniais => Set<AtivoPatrimonial>();
    public DbSet<PassivoPatrimonial> PassivosPatrimoniais => Set<PassivoPatrimonial>();
    public DbSet<SimulacaoPatrimonial> SimulacoesPatrimoniais => Set<SimulacaoPatrimonial>();
    public DbSet<PatrimonioSnapshot> PatrimonioSnapshots => Set<PatrimonioSnapshot>();
    public DbSet<AlocacaoAlvo> AlocacoesAlvo => Set<AlocacaoAlvo>();
    public DbSet<PlanoAcao> PlanosAcao => Set<PlanoAcao>();
    public DbSet<ParametrosSaude> ParametrosSaude => Set<ParametrosSaude>();
    public DbSet<Investimento> Investimentos => Set<Investimento>();
    public DbSet<TipoAtivoParam> TiposAtivoParam => Set<TipoAtivoParam>();
    public DbSet<TipoInvestimentoParam> TiposInvestimentoParam => Set<TipoInvestimentoParam>();
    public DbSet<MoedaParam> MoedasParam => Set<MoedaParam>();
    public DbSet<CotacaoHistorico> CotacoesHistorico => Set<CotacaoHistorico>();
    public DbSet<PrecoAtivoHistorico> PrecosAtivoHistorico => Set<PrecoAtivoHistorico>();
    public DbSet<ConsultoriaConfig> ConsultoriaConfigs => Set<ConsultoriaConfig>();
    public DbSet<Meta> Metas => Set<Meta>();
    public DbSet<WhatsAppVinculo> WhatsAppVinculos => Set<WhatsAppVinculo>();
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<Venda> Vendas => Set<Venda>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }

    public new async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await base.SaveChangesAsync(cancellationToken);
    }
}
