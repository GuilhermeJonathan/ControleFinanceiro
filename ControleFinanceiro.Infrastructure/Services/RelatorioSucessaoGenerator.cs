using System.Globalization;
using System.Text;
using ControleFinanceiro.Application.Patrimonio.Queries.GetContas;
using ControleFinanceiro.Application.Patrimonio.Queries.GetEstruturas;
using ControleFinanceiro.Application.Patrimonio.Queries.GetPlanoAcao;
using ControleFinanceiro.Application.Relatorios;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ControleFinanceiro.Infrastructure.Services;

/// <summary>Gera o PDF do relatório de sucessão (estrutura, beneficiários, contas, planos) com QuestPDF.</summary>
public class RelatorioSucessaoGenerator : IRelatorioSucessaoGenerator
{
    private static readonly CultureInfo Pt = CultureInfo.GetCultureInfo("pt-BR");
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
    private const string GOLD = "#C79A4E";
    private const string BLUE = "#6C8EBF";
    private static readonly string[] Paleta = { "#C79A4E", "#6C8EBF", "#B784D6", "#4E9A7E", "#D6795B", "#9AA5B1", "#C7574E", "#4E7EC7" };

    private static readonly Dictionary<int, string> TipoEstrutura = new()
    { [1] = "Trust", [2] = "Holding Patrimonial", [3] = "Holding de Participações", [4] = "Offshore", [5] = "Empresa Operacional", [6] = "PPLI", [99] = "Outro" };
    private static readonly Dictionary<int, string> Papel = new() { [1] = "Cônjuge", [2] = "Filho", [3] = "Neto", [99] = "Outro" };
    private static readonly Dictionary<int, string> TipoConta = new() { [1] = "Corrente", [2] = "Investimento / Custódia", [3] = "Internacional", [99] = "Conta" };

    static RelatorioSucessaoGenerator() => QuestPDF.Settings.License = LicenseType.Community;

    private static string Money(decimal v) => v.ToString("C2", Pt);
    private static string MoneyCurto(decimal v)
        => Math.Abs(v) >= 1_000_000 ? "R$ " + (v / 1_000_000).ToString("0.0", Pt) + "M"
         : Math.Abs(v) >= 1_000 ? "R$ " + (v / 1_000).ToString("0.0", Pt) + "k"
         : "R$ " + v.ToString("0", Pt);

    private static byte[]? ParseLogo(string? base64)
    {
        if (string.IsNullOrWhiteSpace(base64)) return null;
        var raw = base64.Contains(',') ? base64[(base64.IndexOf(',') + 1)..] : base64;
        try { return Convert.FromBase64String(raw); } catch { return null; }
    }

    public byte[] Gerar(RelatorioSucessaoDados d, RelatorioBranding branding) => Build(d, branding).GeneratePdf();

    /// <summary>Monta o documento (sem gerar bytes) — permite mesclar no relatório completo.</summary>
    internal IDocument Build(RelatorioSucessaoDados d, RelatorioBranding branding)
    {
        var brand = string.IsNullOrWhiteSpace(branding.CorMarca) ? "#16a34a" : branding.CorMarca!;
        var consultoria = string.IsNullOrWhiteSpace(branding.NomeConsultoria) ? d.AssessorNome : branding.NomeConsultoria!;
        var logo = ParseLogo(branding.LogoBase64);

        var totalFamilia = d.Grafo.TotalEmEstruturasBRL + d.Grafo.TotalPessoaFisicaBRL;
        var distribs = d.Sucessao.Distribuicoes;
        var totalDistribuido = distribs.Sum(x => x.ValorBRL);
        var benef = d.Sucessao.Beneficiarios;
        var planos = d.Planos.Where(p => p.Etapas.Any()).ToList();
        var totalEtapas = planos.Sum(p => p.Etapas.Count());
        var etapasFeitas = planos.Sum(p => p.Etapas.Count(e => e.Status == 3));
        var progresso = totalEtapas > 0 ? (int)Math.Round((double)etapasFeitas / totalEtapas * 100) : 0;

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(0);
                page.DefaultTextStyle(x => x.FontSize(9).FontColor("#374151"));

                page.Header().Background(brand).PaddingVertical(16).PaddingHorizontal(30).Row(row =>
                {
                    if (logo != null) row.ConstantItem(46).Height(40).Image(logo).FitArea();
                    row.RelativeItem().PaddingLeft(logo != null ? 12 : 0).Column(col =>
                    {
                        col.Item().Text(consultoria).FontColor("#ffffff").FontSize(16).Bold();
                        col.Item().Text($"Relatório de Sucessão · {d.ClienteNome}").FontColor("#ffffffcc").FontSize(10);
                    });
                    row.ConstantItem(120).AlignRight().Text(d.GeradoEm.ToLocalTime().ToString("dd/MM/yyyy", Pt))
                        .FontColor("#ffffffcc").FontSize(9);
                });

                page.Content().PaddingHorizontal(30).PaddingVertical(20).Column(col =>
                {
                    col.Spacing(18);

                    // 1) KPIs
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Element(c => CardMetrica(c, "Patrimônio da família", MoneyCurto(totalFamilia), brand));
                        row.ConstantItem(10);
                        row.RelativeItem().Element(c => CardMetrica(c, "Distribuído", MoneyCurto(totalDistribuido), "#374151"));
                        row.ConstantItem(10);
                        row.RelativeItem().Element(c => CardMetrica(c, "Beneficiários", benef.Count.ToString(), "#374151"));
                        row.ConstantItem(10);
                        row.RelativeItem().Element(c => CardMetrica(c, "Planejamento", $"{progresso}%", GOLD));
                    });

                    // 1b) Indicadores (gauges)
                    col.Item().Element(c => Secao(c, "Indicadores", inner =>
                        inner.Row(row =>
                        {
                            row.RelativeItem().Column(x => { x.Item().AlignCenter().Svg(GaugeSvg(d.Indicadores.GovernancaScore)); x.Item().AlignCenter().Text("Governança do Trust").FontSize(8).FontColor("#6b7280"); });
                            row.RelativeItem().Column(x => { x.Item().AlignCenter().Svg(GaugeSvg(d.Indicadores.ConformidadeScore)); x.Item().AlignCenter().Text("Conformidade").FontSize(8).FontColor("#6b7280"); });
                            row.RelativeItem().Column(x => { x.Item().AlignCenter().Svg(GaugeSvg(progresso)); x.Item().AlignCenter().Text("Planejamento Sucessório").FontSize(8).FontColor("#6b7280"); });
                        })));

                    // 2) Estrutura Patrimonial Lógica
                    if (d.Grafo.Estruturas.Any())
                        col.Item().Element(c => Secao(c, "Estrutura Patrimonial Lógica", inner =>
                            inner.Svg(GrafoSvg(d.Grafo))));

                    // 3) Planejado × Distribuído
                    if (benef.Any())
                        col.Item().Element(c => Secao(c, "Planejado × Distribuído", inner =>
                        {
                            inner.Table(table =>
                            {
                                table.ColumnsDefinition(cd => { cd.RelativeColumn(3); cd.RelativeColumn(1.5f); cd.RelativeColumn(2); cd.RelativeColumn(1.6f); });
                                table.Header(h =>
                                {
                                    Th(h.Cell(), "BENEFICIÁRIO"); Th(h.Cell(), "PLANEJADO", true);
                                    Th(h.Cell(), "DISTRIBUÍDO", true); Th(h.Cell(), "STATUS", true);
                                });
                                foreach (var b in benef)
                                {
                                    var distBRL = distribs.Where(x => x.BeneficiarioId == b.Id).Sum(x => x.ValorBRL);
                                    var distPct = totalDistribuido > 0 ? distBRL / totalDistribuido * 100 : 0;
                                    var recebeu = distBRL > 0;
                                    table.Cell().PaddingVertical(3).Text(t => { t.Span(b.Nome).Bold(); t.Span($"  · {Papel.GetValueOrDefault(b.Papel, "Outro")}").FontSize(8).FontColor("#6b7280"); });
                                    table.Cell().PaddingVertical(3).AlignRight().Text($"{b.PercentualDistribuicao.ToString("0", Pt)}%");
                                    table.Cell().PaddingVertical(3).AlignRight().Text($"{Money(distBRL)} · {distPct.ToString("0", Pt)}%");
                                    table.Cell().PaddingVertical(3).AlignRight().Text(recebeu ? "Distribuído" : "A distribuir")
                                        .FontColor(recebeu ? "#16a34a" : "#9ca3af").Bold().FontSize(8);
                                }
                            });
                        }));

                    // 4) Distribuições por beneficiário (pizza)
                    var slices = distribs.GroupBy(x => x.BeneficiarioNome ?? "Sem beneficiário")
                        .Select(g => (Label: g.Key, Valor: g.Sum(x => x.ValorBRL)))
                        .Where(x => x.Valor > 0).OrderByDescending(x => x.Valor).ToList();
                    if (slices.Count > 0)
                        col.Item().Element(c => Secao(c, "Distribuições por beneficiário", inner =>
                        {
                            inner.Row(row =>
                            {
                                row.ConstantItem(170).AlignMiddle().Svg(DonutSvg(slices.Select(x => (double)x.Valor).ToList()));
                                row.RelativeItem().AlignMiddle().Column(lg =>
                                {
                                    lg.Spacing(5);
                                    for (var i = 0; i < slices.Count; i++)
                                    {
                                        var sl = slices[i];
                                        var pct = totalDistribuido > 0 ? sl.Valor / totalDistribuido * 100 : 0;
                                        lg.Item().Row(r =>
                                        {
                                            r.ConstantItem(12).AlignMiddle().Height(12).Background(Paleta[i % Paleta.Length]);
                                            r.RelativeItem().PaddingLeft(6).Text(sl.Label);
                                            r.ConstantItem(90).AlignRight().Text($"{Money(sl.Valor)}");
                                            r.ConstantItem(46).AlignRight().Text($"{pct.ToString("0", Pt)}%").Bold();
                                        });
                                    }
                                });
                            });
                        }));

                    // 5) Contas financeiras (Nacionais × Internacionais)
                    if (d.Contas.Contas.Any())
                        col.Item().Element(c => Secao(c, "Contas financeiras", inner =>
                        {
                            inner.Column(cc =>
                            {
                                cc.Spacing(12);
                                foreach (var g in GruposContas(d.Contas.Contas))
                                    cc.Item().Element(gc => GrupoContas(gc, g.Chave, g.Contas, g.Total));
                            });
                        }));

                    // 6) Planos de ação (trilha)
                    if (planos.Count > 0)
                        col.Item().Element(c => Secao(c, planos.Count > 1 ? "Planos de Ação" : "Plano de Ação", inner =>
                        {
                            inner.Column(cont =>
                            {
                                cont.Spacing(14);
                                foreach (var p in planos)
                                    cont.Item().Column(o =>
                                    {
                                        o.Item().Text(t =>
                                        {
                                            t.Span("Objetivo: ").Bold();
                                            t.Span(p.Objetivo).Bold().FontColor("#111827");
                                            if (!string.IsNullOrWhiteSpace(p.Prazo)) t.Span($"   ·   meta {p.Prazo}").FontColor("#6b7280");
                                        });
                                        o.Item().PaddingVertical(4).Svg(TrilhaSvg(p));
                                    });
                            });
                        }));

                    col.Item().PaddingTop(4).Text("* Valores em moeda estrangeira convertidos por câmbio estimado, não em tempo real.")
                        .FontSize(7.5f).Italic().FontColor("#9ca3af");
                });

                var rodapeMsg = string.IsNullOrWhiteSpace(branding.MensagemRodape)
                    ? "Documento gerencial de planejamento sucessório. Não constitui aconselhamento jurídico ou tributário."
                    : branding.MensagemRodape!;
                page.Footer().PaddingHorizontal(30).PaddingBottom(12).Row(row =>
                {
                    row.RelativeItem().Text($"{consultoria}  ·  {rodapeMsg}").FontSize(7.5f).FontColor("#9ca3af");
                    row.ConstantItem(60).AlignRight().Text(t =>
                    {
                        t.CurrentPageNumber().FontSize(7.5f).FontColor("#9ca3af");
                        t.Span(" / ").FontSize(7.5f).FontColor("#9ca3af");
                        t.TotalPages().FontSize(7.5f).FontColor("#9ca3af");
                    });
                });
            });
        });

        return doc;
    }

    // ── Contas agrupadas ──────────────────────────────────────────────────
    private static bool ContaNacional(ContaDto c)
    {
        var p = (c.Pais ?? "").ToLowerInvariant();
        if (p.Contains("bras")) return true;
        if (string.IsNullOrWhiteSpace(p) && c.Moeda == "BRL") return true;
        return false;
    }

    private static IEnumerable<(string Chave, List<ContaDto> Contas, decimal Total)> GruposContas(IEnumerable<ContaDto> contas)
    {
        var lista = contas.ToList();
        foreach (var (chave, sel) in new[] { ("Nacionais", lista.Where(ContaNacional).ToList()), ("Internacionais", lista.Where(c => !ContaNacional(c)).ToList()) })
            if (sel.Count > 0) yield return (chave, sel, sel.Sum(c => c.ValorBRL));
    }

    private static void GrupoContas(IContainer c, string chave, List<ContaDto> contas, decimal total)
    {
        c.Column(col =>
        {
            col.Item().PaddingBottom(6).Row(r =>
            {
                r.RelativeItem().Text($"{chave}  ·  {contas.Count}").FontSize(10).Bold().FontColor("#111827");
                r.ConstantItem(110).AlignRight().Text(Money(total)).FontSize(10).Bold().FontColor(GOLD);
            });
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(cd => { cd.RelativeColumn(2.4f); cd.RelativeColumn(1.4f); cd.RelativeColumn(1.6f); cd.RelativeColumn(2f); });
                table.Header(h =>
                {
                    Th(h.Cell(), "CONTA"); Th(h.Cell(), "SALDO/PORTFÓLIO", true); Th(h.Cell(), "EM BRL", true); Th(h.Cell(), "LOMBARD / STATUS", true);
                });
                foreach (var c2 in contas)
                {
                    table.Cell().PaddingVertical(3).Text(t =>
                    {
                        t.Span(c2.Nome).Bold();
                        var sub = c2.Instituicao ?? TipoConta.GetValueOrDefault(c2.Tipo, "Conta");
                        var loc = c2.EstruturaNome ?? "Pessoa física";
                        t.Span($"\n{sub} · {loc}").FontSize(7.5f).FontColor("#6b7280");
                    });
                    table.Cell().PaddingVertical(3).AlignRight().Text(c2.ValorPortfolio != null
                        ? $"{c2.Moeda} {c2.ValorPortfolio.Value.ToString("N0", Pt)}"
                        : (c2.AgregaInvestimentos ? "derivado" : $"{c2.Moeda} {c2.Saldo.ToString("N0", Pt)}"));
                    table.Cell().PaddingVertical(3).AlignRight().Text(Money(c2.ValorBRL)).Bold();
                    table.Cell().PaddingVertical(3).AlignRight().Text(t =>
                    {
                        if (c2.LombardLimite != null)
                            t.Span($"Lombard {c2.Moeda} {(c2.LombardDisponivel ?? 0).ToString("N0", Pt)}/{c2.LombardLimite.Value.ToString("N0", Pt)}").FontSize(8);
                        if (!string.IsNullOrWhiteSpace(c2.Status))
                            t.Span($"{(c2.LombardLimite != null ? "\n" : "")}{c2.Status}").FontSize(8).FontColor(GOLD).Bold();
                        if (c2.LombardLimite == null && string.IsNullOrWhiteSpace(c2.Status)) t.Span("—").FontColor("#9ca3af");
                    });
                }
            });
        });
    }

    // ── SVG: gauge semicircular (0–100) ────────────────────────────────────
    private static string GaugeSvg(int? val)
    {
        string F(double v) => v.ToString("0.#", Inv);
        const double cx = 70, cy = 78, r = 58;
        (double x, double y) P(double deg) { var a = deg * Math.PI / 180; return (cx + r * Math.Cos(a), cy - r * Math.Sin(a)); }
        var cor = val == null ? "#d1d5db" : val >= 80 ? "#3fb950" : val >= 50 ? GOLD : "#C7574E";
        var (lx, ly) = P(180); var (rx, ry) = P(0);
        var s = new StringBuilder($"<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 140 92' width='140' height='92'>");
        s.Append($"<path d='M {F(lx)} {F(ly)} A {r} {r} 0 0 1 {F(rx)} {F(ry)}' fill='none' stroke='#e5e7eb' stroke-width='11' stroke-linecap='round'/>");
        if (val is > 0)
        {
            var (vx, vy) = P(180 - 180.0 * Math.Clamp(val.Value, 0, 100) / 100);
            s.Append($"<path d='M {F(lx)} {F(ly)} A {r} {r} 0 0 1 {F(vx)} {F(vy)}' fill='none' stroke='{cor}' stroke-width='11' stroke-linecap='round'/>");
        }
        s.Append($"<text x='{F(cx)}' y='{F(cy - 6)}' font-size='22' font-weight='bold' fill='#111827' text-anchor='middle' font-family='Lato,Arial,sans-serif'>{(val?.ToString() ?? "—")}</text>");
        s.Append($"<text x='{F(cx)}' y='{F(cy + 9)}' font-size='9' fill='#9ca3af' text-anchor='middle' font-family='Lato,Arial,sans-serif'>{(val == null ? "sem nota" : "/ 100")}</text>");
        s.Append("</svg>");
        return s.ToString();
    }

    // ── SVG: donut ────────────────────────────────────────────────────────
    private static string DonutSvg(List<double> valores)
    {
        double total = valores.Sum();
        const int SZ = 150, SW = 24;
        double r = (SZ - SW) / 2.0, cx = SZ / 2.0, circ = 2 * Math.PI * r;
        string F(double v) => v.ToString("0.##", Inv);
        var s = new StringBuilder($"<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 {SZ} {SZ}' width='{SZ}' height='{SZ}'>");
        s.Append($"<circle cx='{F(cx)}' cy='{F(cx)}' r='{F(r)}' fill='none' stroke='#e5e7eb' stroke-width='{SW}'/>");
        double acc = 0;
        if (total > 0)
            for (var i = 0; i < valores.Count; i++)
            {
                double frac = valores[i] / total, dash = frac * circ, off = -acc * circ;
                s.Append($"<circle cx='{F(cx)}' cy='{F(cx)}' r='{F(r)}' fill='none' stroke='{Paleta[i % Paleta.Length]}' stroke-width='{SW}' stroke-dasharray='{F(dash)} {F(circ - dash)}' stroke-dashoffset='{F(off)}' transform='rotate(-90 {F(cx)} {F(cx)})'/>");
                acc += frac;
            }
        s.Append($"<text x='{F(cx)}' y='{F(cx - 2)}' font-size='22' font-weight='bold' fill='#111827' text-anchor='middle' font-family='Lato,Arial,sans-serif'>{valores.Count}</text>");
        s.Append($"<text x='{F(cx)}' y='{F(cx + 16)}' font-size='10' fill='#6b7280' text-anchor='middle' font-family='Lato,Arial,sans-serif'>beneficiários</text>");
        s.Append("</svg>");
        return s.ToString();
    }

    // ── SVG: grafo da estrutura (níveis) ────────────────────────────────────
    private static string GrafoSvg(GrafoEstruturasDto g)
    {
        string F(double v) => v.ToString("0.#", Inv);
        string Esc(string t) => t.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        string Trunc(string t, int m) => t.Length > m ? t[..(m - 1)] + "…" : t;
        const string FF = "Lato, Arial, sans-serif";

        var estruturas = g.Estruturas;
        var nomePorId = estruturas.ToDictionary(e => e.Id, e => e.Nome);
        // Arestas: pai (ou "familia") → filha.
        var arestas = g.Participacoes.Select(p => (From: p.EstruturaPaiId?.ToString() ?? "familia", To: p.EstruturaFilhaId.ToString(), Benef: p.TipoRelacao == 2)).ToList();
        var temPai = new HashSet<string>(arestas.Select(a => a.To));
        foreach (var e in estruturas)
            if (!temPai.Contains(e.Id.ToString())) arestas.Add(("familia", e.Id.ToString(), false));

        // Profundidade via BFS a partir de "familia".
        var depth = new Dictionary<string, int> { ["familia"] = 0 };
        var fila = new Queue<string>(); fila.Enqueue("familia");
        while (fila.Count > 0)
        {
            var atual = fila.Dequeue();
            foreach (var a in arestas.Where(x => x.From == atual))
                if (!depth.ContainsKey(a.To)) { depth[a.To] = depth[atual] + 1; fila.Enqueue(a.To); }
        }
        // Estruturas órfãs (sem caminho) caem no nível 1.
        foreach (var e in estruturas) depth.TryAdd(e.Id.ToString(), 1);

        var porNivel = depth.Where(kv => kv.Key != "familia").GroupBy(kv => kv.Value).ToDictionary(gr => gr.Key, gr => gr.Select(kv => kv.Key).ToList());
        var maxNivel = porNivel.Keys.DefaultIfEmpty(0).Max();

        const int NW = 150, NH = 46, GAPX = 18, GAPY = 62, BW = 104, BH = 34, PAD = 12;
        var benefRow = g.Beneficiarios.Count > 0 ? BH + 40 : 0;
        int maxCols = Math.Max(1, porNivel.Values.Select(v => v.Count).DefaultIfEmpty(1).Max());
        maxCols = Math.Max(maxCols, g.Beneficiarios.Count);
        double W = Math.Max(560, maxCols * (NW + GAPX) + PAD * 2);
        double H = PAD * 2 + benefRow + (maxNivel + 1) * NH + maxNivel * GAPY;

        var pos = new Dictionary<string, (double X, double Y)>();
        (double X, double Y) Place(int nivel, int idx, int count)
        {
            double totalW = count * NW + (count - 1) * GAPX;
            double startX = (W - totalW) / 2;
            return (startX + idx * (NW + GAPX), PAD + benefRow + nivel * (NH + GAPY));
        }
        pos["familia"] = Place(0, 0, 1);
        foreach (var (nivel, ids) in porNivel)
            for (var i = 0; i < ids.Count; i++) pos[ids[i]] = Place(nivel, i, ids.Count);

        var s = new StringBuilder($"<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 {F(W)} {F(H)}' width='{F(W)}' height='{F(H)}'>");
        // Arestas estrutura.
        foreach (var a in arestas)
        {
            if (!pos.TryGetValue(a.From, out var pa) || !pos.TryGetValue(a.To, out var pb)) continue;
            double x1 = pa.X + NW / 2.0, y1 = pa.Y + NH, x2 = pb.X + NW / 2.0, y2 = pb.Y, my = (y1 + y2) / 2;
            s.Append($"<path d='M {F(x1)} {F(y1)} C {F(x1)} {F(my)}, {F(x2)} {F(my)}, {F(x2)} {F(y2)}' fill='none' stroke='{(a.Benef ? BLUE : GOLD)}' stroke-width='1.6' stroke-opacity='0.85'/>");
        }
        // Beneficiários (linha do topo) → família.
        if (g.Beneficiarios.Count > 0)
        {
            double totalW = g.Beneficiarios.Count * BW + (g.Beneficiarios.Count - 1) * GAPX;
            double startX = (W - totalW) / 2;
            var fam = pos["familia"];
            for (var i = 0; i < g.Beneficiarios.Count; i++)
            {
                var b = g.Beneficiarios[i];
                double x = startX + i * (BW + GAPX), y = PAD;
                double x1 = x + BW / 2.0, y1 = y + BH, x2 = fam.X + NW / 2.0, y2 = fam.Y, my = (y1 + y2) / 2;
                s.Append($"<path d='M {F(x1)} {F(y1)} C {F(x1)} {F(my)}, {F(x2)} {F(my)}, {F(x2)} {F(y2)}' fill='none' stroke='{BLUE}' stroke-width='1.1' stroke-opacity='0.6' stroke-dasharray='4 4'/>");
                s.Append($"<rect x='{F(x)}' y='{F(y)}' width='{BW}' height='{BH}' rx='8' fill='#eef4fb' stroke='{BLUE}' stroke-width='1.4'/>");
                s.Append($"<text x='{F(x + BW / 2.0)}' y='{F(y + 15)}' font-size='9.5' font-weight='bold' fill='#1f2937' text-anchor='middle' font-family='{FF}'>{Esc(Trunc(b.Nome, 14))}</text>");
                s.Append($"<text x='{F(x + BW / 2.0)}' y='{F(y + 27)}' font-size='8' fill='#6b7280' text-anchor='middle' font-family='{FF}'>{Esc(Papel.GetValueOrDefault(b.Papel, ""))} · {b.PercentualDistribuicao.ToString("0", Inv)}%</text>");
            }
        }
        // Família.
        DesenhaNo(s, pos["familia"], NW, NH, "Família", g.Beneficiarios.Count > 0 ? $"{g.Beneficiarios.Count} beneficiário(s)" : "Família", BLUE, "#eef4fb", "#111827", FF, Esc, Trunc, 2.0);
        // Estruturas.
        foreach (var e in estruturas)
        {
            if (!pos.TryGetValue(e.Id.ToString(), out var p)) continue;
            DesenhaNo(s, p, NW, NH, Trunc(e.Nome, 22), MoneyCurto(e.ValorTotalBRL), GOLD, "#ffffff", "#111827", FF, Esc, Trunc, 1.4);
        }
        s.Append("</svg>");
        return s.ToString();
    }

    private static void DesenhaNo(StringBuilder s, (double X, double Y) p, int w, int h, string titulo, string sub, string borda, string fill, string cor, string ff, Func<string, string> esc, Func<string, int, string> trunc, double bw)
    {
        string F(double v) => v.ToString("0.#", Inv);
        s.Append($"<rect x='{F(p.X)}' y='{F(p.Y)}' width='{w}' height='{h}' rx='9' fill='{fill}' stroke='{borda}' stroke-width='{F(bw)}'/>");
        s.Append($"<text x='{F(p.X + 10)}' y='{F(p.Y + 19)}' font-size='11' font-weight='bold' fill='{cor}' font-family='{ff}'>{esc(titulo)}</text>");
        s.Append($"<text x='{F(p.X + 10)}' y='{F(p.Y + 34)}' font-size='9.5' fill='#9ca3af' font-family='{ff}'>{esc(sub)}</text>");
    }

    // ── SVG: trilha do plano (igual ao patrimonial) ─────────────────────────
    private static string TrilhaSvg(PlanoAcaoDto plano)
    {
        var etapas = plano.Etapas.OrderBy(e => e.Ordem).ToList();
        const int W = 720, H = 190;
        int n = etapas.Count + 1;
        bool labels = n <= 7;
        double padL = 34, padR = 74, padT = labels ? 46 : 24, padB = labels ? 40 : 20;
        double yB = H - padB, yT = padT;
        double X(int i) => n == 1 ? padL : padL + i * (W - padL - padR) / (n - 1);
        double Y(int i) => n == 1 ? yB : yB - i * (yB - yT) / (n - 1);
        string F(double v) => v.ToString("0.#", Inv);
        string Esc(string t) => t.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        string Trunc(string t, int m) => t.Length > m ? t[..(m - 1)] + "…" : t;
        const string FF = "Lato, Arial, sans-serif";

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
            var sb = new StringBuilder(); double inner = r * 0.42;
            for (int k = 0; k < 10; k++)
            {
                double ang = -Math.PI / 2 + k * Math.PI / 5, rr = k % 2 == 0 ? r : inner;
                sb.Append((k == 0 ? "M " : " L ") + $"{F(cx + rr * Math.Cos(ang))} {F(cy + rr * Math.Sin(ang))}");
            }
            return sb.Append(" Z").ToString();
        }

        bool todas = etapas.Count > 0 && etapas.All(e => e.Status == 3);
        int goldEnd = 0;
        for (int i = 0; i < etapas.Count; i++) if (etapas[i].Status != 1) goldEnd = i;
        if (todas) goldEnd = n - 1;

        var s = new StringBuilder($"<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 {W} {H}' width='{W}' height='{H}'>");
        s.Append("<defs><linearGradient id='gs' x1='0' y1='0' x2='1' y2='0'><stop offset='0' stop-color='#E7C57E'/><stop offset='1' stop-color='#C79A4E'/></linearGradient></defs>");
        s.Append($"<path d='{Smooth(n - 1)} L {F(X(n - 1))} {H} L {F(X(0))} {H} Z' fill='#C79A4E' fill-opacity='0.08'/>");
        s.Append($"<path d='{Smooth(n - 1)}' fill='none' stroke='#d1d5db' stroke-width='4' stroke-linecap='round' stroke-dasharray='1 12'/>");
        if (goldEnd >= 1) s.Append($"<path d='{Smooth(goldEnd)}' fill='none' stroke='url(#gs)' stroke-width='4.5' stroke-linecap='round'/>");
        for (int i = 0; i < etapas.Count; i++)
        {
            var e = etapas[i]; double cx = X(i), cy = Y(i); bool cc = e.Status == 3, at = e.Status == 2;
            if (at) s.Append($"<circle cx='{F(cx)}' cy='{F(cy)}' r='16' fill='#C79A4E' fill-opacity='0.18'/>");
            s.Append($"<circle cx='{F(cx)}' cy='{F(cy)}' r='{(cc ? 13 : 12)}' fill='{(cc ? "#C79A4E" : "#ffffff")}' stroke='{(cc ? "#C79A4E" : at ? "#C79A4E" : "#d1d5db")}' stroke-width='{F(cc ? 0 : at ? 3.2 : 2.5)}'/>");
            if (cc) s.Append($"<path d='M {F(cx - 6)} {F(cy)} l 4 4 l 8 -9' fill='none' stroke='#241a08' stroke-width='2.4' stroke-linecap='round' stroke-linejoin='round'/>");
            else s.Append($"<text x='{F(cx)}' y='{F(cy + 4)}' font-size='12' font-weight='bold' fill='{(at ? "#9a6c22" : "#9ca3af")}' text-anchor='middle' font-family='{FF}'>{i + 1}</text>");
            if (labels)
            {
                s.Append($"<text x='{F(cx)}' y='{F(cy - 22)}' font-size='11' font-weight='bold' fill='{(cc || at ? "#374151" : "#9ca3af")}' text-anchor='middle' font-family='{FF}'>{Esc(Trunc(e.Titulo, 14))}</text>");
                if (!string.IsNullOrWhiteSpace(e.Prazo)) s.Append($"<text x='{F(cx)}' y='{F(cy + 26)}' font-size='10' fill='#9ca3af' text-anchor='middle' font-family='{FF}'>{Esc(e.Prazo!)}</text>");
            }
        }
        double gx = X(n - 1), gy = Y(n - 1);
        s.Append($"<circle cx='{F(gx)}' cy='{F(gy)}' r='16' fill='{(todas ? "#C79A4E" : "#ffffff")}' stroke='#C79A4E' stroke-width='2'/>");
        s.Append($"<path d='{Star(gx, gy, 8)}' fill='{(todas ? "#241a08" : "#C79A4E")}'/>");
        if (labels)
        {
            s.Append($"<text x='{F(gx)}' y='{F(gy - 24)}' font-size='11' font-weight='bold' fill='#9a6c22' text-anchor='middle' font-family='{FF}'>Objetivo</text>");
            if (!string.IsNullOrWhiteSpace(plano.Prazo)) s.Append($"<text x='{F(gx)}' y='{F(gy + 30)}' font-size='10' fill='#9ca3af' text-anchor='middle' font-family='{FF}'>{Esc(plano.Prazo!)}</text>");
        }
        s.Append("</svg>");
        return s.ToString();
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

    private static void Th(IContainer c, string texto, bool right = false)
    {
        var t = c.BorderBottom(1).BorderColor("#e5e7eb").PaddingVertical(4);
        (right ? t.AlignRight() : t).Text(texto).FontSize(8).Bold().FontColor("#6b7280");
    }
}
