namespace ControleFinanceiro.Domain.Entities;

/// <summary>
/// Identidade/marca da consultoria de um assessor (1 por assessor).
/// Usada no relatório PDF (logo + nome + cor) e no card "Seu consultor" do cliente.
/// </summary>
public class ConsultoriaConfig
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UsuarioId { get; private set; }   // assessor dono
    public string NomeConsultoria { get; private set; } = string.Empty;
    /// <summary>Logo em data URL (base64). Null = sem logo.</summary>
    public string? LogoBase64 { get; private set; }
    /// <summary>Cor da marca em hex (ex.: "#16a34a").</summary>
    public string? CorMarca { get; private set; }
    /// <summary>WhatsApp de contato exibido ao cliente.</summary>
    public string? WhatsApp { get; private set; }
    /// <summary>Mensagem/disclaimer que aparece no rodapé do relatório.</summary>
    public string? MensagemRodape { get; private set; }
    public DateTime AtualizadoEm { get; private set; } = DateTime.UtcNow;

    private ConsultoriaConfig() { }

    public ConsultoriaConfig(Guid usuarioId, string nomeConsultoria,
        string? logoBase64, string? corMarca, string? whatsApp, string? mensagemRodape)
    {
        UsuarioId = usuarioId;
        Atualizar(nomeConsultoria, logoBase64, corMarca, whatsApp, mensagemRodape);
    }

    public void Atualizar(string nomeConsultoria, string? logoBase64,
        string? corMarca, string? whatsApp, string? mensagemRodape)
    {
        NomeConsultoria = nomeConsultoria;
        LogoBase64 = logoBase64;
        CorMarca = corMarca;
        WhatsApp = whatsApp;
        MensagemRodape = mensagemRodape;
        AtualizadoEm = DateTime.UtcNow;
    }
}
