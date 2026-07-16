using Microsoft.Extensions.Configuration;

namespace ControleFinanceiro.Application.Common.Email;

/// <summary>
/// Monta o link e o HTML dos e-mails de convite (cliente e corretor).
/// O link aponta para a rota pública /aceitar do app, com a URL base vinda do config.
/// </summary>
public static class ConviteEmailBuilder
{
    private const string FrontendDefault = "https://patrimonio-roan.vercel.app";

    public static string MontarLink(IConfiguration configuration, string codigo, string tipo)
    {
        var baseUrl = (configuration["Frontend:BaseUrl"] ?? FrontendDefault).TrimEnd('/');
        return $"{baseUrl}/aceitar?codigo={Uri.EscapeDataString(codigo)}&tipo={tipo}";
    }

    public static string CorpoCliente(string marca, string cor, string codigo, string link) =>
        Wrap(marca, cor,
            titulo: "Você foi convidado 👋",
            intro: $"<strong style=\"color:#e2e8f0\">{Esc(marca)}</strong> convidou você para acompanhar seu " +
                   "patrimônio e suas finanças de forma organizada. Ao aceitar, você cria sua conta em segundos " +
                   "e passa a ver tudo num só lugar.",
            codigo: codigo, cor2: cor, link: link,
            botao: "Aceitar convite e criar conta");

    public static string CorpoCorretor(string marca, string cor, string codigo, string link) =>
        Wrap(marca, cor,
            titulo: "Convite para atuar como corretor 🤝",
            intro: $"<strong style=\"color:#e2e8f0\">{Esc(marca)}</strong> convidou você para atuar como corretor " +
                   "e atender a carteira de clientes delegada. Ao aceitar, sua conta de corretor é criada em segundos.",
            codigo: codigo, cor2: cor, link: link,
            botao: "Aceitar convite e criar conta");

    private static string Wrap(string marca, string cor, string titulo, string intro, string codigo, string cor2, string link, string botao) =>
        $"""
        <div style="font-family:sans-serif;max-width:560px;margin:0 auto;background:#0f1117;color:#e2e8f0;border-radius:12px;overflow:hidden">
          <div style="padding:24px;border-bottom:2px solid {cor};background:#0f1117">
            <p style="margin:0;font-size:20px;font-weight:800;color:#f1f5f9">{Esc(marca)}</p>
          </div>
          <div style="padding:32px 24px">
            <p style="font-size:18px;font-weight:700;color:#f1f5f9">{titulo}</p>
            <p style="color:#94a3b8;line-height:1.6">{intro}</p>
            <div style="text-align:center;margin:28px 0">
              <a href="{link}"
                 style="background:{cor};color:#fff;text-decoration:none;padding:14px 32px;border-radius:10px;font-weight:700;font-size:15px;display:inline-block">
                {botao}
              </a>
            </div>
            <div style="background:#1e293b;border-radius:10px;padding:16px;margin:8px 0;text-align:center">
              <p style="color:#94a3b8;font-size:13px;margin:0 0 6px">Ou informe este código no app</p>
              <p style="color:{cor2};font-size:28px;font-weight:800;letter-spacing:7px;margin:0">{codigo}</p>
            </div>
            <p style="color:#64748b;font-size:12px;line-height:1.6;margin-top:20px">
              Se você não esperava este convite, pode ignorar este e-mail com segurança.
            </p>
          </div>
        </div>
        """;

    private static string Esc(string s) =>
        s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
}
