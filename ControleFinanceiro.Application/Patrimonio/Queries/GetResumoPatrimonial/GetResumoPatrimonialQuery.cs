using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Patrimonio.Queries.GetResumoPatrimonial;

public record AtivoResumoDto(
    Guid Id,
    string Nome,
    int Tipo,
    string Moeda,
    decimal ValorAtual,
    decimal? ValorizacaoAnualPct,
    decimal ReceitaMensal,
    decimal DespesaMensal,
    decimal FluxoLiquidoMensal,
    decimal? RoiAnualPct,        // retorno total anual = yield + valorização
    decimal? YieldAnualPct,      // só o fluxo de caixa (receita − despesa) / valor
    Guid? EstruturaId);          // estrutura à qual o bem pertence (null = pessoa física)

public record PassivoResumoDto(
    Guid Id,
    string Nome,
    string Moeda,
    decimal Valor,
    int Prazo,
    decimal ValorBRL);

/// <summary>Uma fatia da composição patrimonial (por categoria de bem), já em BRL.</summary>
public record CategoriaComposicaoDto(string Categoria, decimal TotalBRL, decimal Pct, decimal? RoiAnualPct);

public record TotalPorMoedaDto(string Moeda, decimal Total, int Quantidade);

/// <summary>
/// Balanço patrimonial consolidado (em BRL, câmbio estub):
/// Bens − Dívidas = Patrimônio Líquido, com composição por categoria,
/// fluxo de caixa mensal e ROI. Não inclui o módulo de Investimentos
/// (consolidação própria) para evitar dupla contagem.
/// </summary>
public record ResumoPatrimonialDto(
    int QtdAtivos,
    decimal TotalBensBRL,
    decimal TotalDividasBRL,
    decimal PatrimonioLiquidoBRL,
    decimal AlavancagemPct,
    decimal ReceitaMensalBRL,
    decimal DespesaMensalBRL,
    decimal SaldoLiquidoMensalBRL,
    decimal? RoiAnualPct,
    IEnumerable<CategoriaComposicaoDto> Composicao,
    IEnumerable<TotalPorMoedaDto> TotaisPorMoeda,
    decimal TotalConsolidadoBRL,   // = TotalBensBRL (mantido por compatibilidade)
    bool CambioEstimado,           // true enquanto o câmbio for estub (não tempo real)
    IEnumerable<AtivoResumoDto> Ativos,
    IEnumerable<PassivoResumoDto> Passivos)
{
    public ResumoPatrimonialDto()
        : this(0, 0m, 0m, 0m, 0m, 0m, 0m, 0m, null, [], [], 0m, true, [], [])
    {
    }
}

public record GetResumoPatrimonialQuery : IRequest<ResumoPatrimonialDto>;

public class GetResumoPatrimonialQueryHandler(
    IAtivoPatrimonialRepository ativoRepository,
    IPassivoPatrimonialRepository passivoRepository,
    IFxRateResolver fxResolver,
    IPatrimonioSnapshotRepository snapshotRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<GetResumoPatrimonialQuery, ResumoPatrimonialDto>
{
    private static readonly Dictionary<TipoAtivo, string> CategoriaLabel = new()
    {
        [TipoAtivo.Imovel]       = "Imóveis",
        [TipoAtivo.Veiculo]      = "Veículos",
        [TipoAtivo.Embarcacao]   = "Embarcações",
        [TipoAtivo.Aeronave]     = "Aeronaves",
        [TipoAtivo.Participacao] = "Participações Societárias",
        [TipoAtivo.Investimento] = "Investimentos",
        [TipoAtivo.Outro]        = "Outros",
    };

    /// <summary>Retorno total anual = yield (fluxo de caixa / valor) + valorização anual.</summary>
    private static decimal? RetornoTotal(decimal valorBRL, decimal fluxoAnualBRL, decimal? valorizacaoPct)
    {
        if (valorBRL <= 0) return null;
        var rendimento = fluxoAnualBRL / valorBRL * 100m;
        return Math.Round(rendimento + (valorizacaoPct ?? 0m), 2);
    }

    public async Task<ResumoPatrimonialDto> Handle(GetResumoPatrimonialQuery request, CancellationToken cancellationToken)
    {
        var ativos   = (await ativoRepository.GetByUsuarioAsync(currentUser.UserId, cancellationToken)).ToList();
        var passivos = (await passivoRepository.GetByUsuarioAsync(currentUser.UserId, cancellationToken)).ToList();

        // Câmbio definido pelo assessor em Cadastros → Moedas (CotacaoBRL).
        var fx = await fxResolver.GetRatesAsync(cancellationToken);
        decimal ParaBRL(decimal valor, MoedaPatrimonio moeda) =>
            moeda == MoedaPatrimonio.BRL ? valor
            : valor * (fx.TryGetValue(moeda.ToString(), out var r) && r > 0 ? r : 1m);

        // ── Bens ──
        var totalBensBRL     = ativos.Sum(a => ParaBRL(a.ValorAtual, a.Moeda));
        var receitaMensalBRL = ativos.Sum(a => ParaBRL(a.ReceitaMensal, a.Moeda));
        var despesaMensalBRL = ativos.Sum(a => ParaBRL(a.DespesaMensal, a.Moeda));
        var fluxoAnualBRL    = (receitaMensalBRL - despesaMensalBRL) * 12m;

        // ── Composição por categoria (em BRL, com % e ROI) ──
        var composicao = ativos
            .GroupBy(a => a.Tipo)
            .Select(g =>
            {
                var totalCat      = g.Sum(a => ParaBRL(a.ValorAtual, a.Moeda));
                var fluxoAnualCat = g.Sum(a => ParaBRL(a.ReceitaMensal - a.DespesaMensal, a.Moeda)) * 12m;
                var valorizMedia  = g.Where(a => a.ValorizacaoAnualPct != null).Select(a => a.ValorizacaoAnualPct!.Value).ToList();
                var valoriz       = valorizMedia.Count > 0 ? valorizMedia.Average() : (decimal?)null;
                return new CategoriaComposicaoDto(
                    CategoriaLabel.GetValueOrDefault(g.Key, "Outros"),
                    Math.Round(totalCat, 2),
                    totalBensBRL > 0 ? Math.Round(totalCat / totalBensBRL * 100m, 1) : 0m,
                    RetornoTotal(totalCat, fluxoAnualCat, valoriz));
            })
            .OrderByDescending(c => c.TotalBRL)
            .ToList();

        // ── Dívidas ──
        var totalDividasBRL = passivos.Sum(p => ParaBRL(p.Valor, p.Moeda));
        var passivosDto = passivos
            .Select(p => new PassivoResumoDto(
                p.Id, p.Nome, p.Moeda.ToString(), p.Valor, (int)p.Prazo,
                Math.Round(ParaBRL(p.Valor, p.Moeda), 2)))
            .OrderByDescending(p => p.ValorBRL)
            .ToList();

        // ── Totais por moeda (bens) ──
        var totaisPorMoeda = ativos
            .GroupBy(a => a.Moeda)
            .Select(g => new TotalPorMoedaDto(g.Key.ToString(), g.Sum(a => a.ValorAtual), g.Count()))
            .OrderByDescending(t => t.Total)
            .ToList();

        // ── Ativos detalhados ──
        var ativosDto = ativos.Select(a =>
        {
            var valorBRL   = ParaBRL(a.ValorAtual, a.Moeda);
            var fluxoAnual = ParaBRL(a.ReceitaMensal - a.DespesaMensal, a.Moeda) * 12m;
            var yieldPct   = valorBRL > 0 ? Math.Round(fluxoAnual / valorBRL * 100m, 2) : (decimal?)null;
            return new AtivoResumoDto(
                a.Id, a.Nome, (int)a.Tipo, a.Moeda.ToString(),
                a.ValorAtual, a.ValorizacaoAnualPct,
                a.ReceitaMensal, a.DespesaMensal,
                a.ReceitaMensal - a.DespesaMensal,
                RetornoTotal(valorBRL, fluxoAnual, a.ValorizacaoAnualPct),
                yieldPct,
                a.EstruturaId);
        }).ToList();

        var patrimonioLiquido = totalBensBRL - totalDividasBRL;

        // Captura preguiçosa: registra/atualiza o snapshot do mês corrente (só quando há patrimônio).
        if (ativos.Count > 0 || passivos.Count > 0)
        {
            var hoje = DateTime.UtcNow;
            var liq  = Math.Round(patrimonioLiquido, 2);
            var bens = Math.Round(totalBensBRL, 2);
            var div  = Math.Round(totalDividasBRL, 2);
            var snap = await snapshotRepository.GetByUsuarioMesAsync(currentUser.UserId, hoje.Year, hoje.Month, cancellationToken);
            if (snap is null)
                await snapshotRepository.AddAsync(
                    PatrimonioSnapshot.Criar(currentUser.UserId, hoje.Year, hoje.Month, liq, bens, div), cancellationToken);
            else
            {
                snap.Atualizar(liq, bens, div);
                snapshotRepository.Update(snap);
            }
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var alavancagem       = totalBensBRL > 0 ? totalDividasBRL / totalBensBRL * 100m : 0m;
        // Retorno total geral (blended) = (fluxo anual + valorização anual em R$) / total de bens.
        var valorizacaoAnualBRL = ativos.Sum(a => ParaBRL(a.ValorAtual, a.Moeda) * (a.ValorizacaoAnualPct ?? 0m) / 100m);
        var roiGeral          = totalBensBRL > 0
            ? Math.Round((fluxoAnualBRL + valorizacaoAnualBRL) / totalBensBRL * 100m, 2)
            : (decimal?)null;

        return new ResumoPatrimonialDto(
            QtdAtivos: ativos.Count,
            TotalBensBRL: Math.Round(totalBensBRL, 2),
            TotalDividasBRL: Math.Round(totalDividasBRL, 2),
            PatrimonioLiquidoBRL: Math.Round(patrimonioLiquido, 2),
            AlavancagemPct: Math.Round(alavancagem, 1),
            ReceitaMensalBRL: Math.Round(receitaMensalBRL, 2),
            DespesaMensalBRL: Math.Round(despesaMensalBRL, 2),
            SaldoLiquidoMensalBRL: Math.Round(receitaMensalBRL - despesaMensalBRL, 2),
            RoiAnualPct: roiGeral,
            Composicao: composicao,
            TotaisPorMoeda: totaisPorMoeda,
            TotalConsolidadoBRL: Math.Round(totalBensBRL, 2),
            CambioEstimado: true,
            Ativos: ativosDto,
            Passivos: passivosDto);
    }
}
