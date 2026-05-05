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

    public async Task<(IEnumerable<Lancamento> Itens, int TotalCount)> GetPagedByMesAnoAsync(
        int mes, int ano, Guid usuarioId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = context.Lancamentos
            .Include(l => l.Categoria)
            .Include(l => l.Cartao)
            .Include(l => l.ReceitaRecorrente)
            .Include(l => l.ContaBancaria)
            .Where(l => l.Mes == mes && l.Ano == ano && l.UsuarioId == usuarioId)
            .OrderBy(l => l.Data);

        var total = await query.CountAsync(cancellationToken);
        var itens = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (itens, total);
    }

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

    public async Task<IEnumerable<Lancamento>> GetByGrupoParcelasAsync(
        Guid grupoParcelas, Guid usuarioId, CancellationToken cancellationToken = default)
        => await context.Lancamentos
            .Where(l => l.GrupoParcelas == grupoParcelas && l.UsuarioId == usuarioId)
            .ToListAsync(cancellationToken);

    public async Task<decimal> GetSaldoAcumuladoAsync(int mes, int ano, Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var ate = ano * 100 + mes;

        var creditos = await context.Lancamentos
            .Where(l => l.UsuarioId == usuarioId
                && (l.Ano * 100 + l.Mes) <= ate
                && l.Tipo == Domain.Enums.TipoLancamento.Credito)
            .SumAsync(l => l.Valor, cancellationToken);

        var debitos = await context.Lancamentos
            .Where(l => l.UsuarioId == usuarioId
                && (l.Ano * 100 + l.Mes) <= ate
                && (l.Tipo == Domain.Enums.TipoLancamento.Debito || l.Tipo == Domain.Enums.TipoLancamento.Pix))
            .SumAsync(l => l.Valor, cancellationToken);

        return creditos - debitos;
    }

    public async Task<List<Lancamento>> GetRecorrentesAsync(Guid usuarioId, CancellationToken cancellationToken = default)
        => await context.Lancamentos
            .Include(l => l.Categoria)
            .Where(l => l.UsuarioId == usuarioId
                && l.IsRecorrente
                && (l.Tipo == Domain.Enums.TipoLancamento.Debito || l.Tipo == Domain.Enums.TipoLancamento.Pix))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public async Task<(IEnumerable<Lancamento> Itens, int TotalCount)> SearchAsync(
        string q, int page, int pageSize, Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var pattern = $"%{q}%";

        // ILike = ILIKE do PostgreSQL (case-insensitive)
        // Busca em: descrição, nome da categoria e nome do cartão
        var query = context.Lancamentos
            .Include(l => l.Categoria)
            .Include(l => l.Cartao)
            .Where(l => l.UsuarioId == usuarioId && (
                EF.Functions.ILike(l.Descricao, pattern) ||
                (l.Categoria != null && EF.Functions.ILike(l.Categoria.Nome, pattern)) ||
                (l.Cartao    != null && EF.Functions.ILike(l.Cartao.Nome,    pattern))
            ))
            .OrderBy(l =>
                l.Situacao == Domain.Enums.SituacaoLancamento.Vencido  ? 0 :
                l.Situacao == Domain.Enums.SituacaoLancamento.AVencer  ? 1 :
                l.Situacao == Domain.Enums.SituacaoLancamento.AReceber ? 2 : 3)
            .ThenBy(l => l.Data);

        var total = await query.CountAsync(cancellationToken);
        var itens = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (itens, total);
    }

    public async Task NullCategoriaAsync(Guid categoriaId, CancellationToken cancellationToken = default)
        => await context.Lancamentos
            .Where(l => l.CategoriaId == categoriaId)
            .ExecuteUpdateAsync(s => s.SetProperty(l => l.CategoriaId, (Guid?)null), cancellationToken);
}
