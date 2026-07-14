namespace ControleFinanceiro.Domain.Entities;

/// <summary>
/// Vínculo entre um Assessor (dono) e um Corretor (subordinado).
/// O corretor aceita o convite com código de 6 dígitos — mesmo fluxo do cliente.
/// Após aceito, o assessor pode delegar carteiras de clientes ao corretor.
/// </summary>
public class VinculoCorretor
{
    public Guid Id                { get; private set; } = Guid.NewGuid();
    public Guid AssessorId        { get; private set; }
    public Guid CorretorId        { get; private set; }   // preenchido no aceite
    public string CodigoConvite   { get; private set; } = string.Empty;
    public DateTime CriadoEm      { get; private set; } = DateTime.UtcNow;
    public DateTime? AceitoEm     { get; private set; }
    public DateTime? RevogadoEm   { get; private set; }
    public string? NomeCorretor   { get; private set; }
    public string? NomeAssessor   { get; private set; }

    public bool Ativo => AceitoEm != null && RevogadoEm == null;

    private VinculoCorretor() { }

    public static VinculoCorretor Criar(Guid assessorId, string codigo, string? nomeAssessor = null) =>
        new() { AssessorId = assessorId, CodigoConvite = codigo.ToUpperInvariant(), NomeAssessor = nomeAssessor };

    public void Aceitar(Guid corretorId, string nomeCorretor)
    {
        if (AceitoEm != null)   throw new InvalidOperationException("Convite já utilizado.");
        if (corretorId == AssessorId) throw new InvalidOperationException("Assessor não pode ser corretor de si mesmo.");
        CorretorId   = corretorId;
        NomeCorretor = nomeCorretor;
        AceitoEm     = DateTime.UtcNow;
    }

    public void Revogar()
    {
        if (RevogadoEm != null) throw new InvalidOperationException("Vínculo já revogado.");
        RevogadoEm = DateTime.UtcNow;
    }
}
