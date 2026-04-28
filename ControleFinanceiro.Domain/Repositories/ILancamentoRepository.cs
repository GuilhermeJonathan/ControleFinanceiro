using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Repositories;

public interface ILancamentoRepository
{
    Task<Lancamento?> GetByIdAsync(Guid id, Guid usuarioId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Lancamento>> GetByMesAnoAsync(int mes, int ano, Guid usuarioId, CancellationToken cancellationToken = default);
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
}
