using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Patrimonio.Queries.GetEstruturas;

public record EstruturaDto(
    Guid Id,
    string Nome,
    int Tipo,
    string? Jurisdicao,
    DateTime? ConstituidaEm,
    string? Observacoes,
    int QtdAtivos,
    int QtdInvestimentos,
    /// <summary>Soma (BRL) dos ativos + investimentos ligados diretamente à estrutura.</summary>
    decimal ValorDiretoBRL,
    /// <summary>Valor direto + percentual das estruturas detidas (derivado, recursivo).</summary>
    decimal ValorTotalBRL,
    double? PosX,
    double? PosY);

public record ParticipacaoDto(
    Guid Id,
    Guid? EstruturaPaiId,
    Guid EstruturaFilhaId,
    decimal PercentualParticipacao,
    int TipoRelacao);

public record BeneficiarioGrafoDto(
    Guid Id, string Nome, int Papel, decimal PercentualDistribuicao, string? CondicaoLiberacao);

public record GrafoEstruturasDto(
    decimal TotalEmEstruturasBRL,
    decimal TotalPessoaFisicaBRL,
    IReadOnlyList<EstruturaDto> Estruturas,
    IReadOnlyList<ParticipacaoDto> Participacoes,
    IReadOnlyList<BeneficiarioGrafoDto> Beneficiarios)
{
    public GrafoEstruturasDto() : this(0m, 0m, [], [], []) { }
}

public record GetEstruturasQuery : IRequest<GrafoEstruturasDto>;

public class GetEstruturasQueryHandler(
    IEstruturaRepository estruturaRepo,
    IAtivoPatrimonialRepository ativoRepo,
    IInvestimentoRepository investimentoRepo,
    IContaFinanceiraRepository contaRepo,
    IFxRateResolver fxResolver,
    ICurrentUser currentUser)
    : IRequestHandler<GetEstruturasQuery, GrafoEstruturasDto>
{
    public async Task<GrafoEstruturasDto> Handle(GetEstruturasQuery request, CancellationToken ct)
    {
        var userId = currentUser.UserId;

        var estruturas    = await estruturaRepo.GetByUsuarioAsync(userId, ct);
        var participacoes = await estruturaRepo.GetParticipacoesByUsuarioAsync(userId, ct);
        var ativos        = (await ativoRepo.GetByUsuarioAsync(userId, ct)).ToList();
        var investimentos = (await investimentoRepo.GetByUsuarioAsync(userId, ct)).ToList();
        // Só contas-CAIXA entram no consolidado (custódia já é representada pelos investimentos ligados).
        var contasCaixa   = (await contaRepo.GetByUsuarioAsync(userId, ct))
            .Where(c => c.Tipo != TipoContaFinanceira.InvestimentoCustodia).ToList();
        var fx            = await fxResolver.GetRatesAsync(ct);

        decimal ParaBRL(decimal v, MoedaPatrimonio moeda) =>
            moeda == MoedaPatrimonio.BRL ? v : v * (fx.TryGetValue(moeda.ToString(), out var r) && r > 0 ? r : 1m);

        // Valor DIRETO por estrutura = ativos + investimentos com EstruturaId apontando para ela.
        var valorDireto = estruturas.ToDictionary(e => e.Id, _ => 0m);
        var qtdAtivos   = estruturas.ToDictionary(e => e.Id, _ => 0);
        var qtdInvest   = estruturas.ToDictionary(e => e.Id, _ => 0);

        foreach (var a in ativos.Where(a => a.EstruturaId.HasValue && valorDireto.ContainsKey(a.EstruturaId!.Value)))
        {
            valorDireto[a.EstruturaId!.Value] += ParaBRL(a.ValorAtual, a.Moeda);
            qtdAtivos[a.EstruturaId!.Value]++;
        }
        foreach (var i in investimentos.Where(i => i.EstruturaId.HasValue && valorDireto.ContainsKey(i.EstruturaId!.Value)))
        {
            valorDireto[i.EstruturaId!.Value] += ParaBRL(i.ValorAtual, i.Moeda);
            qtdInvest[i.EstruturaId!.Value]++;
        }
        // Caixa das contas ligadas a estruturas soma no valor direto delas.
        foreach (var c in contasCaixa.Where(c => c.EstruturaId.HasValue && valorDireto.ContainsKey(c.EstruturaId!.Value)))
            valorDireto[c.EstruturaId!.Value] += ParaBRL(c.Saldo, c.Moeda);

        // Valor TOTAL = direto + Σ (% × total da filha). Memoizado com guarda anticiclo.
        var filhasPorPai = participacoes
            .Where(p => p.EstruturaPaiId.HasValue)
            .GroupBy(p => p.EstruturaPaiId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var memo = new Dictionary<Guid, decimal>();
        decimal ValorTotal(Guid id, HashSet<Guid> caminho)
        {
            if (memo.TryGetValue(id, out var m)) return m;
            if (!caminho.Add(id)) return 0m; // ciclo (defensivo — Save valida) → corta

            var total = valorDireto.GetValueOrDefault(id);
            if (filhasPorPai.TryGetValue(id, out var filhas))
                foreach (var p in filhas)
                    total += ValorTotal(p.EstruturaFilhaId, caminho) * p.PercentualParticipacao / 100m;

            caminho.Remove(id);
            memo[id] = total;
            return total;
        }

        var dtos = estruturas.Select(e => new EstruturaDto(
            e.Id, e.Nome, (int)e.Tipo, e.Jurisdicao, e.ConstituidaEm, e.Observacoes,
            qtdAtivos[e.Id], qtdInvest[e.Id],
            Math.Round(valorDireto[e.Id], 2),
            Math.Round(ValorTotal(e.Id, []), 2),
            e.PosX, e.PosY)).ToList();

        var totalEstruturas = ativos.Where(a => a.EstruturaId.HasValue).Sum(a => ParaBRL(a.ValorAtual, a.Moeda))
                            + investimentos.Where(i => i.EstruturaId.HasValue).Sum(i => ParaBRL(i.ValorAtual, i.Moeda))
                            + contasCaixa.Where(c => c.EstruturaId.HasValue).Sum(c => ParaBRL(c.Saldo, c.Moeda));
        var totalPF = ativos.Where(a => !a.EstruturaId.HasValue).Sum(a => ParaBRL(a.ValorAtual, a.Moeda))
                    + investimentos.Where(i => !i.EstruturaId.HasValue).Sum(i => ParaBRL(i.ValorAtual, i.Moeda))
                    + contasCaixa.Where(c => !c.EstruturaId.HasValue).Sum(c => ParaBRL(c.Saldo, c.Moeda));

        var partDtos = participacoes.Select(p => new ParticipacaoDto(
            p.Id, p.EstruturaPaiId, p.EstruturaFilhaId, p.PercentualParticipacao, (int)p.TipoRelacao)).ToList();

        var beneficiarios = (await estruturaRepo.GetBeneficiariosByUsuarioAsync(userId, ct))
            .Select(b => new BeneficiarioGrafoDto(b.Id, b.Nome, (int)b.Papel, b.PercentualDistribuicao, b.CondicaoLiberacao))
            .ToList();

        return new GrafoEstruturasDto(
            Math.Round(totalEstruturas, 2), Math.Round(totalPF, 2), dtos, partDtos, beneficiarios);
    }
}
