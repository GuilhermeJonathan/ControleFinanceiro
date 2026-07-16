using Microsoft.Extensions.Configuration;

namespace ControleFinanceiro.Application.Common.Email;

/// <summary>
/// Monta os e-mails com a marca da consultoria do assessor (logo + cor + nome).
/// Usado tanto pelos convites (cliente/corretor) quanto pela notificação de
/// recomendação, garantindo esquema de cores e layout idênticos.
/// </summary>
public static class ConviteEmailBuilder
{
    private const string FrontendDefault = "https://patrimonio-roan.vercel.app";

    public static string MontarLink(IConfiguration configuration, string codigo, string tipo)
    {
        var baseUrl = BaseUrl(configuration);
        return $"{baseUrl}/aceitar?codigo={Uri.EscapeDataString(codigo)}&tipo={tipo}";
    }

    public static string BaseUrl(IConfiguration configuration) =>
        (configuration["Frontend:BaseUrl"] ?? FrontendDefault).TrimEnd('/');

    // ── Convites ─────────────────────────────────────────────────────────────

    public static string CorpoCliente(string marca, string cor, string? logo, string codigo, string link) =>
        Wrap(marca, cor, logo, $"""
            <p style="font-size:18px;font-weight:700;color:#f1f5f9;margin:0 0 8px">Você foi convidado 👋</p>
            <p style="color:#94a3b8;line-height:1.6;margin:0 0 4px">
              <strong style="color:#e2e8f0">{Esc(marca)}</strong> convidou você para acompanhar seu patrimônio
              e suas finanças de forma organizada. Ao aceitar, você cria sua conta em segundos e passa a ver
              tudo num só lugar.
            </p>
            {Botao("Aceitar convite e criar conta", link, cor)}
            {CaixaCodigo(codigo, cor)}
            <p style="color:#64748b;font-size:12px;line-height:1.6;margin-top:20px">
              Se você não esperava este convite, pode ignorar este e-mail com segurança.
            </p>
            """);

    public static string CorpoCorretor(string marca, string cor, string? logo, string codigo, string link) =>
        Wrap(marca, cor, logo, $"""
            <p style="font-size:18px;font-weight:700;color:#f1f5f9;margin:0 0 8px">Convite para atuar como corretor 🤝</p>
            <p style="color:#94a3b8;line-height:1.6;margin:0 0 4px">
              <strong style="color:#e2e8f0">{Esc(marca)}</strong> convidou você para atuar como corretor e atender
              a carteira de clientes delegada. Ao aceitar, sua conta de corretor é criada em segundos.
            </p>
            {Botao("Aceitar convite e criar conta", link, cor)}
            {CaixaCodigo(codigo, cor)}
            <p style="color:#64748b;font-size:12px;line-height:1.6;margin-top:20px">
              Se você não esperava este convite, pode ignorar este e-mail com segurança.
            </p>
            """);

    // ── Recomendação ─────────────────────────────────────────────────────────

    public static string CorpoRecomendacao(string marca, string cor, string? logo,
        string nomeCliente, string tipoLabel, string texto, string link) =>
        Wrap(marca, cor, logo, $"""
            <p style="font-size:18px;font-weight:700;color:#f1f5f9;margin:0 0 8px">Olá, {Esc(nomeCliente)}!</p>
            <p style="color:#94a3b8;line-height:1.6;margin:0">
              <strong style="color:#e2e8f0">{Esc(marca)}</strong> enviou uma nova recomendação para você:
            </p>
            <div style="background:#1e293b;border-radius:10px;padding:16px;margin:20px 0">
              <p style="color:#94a3b8;font-size:13px;margin:0 0 8px">{Esc(tipoLabel)}</p>
              <p style="color:#e2e8f0;font-size:14px;line-height:1.6;margin:0">{Esc(texto)}</p>
            </div>
            {Botao("Responder", link, cor)}
            """);

    // ── Blocos base ──────────────────────────────────────────────────────────

    private static string Header(string marca, string cor, string? logo)
    {
        var temLogo = !string.IsNullOrWhiteSpace(logo);
        var conteudo = temLogo
            ? $"""<img src="{logo}" alt="{Esc(marca)}" style="max-height:52px;max-width:220px;height:auto;display:block;border:0" />"""
            : $"""<p style="margin:0;font-size:20px;font-weight:800;color:#f1f5f9">{Esc(marca)}</p>""";
        return $"""
            <div style="padding:24px;border-bottom:2px solid {cor};background:#0f1117">
              {conteudo}
            </div>
            """;
    }

    private static string Botao(string texto, string link, string cor) => $"""
        <div style="text-align:center;margin:28px 0">
          <a href="{link}" style="background:{cor};color:#fff;text-decoration:none;padding:14px 32px;border-radius:10px;font-weight:700;font-size:15px;display:inline-block">
            {Esc(texto)}
          </a>
        </div>
        """;

    private static string CaixaCodigo(string codigo, string cor) => $"""
        <div style="background:#1e293b;border-radius:10px;padding:16px;margin:8px 0;text-align:center">
          <p style="color:#94a3b8;font-size:13px;margin:0 0 6px">Ou informe este código no app</p>
          <p style="color:{cor};font-size:28px;font-weight:800;letter-spacing:7px;margin:0">{Esc(codigo)}</p>
        </div>
        """;

    private static string Wrap(string marca, string cor, string? logo, string inner) => $"""
        <div style="font-family:sans-serif;max-width:560px;margin:0 auto;background:#0f1117;color:#e2e8f0;border-radius:12px;overflow:hidden">
          {Header(marca, cor, logo)}
          <div style="padding:32px 24px">
            {inner}
            <p style="color:#64748b;font-size:12px;text-align:center;margin-top:24px">{Esc(marca)}</p>
          </div>
        </div>
        """;

    private static string Esc(string s) =>
        (s ?? string.Empty).Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
}
