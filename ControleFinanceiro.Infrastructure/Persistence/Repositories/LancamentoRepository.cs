using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiro.Infrastructure.Persistence.Repositories;

public class LancamentoRepository(AppDbContext context) : ILancamentoRepository
{
    public async Task<Lancamento?> GetByIdAsync(Guid id, Guid usuarioId, CancellationToken cancellationToken = default)
        => await context.Lancamentos
            .Include(l => l.Categoria)
            .Include(l => l.Cartao)
            .FirstOrDefaultAsync(l => l.Id == id && l.UsuarioId == usuarioId, cancellationToken);

    public async Task<IEnumerable<Lancamento>> GetByMesAnoAsync(int mes, int ano, Guid usuarioId, CancellationToken cancellationToken = default)
        => await context.Lancamentos
            .Include(l => l.Categoria)
            .Include(l => l.Cartao)
            .Include(l => l.ReceitaRecorrente)
            .Include(l => l.ContaBancaria)
            .Where(l => l.Mes == mes && l.Ano == ano && l.UsuarioId == usuarioId)
            .OrderBy(l => l.Data)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Lancamento>> GetByCartaoMesAnoAsync(Guid cartaoId, int mes, int ano, Guid usuarioId, CancellationToken cancellationToken = default)
        => await context.Lancamentos
            .Include(l => l.Categoria)
            .Where(l => l.CartaoId == cartaoId && l.Mes == mes && l.Ano == ano && l.UsuarioId == usuarioId)
            .OrderBy(l => l.Data)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Lancamento lancamento, CancellationToken cancellationToken = default)
        => await context.Lancamentos.AddAsync(lancamento, cancellationToken);

    public async Task AddRangeAsync(IEnumerable<Lancamento> lancamentos, CancellationToken cancellationToken = default)
        => await context.Lancamentos.AddRangeAsync(lancamentos, cancellationToken);

    public void Update(Lancamento lancamento) => context.Lancamentos.Update(lancamento);

    public void Delete(Lancamento lancamento) => context.Lancamentos.Remove(lancamento);

    public async Task<IEnumerable<Lancamento>> GetFutureByReceitaRecorrenteIdAsync(
        Guid receitaRecorrenteId, int mesAtual, int anoAtual, Guid usuarioId, CancellationToken cancellationToken = default)
        => await context.Lancamentos
            .Where(l => l.ReceitaRecorrenteId == receitaRecorrenteId
                && l.UsuarioId == usuarioId
                && (l.Ano > anoAtual || (l.Ano == anoAtual && l.Mes >= mesAtual)))
            .ToListAsync(cancellationToken);

    public void DeleteRange(IEnumerable<Lancamento> lancamentos)
        => context.Lancamentos.RemoveRange(lancamentos);

    public async Task<IEnumerable<Lancamento>> GetParceladosVigentesAsync(Guid usuarioId, CancellationToken cancellationToken = default)
        => await context.Lancamentos
            .Include(l => l.Categoria)
            .Include(l => l.Cartao)
            .Where(l => l.UsuarioId == usuarioId
                && l.ParcelaAtual != null
                && !l.IsRecorrente
                && l.TotalParcelas < 120          // exclui recorrentes criados antes do flag existir
                && (l.Situacao == Domain.Enums.SituacaoLancamento.AVencer
                    || l.Situacao == Domain.Enums.SituacaoLancamento.Vencido))
            .OrderBy(l => l.Data)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Lancamento>> GetByAnoAsync(int ano, Guid usuarioId, CancellationToken cancellationToken = default)
        => await context.Lancamentos
            .Include(l => l.Categoria)
            .Where(l => l.Ano == ano && l.UsuarioId == usuarioId)
            .OrderBy(l => l.Mes).ThenBy(l => l.Data)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Lancamento>> GetProjecaoAsync(
        int mesInicio, int anoInicio, int mesFim, int anoFim,
        Guid usuarioId, CancellationToken cancellationToken = default)
    {
        // Converte para número inteiro AnoMes (ex: 202603) para comparação simples
        var de  = anoInicio * 100 + mesInicio;
        var ate = anoFim    * 100 + mesFim;

        return await context.Lancamentos
            .Where(l => l.UsuarioId == usuarioId
                && (l.Ano * 100 + l.Mes) >= de
                && (l.Ano * 100 + l.Mes) <= ate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Lancamento>> GetByGrupoParcelasFromAsync(
        Guid grupoParcelas, int parcelaAtualFrom, Guid usuarioId, CancellationToken cancellationToken = default)
        => await context.Lancamentos
            .Where(l => l.GrupoParcelas == grupoParcelas && l.ParcelaAtual >= parcelaAtualFrom && l.UsuarioId == usuarioId)
            .ToListAsync(cancellationToken);
}
