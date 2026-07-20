using System.Globalization;
using System.Text;
using ControleFinanceiro.Application.Relatorios;
using ControleFinanceiro.Application.Patrimonio.Queries.GetPlanoAcao;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ControleFinanceiro.Infrastructure.Services;

/// <summary>Gera o PDF do relatório patrimonial com QuestPDF (marca do assessor + dados do cliente).</summary>
public class RelatorioPatrimonialGenerator : IRelatorioPatrimonialGenerator
{
    private static readonly CultureInfo Pt = CultureInfo.GetCultureInfo("pt-BR");
    private static readonly string[] Paleta = { "#f59e0b", "#8b5cf6", "#3b82f6", "#eab308", "#22c55e", "#ec4899", "#14b8a6", "#f97316" };

    static RelatorioPatrimonialGenerator()
    {
        // Licença Community (grátis p/ faturamento < US$ 1M).
        QuestPDF.Settings.License = LicenseType.Community;
    }

    private static string Money(decimal v) => v.ToString("C2", Pt);
    private static string Pct(decimal v) => v.ToString("0.0", Pt) + "%";

    private static byte[]? ParseLogo(string? base64)
    {
        if (string.IsNullOrWhiteSpace(base64)) return null;
        var raw = base64.Contains(',') ? base64[(base64.IndexOf(',') + 1)..] : base64;
        try { return Convert.FromBase64String(raw); } catch { return null; }
    }

    public byte[] Gerar(RelatorioPatrimonialDados d, RelatorioBranding branding)
    {
        var brand = string.IsNullOrWhiteSpace(branding.CorMarca) ? "#16a34a" : branding.CorMarca!;
        var consultoria = string.IsNullOrWhiteSpace(branding.NomeConsultoria) ? d.AssessorNome : branding.NomeConsultoria!;
        var logo = ParseLogo(branding.LogoBase64);
        var r = d.Resumo;

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(0);
                page.DefaultTextStyle(x => x.FontSize(9).FontColor("#374151"));

                // ── Cabeçalho (marca) ──
                page.Header().Background(brand).PaddingVertical(16).PaddingHorizontal(30).Row(row =>
                {
                    if (logo != null)
                        row.ConstantItem(46).Height(40).Image(logo).FitArea();
                    row.RelativeItem().PaddingLeft(logo != null ? 12 : 0).Column(col =>
                    {
                        col.Item().Text(consultoria).FontColor("#ffffff").FontSize(16).Bold();
                        col.Item().Text($"Relatório Patrimonial · {d.ClienteNome}").FontColor("#ffffffcc").FontSize(10);
                    });
                    row.ConstantItem(120).AlignRight().Text(d.GeradoEm.ToLocalTime().ToString("dd/MM/yyyy", Pt))
                        .FontColor("#ffffffcc").FontSize(9);
                });

                // ── Conteúdo ──
                page.Content().PaddingHorizontal(30).PaddingVertical(20).Column(col =>
                {
                    col.Spacing(18);

                    // 1) Balanço patrimonial
                    col.Item().Element(c => Secao(c, "Balanço Patrimonial", inner =>
                    {
                        inner.Row(row =>
                        {
                            row.RelativeItem().Element(cc => CardMetrica(cc, "Bens", Money(r.TotalBensBRL), "#374151"));
                            row.ConstantItem(10);
                            row.RelativeItem().Element(cc => CardMetrica(cc, "Dívidas", Money(r.TotalDividasBRL), "#dc2626"));
                            row.ConstantItem(10);
                            row.RelativeItem().Element(cc => CardMetrica(cc, "Patrimônio líquido", Money(r.PatrimonioLiquidoBRL), brand));
                            row.ConstantItem(10);
                            row.RelativeItem().Element(cc => CardMetrica(cc, "Alavancagem", Pct(r.AlavancagemPct), "#374151"));
                        });
                    }));

                    // 2) Métricas mensais
                    col.Item().Element(c => Secao(c, "Fluxo de caixa mensal", inner =>
                    {
                        inner.Row(row =>
                        {
                            row.RelativeItem().Element(cc => CardMetrica(cc, "Receita", Money(r.ReceitaMensalBRL), "#16a34a"));
                            row.ConstantItem(10);
                            row.RelativeItem().Element(cc => CardMetrica(cc, "Despesa", Money(r.DespesaMensalBRL), "#dc2626"));
                            row.ConstantItem(10);
                            row.RelativeItem().Element(cc => CardMetrica(cc, "Saldo", Money(r.SaldoLiquidoMensalBRL), r.SaldoLiquidoMensalBRL >= 0 ? "#16a34a" : "#dc2626"));
                            row.ConstantItem(10);
                            row.RelativeItem().Element(cc => CardMetrica(cc, "ROI anual", r.RoiAnualPct != null ? Pct(r.RoiAnualPct.Value) : "—", "#374151"));
                        });
                    }));

                    // 3) Composição por categoria (barras)
                    if (r.Composicao.Any())
                        col.Item().Element(c => Secao(c, "Composição do patrimônio", inner =>
                        {
                            var comp = r.Composicao.ToList();
                            inner.Column(cc =>
                            {
                                cc.Spacing(8);
                                for (var i = 0; i < comp.Count; i++)
                                {
                                    var item = comp[i];
                                    var cor = Paleta[i % Paleta.Length];
                                    cc.Item().Row(row =>
                                    {
                                        row.RelativeItem(3).Text(item.Categoria);
                                        row.RelativeItem(4).AlignMiddle().Element(bc => Barra(bc, item.Pct, cor));
                                        row.ConstantItem(70).AlignRight().Text(Pct(item.Pct)).Bold();
                                        row.ConstantItem(90).AlignRight().Text(Money(item.TotalBRL));
                                    });
                                }
                            });
                        }));

                    // 4) Bens
                    if (r.Ativos.Any())
                        col.Item().Element(c => Secao(c, "Bens", inner =>
                        {
                            inner.Table(table =>
                            {
                                table.ColumnsDefinition(cd =>
                                {
                                    cd.RelativeColumn(3); cd.RelativeColumn(2); cd.RelativeColumn(2);
                                    cd.RelativeColumn(2); cd.RelativeColumn(1.4f);
                                });
                                table.Header(h =>
                                {
                                    Th(h.Cell(), "BEM"); Th(h.Cell(), "VALOR", true);
                                    Th(h.Cell(), "RECEITA/MÊS", true); Th(h.Cell(), "DESPESA/MÊS", true);
                                    Th(h.Cell(), "ROI a.a.", true);
                                });
                                foreach (var a in r.Ativos)
                                {
                                    table.Cell().PaddingVertical(3).Text(a.Nome);
                                    table.Cell().PaddingVertical(3).AlignRight().Text(Money(a.ValorAtual));
                                    table.Cell().PaddingVertical(3).AlignRight().Text(a.ReceitaMensal > 0 ? Money(a.ReceitaMensal) : "—");
                                    table.Cell().PaddingVertical(3).AlignRight().Text(a.DespesaMensal > 0 ? Money(a.DespesaMensal) : "—");
                                    table.Cell().PaddingVertical(3).AlignRight().Text(a.RoiAnualPct != null ? Pct(a.RoiAnualPct.Value) : "—");
                                }
                            });
                        }));

                    // 5) Dívidas
                    if (r.Passivos.Any())
                        col.Item().Element(c => Secao(c, "Dívidas", inner =>
                        {
                            inner.Table(table =>
                            {
                                table.ColumnsDefinition(cd => { cd.RelativeColumn(4); cd.RelativeColumn(2); cd.RelativeColumn(2); });
                                table.Header(h =>
                                {
                                    Th(h.Cell(), "DÍVIDA"); Th(h.Cell(), "PRAZO", true); Th(h.Cell(), "SALDO (BRL)", true);
                                });
                                foreach (var p in r.Passivos)
                                {
                                    table.Cell().PaddingVertical(3).Text(p.Nome);
                                    table.Cell().PaddingVertical(3).AlignRight().Text(p.Prazo == 1 ? "Curto" : "Longo");
                                    table.Cell().PaddingVertical(3).AlignRight().Text(Money(p.ValorBRL));
                                }
                            });
                        }));

                    // 6) Investimentos
                    if (d.Investimentos.QtdInvestimentos > 0)
                        col.Item().Element(c => Secao(c, "Investimentos", inner =>
                        {
                            inner.Row(row =>
                            {
                                row.RelativeItem().Element(cc => CardMetrica(cc, "Aplicado", Money(d.Investimentos.TotalAplicadoBRL), "#374151"));
                                row.ConstantItem(10);
                                row.RelativeItem().Element(cc => CardMetrica(cc, "Atual", Money(d.Investimentos.TotalAtualBRL), "#2563eb"));
                                row.ConstantItem(10);
                                row.RelativeItem().Element(cc => CardMetrica(cc, "Rentabilidade",
                                    d.Investimentos.RentabilidadePct != null ? Pct(d.Investimentos.RentabilidadePct.Value) : "—", "#16a34a"));
                            });
                        }));

                    // 7) Projeção patrimonial (simulação)
                    if (d.SimulacaoDestaque is { } sim)
                        col.Item().Element(c => Secao(c, $"Projeção Patrimonial · {sim.Nome}", inner =>
                        {
                            inner.Column(cc =>
                            {
                                cc.Item().Text(t =>
                                {
                                    t.Span($"Dos {sim.IdadeAtual} aos {sim.IdadeAlvo} anos · aporte {Money(sim.AporteMensal)}/mês · retorno real {Pct(sim.TaxaRetornoRealAnualPct)} · retirada {Money(sim.RetiradaMensal)}/mês")
                                        .FontSize(8).FontColor("#6b7280");
                                });
                                cc.Item().PaddingTop(8).Row(row =>
                                {
                                    row.RelativeItem().Element(x => CardMetrica(x, $"Patrimônio aos {sim.IdadeAlvo}", Money(sim.PatrimonioNaIdadeAlvo), brand));
                                    row.ConstantItem(10);
                                    row.RelativeItem().Element(x => CardMetrica(x, "Extinção dos recursos",
                                        sim.IdadeExtincao != null ? $"{sim.IdadeExtincao} anos" : "Nunca",
                                        sim.IdadeExtincao != null ? "#dc2626" : "#16a34a"));
                                });
                            });
                        }));

                    // 8) Plano de Ação (jornada de etapas) — estilizado como no app
                    if (d.Plano is { } plano && plano.Etapas.Any())
                        col.Item().Element(c => Secao(c, "Plano de Ação", inner =>
                        {
                            var etapas = plano.Etapas.OrderBy(e => e.Ordem).ToList();
                            var concl = etapas.Count(e => e.Status == 3);
                            var pct = etapas.Count > 0 ? (decimal)concl / etapas.Count * 100m : 0m;
                            inner.Column(cc =>
                            {
                                cc.Spacing(7);

                                // Objetivo + barra de progresso
                                cc.Item().Column(o =>
                                {
                                    o.Item().Text(t =>
                                    {
                                        t.Span("Objetivo: ").Bold();
                                        t.Span(plano.Objetivo).Bold().FontColor("#111827");
                                        if (!string.IsNullOrWhiteSpace(plano.Prazo)) t.Span($"   ·   meta {plano.Prazo}").FontColor("#6b7280");
                                    });
                                    o.Item().PaddingTop(6).Element(x => Barra(x, pct, "#C79A4E"));
                                    o.Item().PaddingTop(3).Text($"{concl} de {etapas.Count} etapas concluídas · {pct.ToString("0", Pt)}%")
                                        .FontSize(8).FontColor("#6b7280");
                                });

                                // Trilha (gráfico vetorial, igual ao app)
                                cc.Item().PaddingVertical(4).Svg(TrilhaSvg(plano));

                                // Cards de etapa (faixa de status + pill + alvo)
                                for (var i = 0; i < etapas.Count; i++)
                                {
                                    var e = etapas[i];
                                    var cor = StatusCor(e.Status);
                                    cc.Item().Border(1).BorderColor("#e5e7eb").Row(row =>
                                    {
                                        row.ConstantItem(4).Background(cor);
                                        row.RelativeItem().Padding(10).Column(card =>
                                        {
                                            card.Item().Row(h =>
                                            {
                                                h.RelativeItem().Text(t =>
                                                {
                                                    t.Span($"{i + 1}.  ").Bold().FontColor("#9ca3af");
                                                    t.Span(e.Titulo).Bold().FontColor("#111827");
                                                });
                                                if (!string.IsNullOrWhiteSpace(e.Prazo))
                                                    h.ConstantItem(70).AlignRight().Text(e.Prazo!).FontSize(8).FontColor("#6b7280");
                                            });
                                            if (!string.IsNullOrWhiteSpace(e.Descricao))
                                                card.Item().PaddingTop(3).Text(e.Descricao!).FontSize(8).FontColor("#6b7280");
                                            card.Item().PaddingTop(7).Row(f =>
                                            {
                                                f.AutoItem().Background(StatusBg(e.Status)).PaddingVertical(2).PaddingHorizontal(7)
                                                    .Text(StatusLabel(e.Status)).FontSize(7.5f).Bold().FontColor(cor);
                                                f.RelativeItem();
                                                if (!string.IsNullOrWhiteSpace(e.Alvo))
                                                    f.AutoItem().AlignRight().AlignMiddle().Text(e.Alvo!).FontSize(8.5f).Bold().FontColor("#9a6c22");
                                            });
                                        });
                                    });
                                }

                                // Card do objetivo final (destaque)
                                cc.Item().Background("#0e2a26").Padding(12).Row(g =>
                                {
                                    g.RelativeItem().Column(x =>
                                    {
                                        x.Item().Text("★  " + plano.Objetivo).Bold().FontColor("#ffffff");
                                        x.Item().PaddingTop(2).Text("Objetivo final").FontSize(8).Bold().FontColor("#E7C57E");
                                    });
                                    if (!string.IsNullOrWhiteSpace(plano.Prazo))
                                        g.ConstantItem(70).AlignRight().AlignMiddle().Text(plano.Prazo!).Bold().FontColor("#E7C57E");
                                });
                            });
                        }));

                    if (r.CambioEstimado)
                        col.Item().PaddingTop(4).Text("* Valores em moeda estrangeira convertidos por câmbio estimado, não em tempo real.")
                            .FontSize(7.5f).Italic().FontColor("#9ca3af");
                });

                // ── Rodapé ──
                var rodapeMsg = string.IsNullOrWhiteSpace(branding.MensagemRodape)
                    ? "Não constitui recomendação formal de investimento."
                    : branding.MensagemRodape!;

                page.Footer().PaddingHorizontal(30).PaddingBottom(12).Row(row =>
                {
                    row.RelativeItem().Text($"{consultoria}  ·  {rodapeMsg}")
                        .FontSize(7.5f).FontColor("#9ca3af");
                    row.ConstantItem(60).AlignRight().Text(t =>
                    {
                        t.CurrentPageNumber().FontSize(7.5f).FontColor("#9ca3af");
                        t.Span(" / ").FontSize(7.5f).FontColor("#9ca3af");
                        t.TotalPages().FontSize(7.5f).FontColor("#9ca3af");
                    });
                });
            });
        });

        return doc.GeneratePdf();
    }

    // ── Helpers de layout ──
    private static void Secao(IContainer c, string titulo, Action<IContainer> conteudo)
    {
        c.Column(col =>
        {
            col.Item().PaddingBottom(8).Text(titulo).FontSize(13).Bold().FontColor("#111827");
            col.Item().Element(conteudo);
        });
    }

    private static void CardMetrica(IContainer c, string label, string valor, string cor)
    {
        c.Border(1).BorderColor("#e5e7eb").Padding(10).Column(col =>
        {
            col.Item().Text(label).FontSize(8).FontColor("#6b7280");
            col.Item().PaddingTop(2).Text(valor).FontSize(13).Bold().FontColor(cor);
        });
    }

    private static void Barra(IContainer c, decimal pct, string cor)
    {
        c.Height(8).Background("#e5e7eb").Row(row =>
        {
            var p = (float)Math.Clamp(pct, 0m, 100m);
            if (p > 0) row.RelativeItem(p).Background(cor);
            if (p < 100) row.RelativeItem(100 - p);
        });
    }

    private static string StatusLabel(int status) => status switch { 3 => "Concluída", 2 => "Em andamento", _ => "A fazer" };
    private static string StatusCor(int status) => status switch { 3 => "#16a34a", 2 => "#b45309", _ => "#9ca3af" };
    private static string StatusBg(int status) => status switch { 3 => "#dcfce7", 2 => "#fef3c7", _ => "#f3f4f6" };

    /// <summary>Gera o SVG da trilha ascendente do Plano de Ação (nós + estrela do objetivo).</summary>
    private static string TrilhaSvg(PlanoAcaoDto plano)
    {
        var inv = CultureInfo.InvariantCulture;
        var etapas = plano.Etapas.OrderBy(e => e.Ordem).ToList();
        const int W = 720, H = 200;
        int n = etapas.Count + 1;
        bool labels = n <= 7;
        double padL = 34, padR = 74, padT = labels ? 48 : 26, padB = labels ? 42 : 22;
        double yB = H - padB, yT = padT;
        double X(int i) => n == 1 ? padL : padL + i * (W - padL - padR) / (n - 1);
        double Y(int i) => n == 1 ? yB : yB - i * (yB - yT) / (n - 1);
        string F(double v) => v.ToString("0.#", inv);

        string Smooth(int upto)
        {
            var sb = new StringBuilder($"M {F(X(0))} {F(Y(0))}");
            for (int i = 1; i <= upto; i++)
            {
                double cx = (X(i - 1) + X(i)) / 2;
                sb.Append($" C {F(cx)} {F(Y(i - 1))} {F(cx)} {F(Y(i))} {F(X(i))} {F(Y(i))}");
            }
            return sb.ToString();
        }

        string Star(double cx, double cy, double r)
        {
            var sb = new StringBuilder();
            double inner = r * 0.42;
            for (int k = 0; k < 10; k++)
            {
                double ang = -Math.PI / 2 + k * Math.PI / 5;
                double rr = k % 2 == 0 ? r : inner;
                double x = cx + rr * Math.Cos(ang), y = cy + rr * Math.Sin(ang);
                sb.Append(k == 0 ? $"M {F(x)} {F(y)}" : $" L {F(x)} {F(y)}");
            }
            return sb.Append(" Z").ToString();
        }

        string Esc(string t) => t.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        string Trunc(string t, int m) => t.Length > m ? t[..(m - 1)] + "…" : t;
        const string FF = "Lato, Helvetica, Arial, sans-serif";

        bool todas = etapas.All(e => e.Status == 3);
        int goldEnd = 0;
        for (int i = 0; i < etapas.Count; i++) if (etapas[i].Status != 1) goldEnd = i;
        if (todas) goldEnd = n - 1;

        var s = new StringBuilder();
        s.Append($"<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 {W} {H}' width='{W}' height='{H}'>");
        s.Append("<defs><linearGradient id='g' x1='0' y1='0' x2='1' y2='0'><stop offset='0' stop-color='#E7C57E'/><stop offset='1' stop-color='#C79A4E'/></linearGradient></defs>");
        s.Append($"<path d='{Smooth(n - 1)} L {F(X(n - 1))} {H} L {F(X(0))} {H} Z' fill='#C79A4E' fill-opacity='0.08'/>");
        s.Append($"<path d='{Smooth(n - 1)}' fill='none' stroke='#d1d5db' stroke-width='4' stroke-linecap='round' stroke-dasharray='1 12'/>");
        if (goldEnd >= 1)
            s.Append($"<path d='{Smooth(goldEnd)}' fill='none' stroke='url(#g)' stroke-width='4.5' stroke-linecap='round'/>");

        for (int i = 0; i < etapas.Count; i++)
        {
            var e = etapas[i];
            double cx = X(i), cy = Y(i);
            bool cc = e.Status == 3, at = e.Status == 2;
            if (at) s.Append($"<circle cx='{F(cx)}' cy='{F(cy)}' r='16' fill='#C79A4E' fill-opacity='0.18'/>");
            s.Append($"<circle cx='{F(cx)}' cy='{F(cy)}' r='{(cc ? 13 : 12)}' fill='{(cc ? "#C79A4E" : "#ffffff")}' stroke='{(cc ? "#C79A4E" : at ? "#C79A4E" : "#d1d5db")}' stroke-width='{F(cc ? 0 : at ? 3.2 : 2.5)}'/>");
            if (cc)
                s.Append($"<path d='M {F(cx - 6)} {F(cy)} l 4 4 l 8 -9' fill='none' stroke='#241a08' stroke-width='2.4' stroke-linecap='round' stroke-linejoin='round'/>");
            else
                s.Append($"<text x='{F(cx)}' y='{F(cy + 4)}' font-size='12' font-weight='bold' fill='{(at ? "#9a6c22" : "#9ca3af")}' text-anchor='middle' font-family='{FF}'>{i + 1}</text>");
            if (labels)
            {
                s.Append($"<text x='{F(cx)}' y='{F(cy - 22)}' font-size='11' font-weight='bold' fill='{(cc || at ? "#374151" : "#9ca3af")}' text-anchor='middle' font-family='{FF}'>{Esc(Trunc(e.Titulo, 14))}</text>");
                if (!string.IsNullOrWhiteSpace(e.Prazo))
                    s.Append($"<text x='{F(cx)}' y='{F(cy + 26)}' font-size='10' fill='#9ca3af' text-anchor='middle' font-family='{FF}'>{Esc(e.Prazo!)}</text>");
            }
        }

        double gx = X(n - 1), gy = Y(n - 1);
        s.Append($"<circle cx='{F(gx)}' cy='{F(gy)}' r='16' fill='{(todas ? "#C79A4E" : "#ffffff")}' stroke='#C79A4E' stroke-width='2'/>");
        s.Append($"<path d='{Star(gx, gy, 8)}' fill='{(todas ? "#241a08" : "#C79A4E")}'/>");
        if (labels)
        {
            s.Append($"<text x='{F(gx)}' y='{F(gy - 24)}' font-size='11' font-weight='bold' fill='#9a6c22' text-anchor='middle' font-family='{FF}'>Objetivo</text>");
            if (!string.IsNullOrWhiteSpace(plano.Prazo))
                s.Append($"<text x='{F(gx)}' y='{F(gy + 30)}' font-size='10' fill='#9ca3af' text-anchor='middle' font-family='{FF}'>{Esc(plano.Prazo!)}</text>");
        }
        s.Append("</svg>");
        return s.ToString();
    }

    private static void Th(IContainer c, string texto, bool right = false)
    {
        var t = c.BorderBottom(1).BorderColor("#e5e7eb").PaddingVertical(4);
        (right ? t.AlignRight() : t).Text(texto).FontSize(8).Bold().FontColor("#6b7280");
    }
}
