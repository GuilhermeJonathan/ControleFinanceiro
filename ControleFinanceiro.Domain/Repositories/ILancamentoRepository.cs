using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Repositories;

public interface ILancamentoRepository
{
    Task<Lancamento?> GetByIdAsync(Guid id, Guid usuarioId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Lancamento>> GetByMesAnoAsync(int mes, int ano, Guid usuarioId, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Lancamento> Itens, int TotalCount)> GetPagedByMesAnoAsync(int mes, int ano, Guid usuarioId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IEnumerable<Lancamento>> GetByCartaoMesAnoAsync(Guid cartaoId, int mes, int ano, Guid usuarioId, CancellationToken cancellationToken = default);
    Task AddAsync(Lancamento lancamento, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<Lancamento> lancamentos, CancellationToken cancellationToken = default);
    void Update(Lancamento lancamento);
    void Delete(Lancamento lancamento);
    Task<IEnumerable<Lancamento>> GetFutureByReceitaRecorrenteIdAsync(Guid receitaRecorrenteId, int mesAtual, int anoAtual, Guid usuarioId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Lancamento>> GetByGrupoParcelasFromAsync(Guid grupoParcelas, int parcelaAtualFrom, Guid usuarioId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Lancamento>> GetByGrupoParcelasAsync(Guid grupoParcelas, Guid usuarioId, CancellationToken cancellationToken = default);
    void DeleteRange(IEnumerable<Lancamento> lancamentos);
    Task<IEnumerable<Lancamento>> GetParceladosVigentesAsync(Guid usuarioId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Lancamento>> GetByAnoAsync(int ano, Guid usuarioId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Lancamento>> GetProjecaoAsync(int mesInicio, int anoInicio, int mesFim, int anoFim, Guid usuarioId, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Lancamento> Itens, int TotalCount)> SearchAsync(string q, int page, int pageSize, Guid usuarioId, CancellationToken cancellationToken = default);
    /// <summary>Retorna créditos − débitos de todos os lançamentos até (e incluindo) o mês/ano informado.</summary>
    Task<decimal> GetSaldoAcumuladoAsync(int mes, int ano, Guid usuarioId, CancellationToken cancellationToken = default);
    /// <summary>Retorna todos os lançamentos recorrentes de débito do usuário (assinaturas).</summary>
    Task<List<Lancamento>> GetRecorrentesAsync(Guid usuarioId, CancellationToken cancellationToken = default);
    Task NullCategoriaAsync(Guid categoriaId, CancellationToken cancellationToken = default);
}
