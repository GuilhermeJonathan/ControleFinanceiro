namespace Login.Application.Common.Email;

/// <summary>
/// Template compartilhado dos e-mails transacionais do Meu FinDog.
/// Todos os e-mails usam o mesmo wrapper: container escuro + banner (og-image) + rodapé.
/// O conteúdo interno é passado pelo chamador via <see cref="Wrap"/>.
/// </summary>
public static class EmailTemplateBuilder
{
    public const string AppUrl = "https://app.findog.com.br";

    /// <summary>Container padrão: banner clicável no topo, conteúdo no meio, rodapé fixo.</summary>
    public static string Wrap(string innerHtml) => $"""
        <div style="font-family:sans-serif;max-width:560px;margin:0 auto;background:#0f1117;color:#e2e8f0;border-radius:12px;overflow:hidden">
          <div style="background:#0f1117;padding:0;border-bottom:2px solid #16a34a">
            <a href="{AppUrl}" style="display:block;line-height:0">
              <img src="{AppUrl}/og-image.png" alt="Meu FinDog" width="560"
                   style="display:block;width:100%;max-width:560px;height:auto;border:0" />
            </a>
          </div>
          <div style="padding:32px 24px">
            {innerHtml}
            <p style="color:#64748b;font-size:12px;text-align:center;margin-top:24px">
              Meu FinDog · <a href="{AppUrl}" style="color:#64748b">app.findog.com.br</a>
            </p>
          </div>
        </div>
        """;

    /// <summary>Saudação padrão em destaque.</summary>
    public static string Greeting(string texto) =>
        $"""<p style="font-size:18px;font-weight:700;color:#f1f5f9">{texto}</p>""";

    /// <summary>Parágrafo de corpo com cor secundária.</summary>
    public static string Paragraph(string html) =>
        $"""<p style="color:#94a3b8;line-height:1.6">{html}</p>""";

    /// <summary>Card escuro para listas e detalhes.</summary>
    public static string Card(string innerHtml) =>
        $"""<div style="background:#1e293b;border-radius:10px;padding:16px;margin:20px 0">{innerHtml}</div>""";

    /// <summary>Linha rótulo/valor dentro de um Card.</summary>
    public static string CardRow(string chave, string valor, string corValor = "#e2e8f0") => $"""
        <div style="display:flex;justify-content:space-between;margin-bottom:8px">
          <span style="color:#94a3b8;font-size:14px">{chave}</span>
          <span style="color:{corValor};font-weight:700;font-size:14px">{valor}</span>
        </div>
        """;

    /// <summary>Botão CTA verde centralizado.</summary>
    public static string Button(string texto, string url) => $"""
        <div style="text-align:center;margin:28px 0">
          <a href="{url}"
             style="background:#16a34a;color:#fff;text-decoration:none;padding:14px 32px;border-radius:10px;font-weight:700;font-size:15px;display:inline-block">
            {texto}
          </a>
        </div>
        """;
}
