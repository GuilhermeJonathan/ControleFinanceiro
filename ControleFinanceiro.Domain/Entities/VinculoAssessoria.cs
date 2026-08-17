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
    public string? NomeCliente { get; private set; }   // guardado no aceite para exibir na UI (o assessor pode ajustar depois)
    public string? NomeAssessor { get; private set; }  // guardado na criação para exibir ao cliente
    public string? Telefone { get; private set; }      // contato (WhatsApp) que o assessor mantém do cliente
    public string? Observacoes { get; private set; }   // nota interna do assessor sobre o cliente (não visível ao cliente)
    public string? EmailConvidado { get; private set; } // preenchido quando o convite é enviado por e-mail
    public DateTime? ExpiraEm { get; private set; }      // convite pendente expira; null = sem expiração (legado)
    public DateTime? UltimoRelatorioMensalEm { get; private set; } // controle do e-mail mensal automático

    /// <summary>Prazo padrão de validade de um convite (dias).</summary>
    public const int PrazoConviteDias = 7;

    public bool Ativo => AceitoEm != null && RevogadoEm == null;
    public bool Expirado => AceitoEm == null && RevogadoEm == null && ExpiraEm is { } e && e < DateTime.UtcNow;

    private VinculoAssessoria() { }

    public static VinculoAssessoria Criar(Guid assessorId, string codigo, string? nomeAssessor = null, string? emailConvidado = null) =>
        new()
        {
            AssessorId = assessorId,
            CodigoConvite = codigo.ToUpperInvariant(),
            NomeAssessor = nomeAssessor,
            EmailConvidado = string.IsNullOrWhiteSpace(emailConvidado) ? null : emailConvidado.Trim().ToLowerInvariant(),
            ExpiraEm = DateTime.UtcNow.AddDays(PrazoConviteDias),
        };

    public void Aceitar(Guid clienteId, string nomeCliente)
    {
        if (AceitoEm != null) throw new InvalidOperationException("Convite já utilizado.");
        if (Expirado)         throw new InvalidOperationException("Convite expirado. Peça um novo ao assessor.");
        if (clienteId == AssessorId) throw new InvalidOperationException("Assessor não pode ser cliente de si mesmo.");
        ClienteId = clienteId;
        NomeCliente = nomeCliente;
        AceitoEm = DateTime.UtcNow;
    }

    /// <summary>
    /// Assessor ajusta os dados de contato que mantém do cliente: nome de exibição,
    /// telefone/WhatsApp e uma nota interna. Não altera os dados de login do cliente.
    /// </summary>
    public void AtualizarContato(string? nomeCliente, string? telefone, string? observacoes)
    {
        static string? Limpar(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();
        var nome = Limpar(nomeCliente);
        if (nome != null) NomeCliente = nome; // nunca apaga o nome capturado no aceite
        Telefone = Limpar(telefone);
        Observacoes = Limpar(observacoes);
    }

    /// <summary>Marca que o relatório mensal foi enviado (evita reenvio no mesmo mês).</summary>
    public void MarcarRelatorioMensalEnviado() => UltimoRelatorioMensalEm = DateTime.UtcNow;

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
