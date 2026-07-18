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
    public string? EmailConvidado { get; private set; } // preenchido quando o convite é enviado por e-mail
    public DateTime? ExpiraEm     { get; private set; } // convite pendente expira; null = sem expiração (legado)

    /// <summary>Prazo padrão de validade de um convite (dias).</summary>
    public const int PrazoConviteDias = 7;

    public bool Ativo => AceitoEm != null && RevogadoEm == null;
    public bool Expirado => AceitoEm == null && RevogadoEm == null && ExpiraEm is { } e && e < DateTime.UtcNow;

    private VinculoCorretor() { }

    public static VinculoCorretor Criar(Guid assessorId, string codigo, string? nomeAssessor = null, string? emailConvidado = null) =>
        new()
        {
            AssessorId = assessorId,
            CodigoConvite = codigo.ToUpperInvariant(),
            NomeAssessor = nomeAssessor,
            EmailConvidado = string.IsNullOrWhiteSpace(emailConvidado) ? null : emailConvidado.Trim().ToLowerInvariant(),
            ExpiraEm = DateTime.UtcNow.AddDays(PrazoConviteDias),
        };

    public void Aceitar(Guid corretorId, string nomeCorretor)
    {
        if (AceitoEm != null)   throw new InvalidOperationException("Convite já utilizado.");
        if (Expirado)           throw new InvalidOperationException("Convite expirado. Peça um novo ao assessor.");
        if (corretorId == AssessorId) throw new InvalidOperationException("Assessor não pode ser corretor de si mesmo.");
        CorretorId   = corretorId;
        NomeCorretor = nomeCorretor;
        AceitoEm     = DateTime.UtcNow;
    }

    /// <summary>Reenvia o convite: renova a validade a partir de agora.</summary>
    public void RenovarValidade()
    {
        if (AceitoEm != null)   throw new InvalidOperationException("Convite já utilizado.");
        if (RevogadoEm != null) throw new InvalidOperationException("Convite revogado.");
        ExpiraEm = DateTime.UtcNow.AddDays(PrazoConviteDias);
    }

    public void Revogar()
    {
        if (RevogadoEm != null) throw new InvalidOperationException("Vínculo já revogado.");
        RevogadoEm = DateTime.UtcNow;
    }
}
