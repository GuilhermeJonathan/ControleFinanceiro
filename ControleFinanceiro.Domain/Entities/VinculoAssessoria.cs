namespace ControleFinanceiro.Domain.Entities;

/// <summary>
/// Vínculo entre um assessor financeiro (UserType.Assessor no Login) e um cliente.
/// Diferente do VinculoFamiliar, dá ao assessor acesso SOMENTE LEITURA aos dados
/// do cliente, via header X-Assessoria-Cliente (AssessoriaContextMiddleware).
/// </summary>
public class VinculoAssessoria
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid AssessorId { get; private set; }
    public Guid ClienteId { get; private set; }        // preenchido no aceite
    public string CodigoConvite { get; private set; } = string.Empty;
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;
    public DateTime? AceitoEm { get; private set; }    // null = convite pendente
    public DateTime? RevogadoEm { get; private set; }  // preenchido = acesso encerrado
    public string? NomeCliente { get; private set; }   // guardado no aceite para exibir na UI
    public string? NomeAssessor { get; private set; }  // guardado na criação para exibir ao cliente
    public string? EmailConvidado { get; private set; } // preenchido quando o convite é enviado por e-mail

    public bool Ativo => AceitoEm != null && RevogadoEm == null;

    private VinculoAssessoria() { }

    public static VinculoAssessoria Criar(Guid assessorId, string codigo, string? nomeAssessor = null, string? emailConvidado = null) =>
        new()
        {
            AssessorId = assessorId,
            CodigoConvite = codigo.ToUpperInvariant(),
            NomeAssessor = nomeAssessor,
            EmailConvidado = string.IsNullOrWhiteSpace(emailConvidado) ? null : emailConvidado.Trim().ToLowerInvariant(),
        };

    public void Aceitar(Guid clienteId, string nomeCliente)
    {
        if (AceitoEm != null) throw new InvalidOperationException("Convite já utilizado.");
        if (clienteId == AssessorId) throw new InvalidOperationException("Assessor não pode ser cliente de si mesmo.");
        ClienteId = clienteId;
        NomeCliente = nomeCliente;
        AceitoEm = DateTime.UtcNow;
    }

    public void Revogar()
    {
        if (RevogadoEm != null) throw new InvalidOperationException("Vínculo já revogado.");
        RevogadoEm = DateTime.UtcNow;
    }
}
