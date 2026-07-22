using ControleFinanceiro.Application.Patrimonio.Commands.CreateInvestimento;
using ControleFinanceiro.Application.Patrimonio.Commands.DeleteInvestimento;
using ControleFinanceiro.Application.Patrimonio.Commands.UpdateInvestimento;
using ControleFinanceiro.Application.Patrimonio.Commands.AtualizarPrecosInvestimentos;
using ControleFinanceiro.Application.Patrimonio.Queries.GetResumoInvestimentos;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.Api.Controllers;

public record InvestimentoRequest(
    string Nome,
    TipoInvestimento Tipo,
    string Moeda,
    string? Corretora,
    string? Ticker,
    decimal ValorAplicado,
    decimal ValorAtual,
    decimal? RentabilidadeAnualPct,
    decimal? Quantidade,
    Guid? EstruturaId,
    Guid? ContaId,
    string? Subclasse);

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class InvestimentosController(IMediator mediator, IPrecoAtivoHistoricoRepository precoRepo) : ControllerBase
{
    private static readonly Dictionary<string, MoedaPatrimonio> MoedaMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["BRL"] = MoedaPatrimonio.BRL,
        ["USD"] = MoedaPatrimonio.USD,
        ["EUR"] = MoedaPatrimonio.EUR,
        ["CHF"] = MoedaPatrimonio.CHF,
        ["GBP"] = MoedaPatrimonio.GBP,
    };

    [HttpGet("resumo")]
    public async Task<IActionResult> GetResumo(CancellationToken ct) =>
        Ok(await mediator.Send(new GetResumoInvestimentosQuery(), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] InvestimentoRequest request, CancellationToken ct)
    {
        if (!MoedaMap.TryGetValue(request.Moeda, out var moeda))
            return BadRequest($"Moeda invalida: {request.Moeda}.");

        var id = await mediator.Send(
            new CreateInvestimentoCommand(request.Nome, request.Tipo, moeda, request.Corretora,
                request.Ticker, request.ValorAplicado, request.ValorAtual, request.RentabilidadeAnualPct, request.Quantidade, request.EstruturaId, request.ContaId, request.Subclasse), ct);

        return CreatedAtAction(nameof(GetResumo), new { }, new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] InvestimentoRequest request, CancellationToken ct)
    {
        if (!MoedaMap.TryGetValue(request.Moeda, out var moeda))
            return BadRequest($"Moeda invalida: {request.Moeda}.");

        await mediator.Send(
            new UpdateInvestimentoCommand(id, request.Nome, request.Tipo, moeda, request.Corretora,
                request.Ticker, request.ValorAplicado, request.ValorAtual, request.RentabilidadeAnualPct, request.Quantidade, request.EstruturaId, request.ContaId, request.Subclasse), ct);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteInvestimentoCommand(id), ct);
        return NoContent();
    }

    /// <summary>Atualiza os preços dos investimentos (com ticker) do cliente efetivo via mercado.</summary>
    [HttpPost("atualizar-precos")]
    public async Task<IActionResult> AtualizarPrecos(CancellationToken ct)
    {
        var r = await mediator.Send(new AtualizarPrecosInvestimentosCommand(true), ct);
        return Ok(new { atualizados = r.Atualizados });
    }

    /// <summary>Histórico de preço de um ticker.</summary>
    [HttpGet("historico/{ticker}")]
    public async Task<IActionResult> GetHistorico(string ticker, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 10, CancellationToken ct = default)
    {
        tamanhoPagina = Math.Clamp(tamanhoPagina, 1, 50);
        var (items, total) = await precoRepo.GetByTickerAsync(ticker.ToUpperInvariant(), pagina, tamanhoPagina, ct);
        return Ok(new
        {
            pagina, tamanhoPagina, total,
            totalPaginas = (int)Math.Ceiling((double)total / tamanhoPagina),
            items = items.Select(h => new { h.Ticker, h.Preco, h.Fonte, h.DataHora }),
        });
    }
}
