using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Patrimonio.Queries.GetEstruturas;

/// <summary>Um bem/investimento pendurado diretamente na estrutura.</summary>
public record ItemEstruturaDto(string Nome, string Origem, int Tipo, string Moeda, decimal Valor, decimal ValorBRL);

/// <summary>Uma estrutura detida por esta (participação para baixo).</summary>
public record FilhaEstruturaDto(Guid Id, string Nome, decimal PercentualParticipacao, decimal ValorTotalBRL, decimal ValorParticipacaoBRL);

public record EstruturaDetalheDto(
    Guid Id, string Nome, int Tipo, string? Jurisdicao, string? Observacoes,
    decimal ValorDiretoBRL, decimal ValorTotalBRL,
    IReadOnlyList<ItemEstruturaDto> Itens,
    IReadOnlyList<FilhaEstruturaDto> Filhas);

public record GetEstruturaDetalheQuery(Guid Id) : IRequest<EstruturaDetalheDto>;

public class GetEstruturaDetalheQueryHandler(
    IEstruturaRepository estruturaRepo,
    IAtivoPatrimonialRepository ativoRepo,
    IInvestimentoRepository investimentoRepo,
    IFxRateResolver fxResolver,
    ICurrentUser currentUser)
    : IRequestHandler<GetEstruturaDetalheQuery, EstruturaDetalheDto>
{
    public async Task<EstruturaDetalheDto> Handle(GetEstruturaDetalheQuery request, CancellationToken ct)
    {
        var userId = currentUser.UserId;

        var estrutura = await estruturaRepo.GetByIdAsync(request.Id, ct)
            ?? throw new KeyNotFoundException($"Estrutura {request.Id} não encontrada.");
        if (estrutura.UsuarioId != userId)
            throw new UnauthorizedAccessException("Acesso negado à estrutura.");

        var estruturas    = await estruturaRepo.GetByUsuarioAsync(userId, ct);
        var participacoes = await estruturaRepo.GetParticipacoesByUsuarioAsync(userId, ct);
        var ativos        = (await ativoRepo.GetByUsuarioAsync(userId, ct)).ToList();
        var investimentos = (await investimentoRepo.GetByUsuarioAsync(userId, ct)).ToList();
        var fx            = await fxResolver.GetRatesAsync(ct);

        decimal ParaBRL(decimal v, MoedaPatrimonio moeda) =>
            moeda == MoedaPatrimonio.BRL ? v : v * (fx.TryGetValue(moeda.ToString(), out var r) && r > 0 ? r : 1m);

        // Itens ligados diretamente à estrutura.
        var itens = new List<ItemEstruturaDto>();
        foreach (var a in ativos.Where(a => a.EstruturaId == estrutura.Id))
            itens.Add(new ItemEstruturaDto(a.Nome, "ativo", (int)a.Tipo, a.Moeda.ToString(),
                Math.Round(a.ValorAtual, 2), Math.Round(ParaBRL(a.ValorAtual, a.Moeda), 2)));
        foreach (var i in investimentos.Where(i => i.EstruturaId == estrutura.Id))
            itens.Add(new ItemEstruturaDto(i.Nome, "investimento", (int)i.Tipo, i.Moeda.ToString(),
                Math.Round(i.ValorAtual, 2), Math.Round(ParaBRL(i.ValorAtual, i.Moeda), 2)));

        // Valor total (derivado, recursivo) de qualquer estrutura.
        var valorDireto = estruturas.ToDictionary(e => e.Id, _ => 0m);
        foreach (var a in ativos.Where(a => a.EstruturaId.HasValue && valorDireto.ContainsKey(a.EstruturaId!.Value)))
            valorDireto[a.EstruturaId!.Value] += ParaBRL(a.ValorAtual, a.Moeda);
        foreach (var i in investimentos.Where(i => i.EstruturaId.HasValue && valorDireto.ContainsKey(i.EstruturaId!.Value)))
            valorDireto[i.EstruturaId!.Value] += ParaBRL(i.ValorAtual, i.Moeda);

        var filhasPorPai = participacoes
            .Where(p => p.EstruturaPaiId.HasValue)
            .GroupBy(p => p.EstruturaPaiId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var memo = new Dictionary<Guid, decimal>();
        decimal ValorTotal(Guid id, HashSet<Guid> caminho)
        {
            if (memo.TryGetValue(id, out var m)) return m;
            if (!caminho.Add(id)) return 0m;
            var total = valorDireto.GetValueOrDefault(id);
            if (filhasPorPai.TryGetValue(id, out var fs))
                foreach (var p in fs)
                    total += ValorTotal(p.EstruturaFilhaId, caminho) * p.PercentualParticipacao / 100m;
            caminho.Remove(id);
            memo[id] = total;
            return total;
        }

        var nomePorId = estruturas.ToDictionary(e => e.Id, e => e.Nome);
        var filhas = (filhasPorPai.GetValueOrDefault(estrutura.Id) ?? [])
            .Where(p => nomePorId.ContainsKey(p.EstruturaFilhaId))
            .Select(p =>
            {
                var totalFilha = ValorTotal(p.EstruturaFilhaId, []);
                return new FilhaEstruturaDto(
                    p.EstruturaFilhaId, nomePorId[p.EstruturaFilhaId], p.PercentualParticipacao,
                    Math.Round(totalFilha, 2),
                    Math.Round(totalFilha * p.PercentualParticipacao / 100m, 2));
            })
            .OrderByDescending(f => f.ValorParticipacaoBRL)
            .ToList();

        return new EstruturaDetalheDto(
            estrutura.Id, estrutura.Nome, (int)estrutura.Tipo, estrutura.Jurisdicao, estrutura.Observacoes,
            Math.Round(valorDireto.GetValueOrDefault(estrutura.Id), 2),
            Math.Round(ValorTotal(estrutura.Id, []), 2),
            itens.OrderByDescending(i => i.ValorBRL).ToList(),
            filhas);
    }
}
