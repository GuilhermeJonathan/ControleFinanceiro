using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Domain.Repositories;

public interface ILancamentoRepository
{
    Task<Lancamento?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Lancamento>> GetByMesAnoAsync(int mes, int ano, CancellationToken cancellationToken = default);
    Task<IEnumerable<Lancamento>> GetByCartaoMesAnoAsync(Guid cartaoId, int mes, int ano, CancellationToken cancellationToken = default);
    Task AddAsync(Lancamento lancamento, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<Lancamento> lancamentos, CancellationToken cancellationToken = default);
    void Update(Lancamento lancamento);
    void Delete(Lancamento lancamento);
    Task<IEnumerable<Lancamento>> GetFutureByReceitaRecorrenteIdAsync(Guid receitaRecorrenteId, int mesAtual, int anoAtual, CancellationToken cancellationToken = default);
    Task<IEnumerable<Lancamento>> GetByGrupoParcelasFromAsync(Guid grupoParcelas, int parcelaAtualFrom, CancellationToken cancellationToken = default);
    void DeleteRange(IEnumerable<Lancamento> lancamentos);
}
