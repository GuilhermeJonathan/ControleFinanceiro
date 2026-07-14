namespace ControleFinanceiro.Domain.Entities;

/// <summary>
/// Delegação de um cliente (VinculoAssessoria) a um corretor subordinado.
/// Apenas leitura — o corretor visualiza, nunca edita.
/// Histórico preservado: RevogadoEm preenche em vez de deletar.
/// </summary>
public class DelegacaoCarteira
{
    public Guid Id                   { get; private set; } = Guid.NewGuid();
    public Guid AssessorId           { get; private set; }  // dono
    public Guid CorretorId           { get; private set; }  // quem recebeu a delegação
    public Guid VinculoAssessoriaId  { get; private set; }  // qual cliente
    public Guid ClienteId            { get; private set; }  // atalho para queries
    public string? NomeCliente       { get; private set; }
    public string? NomeCorretor      { get; private set; }
    public DateTime DelegadoEm       { get; private set; } = DateTime.UtcNow;
    public DateTime? RevogadoEm      { get; private set; }

    public bool Ativa => RevogadoEm == null;

    private DelegacaoCarteira() { }

    public static DelegacaoCarteira Criar(
        Guid assessorId, Guid corretorId,
        Guid vinculoAssessoriaId, Guid clienteId,
        string? nomeCliente, string? nomeCorretor) =>
        new()
        {
            AssessorId          = assessorId,
            CorretorId          = corretorId,
            VinculoAssessoriaId = vinculoAssessoriaId,
            ClienteId           = clienteId,
            NomeCliente         = nomeCliente,
            NomeCorretor        = nomeCorretor,
        };

    public void Revogar()
    {
        if (RevogadoEm != null) throw new InvalidOperationException("Delegação já revogada.");
        RevogadoEm = DateTime.UtcNow;
    }
}
