using System.Globalization;
using ControleFinanceiro.Application.Relatorios;
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

    private static void Th(IContainer c, string texto, bool right = false)
    {
        var t = c.BorderBottom(1).BorderColor("#e5e7eb").PaddingVertical(4);
        (right ? t.AlignRight() : t).Text(texto).FontSize(8).Bold().FontColor("#6b7280");
    }
}
