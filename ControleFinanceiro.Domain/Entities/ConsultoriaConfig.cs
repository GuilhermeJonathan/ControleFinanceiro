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
    /// <summary>Slug/rota do login whitelabel (ex.: "aurea-capital"). Único quando preenchido.
    /// O cliente acessa via /login?a={slug}. Null = só acessível pelo Guid do assessor.</summary>
    public string? Slug { get; private set; }
    public DateTime AtualizadoEm { get; private set; } = DateTime.UtcNow;

    private ConsultoriaConfig() { }

    public ConsultoriaConfig(Guid usuarioId, string nomeConsultoria,
        string? logoBase64, string? corMarca, string? whatsApp, string? mensagemRodape, string? slug = null)
    {
        UsuarioId = usuarioId;
        Atualizar(nomeConsultoria, logoBase64, corMarca, whatsApp, mensagemRodape, slug);
    }

    public void Atualizar(string nomeConsultoria, string? logoBase64,
        string? corMarca, string? whatsApp, string? mensagemRodape, string? slug = null)
    {
        NomeConsultoria = nomeConsultoria;
        LogoBase64 = logoBase64;
        CorMarca = corMarca;
        WhatsApp = whatsApp;
        MensagemRodape = mensagemRodape;
        Slug = NormalizarSlug(slug);
        AtualizadoEm = DateTime.UtcNow;
    }

    /// <summary>Normaliza o slug: minúsculo, sem acento/espaço, só [a-z0-9-]. Null se vazio.</summary>
    public static string? NormalizarSlug(string? slug)
    {
        if (string.IsNullOrWhiteSpace(slug)) return null;
        var s = slug.Trim().ToLowerInvariant();
        var semAcento = new string(s.Normalize(System.Text.NormalizationForm.FormD)
            .Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
            .ToArray());
        var sb = new System.Text.StringBuilder();
        foreach (var c in semAcento)
        {
            if (c is >= 'a' and <= 'z' or >= '0' and <= '9') sb.Append(c);
            else if (c is ' ' or '-' or '_' or '.') sb.Append('-');
        }
        var r = sb.ToString().Trim('-');
        while (r.Contains("--")) r = r.Replace("--", "-");
        return r.Length == 0 ? null : r;
    }
}
