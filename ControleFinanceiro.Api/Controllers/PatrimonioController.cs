using ControleFinanceiro.Application.Patrimonio.Commands.CreateAtivo;
using ControleFinanceiro.Application.Patrimonio.Commands.DeleteAtivo;
using ControleFinanceiro.Application.Patrimonio.Commands.UpdateAtivo;
using ControleFinanceiro.Application.Patrimonio.Queries.GetResumoPatrimonial;
using ControleFinanceiro.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.Api.Controllers;

/// <summary>Request body para criação/edição de ativo. Aceita moeda como string ("BRL", "USD"…).</summary>
public record AtivoPatrimonialRequest(
    string Nome,
    TipoAtivo Tipo,
    string Moeda,
    decimal ValorAtual,
    decimal? ValorizacaoAnualPct);

/// <summary>
/// Módulo de gestão patrimonial (B2B alta renda). Bounded context isolado.
/// Sob o header X-Assessoria-Cliente, retorna o patrimônio do cliente visualizado.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class PatrimonioController(IMediator mediator) : ControllerBase
{
    private static readonly Dictionary<string, MoedaPatrimonio> MoedaMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["BRL"] = MoedaPatrimonio.BRL,
        ["USD"] = MoedaPatrimonio.USD,
        ["EUR"] = MoedaPatrimonio.EUR,
        ["CHF"] = MoedaPatrimonio.CHF,
        ["GBP"] = MoedaPatrimonio.GBP,
    };

    /// <summary>Resumo consolidado do patrimônio do usuário efetivo.</summary>
    [HttpGet("resumo")]
    public async Task<IActionResult> GetResumo(CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetResumoPatrimonialQuery(), cancellationToken));

    /// <summary>Cadastra um novo ativo patrimonial.</summary>
    [HttpPost("ativos")]
    public async Task<IActionResult> CreateAtivo([FromBody] AtivoPatrimonialRequest request, CancellationToken cancellationToken)
    {
        if (!MoedaMap.TryGetValue(request.Moeda, out var moeda))
            return BadRequest($"Moeda inválida: {request.Moeda}.");

        var id = await mediator.Send(
            new CreateAtivoPatrimonialCommand(request.Nome, request.Tipo, moeda, request.ValorAtual, request.ValorizacaoAnualPct),
            cancellationToken);

        return CreatedAtAction(nameof(GetResumo), new { }, new { id });
    }

    /// <summary>Atualiza um ativo patrimonial existente.</summary>
    [HttpPut("ativos/{id:guid}")]
    public async Task<IActionResult> UpdateAtivo(Guid id, [FromBody] AtivoPatrimonialRequest request, CancellationToken cancellationToken)
    {
        if (!MoedaMap.TryGetValue(request.Moeda, out var moeda))
            return BadRequest($"Moeda inválida: {request.Moeda}.");

        await mediator.Send(
            new UpdateAtivoPatrimonialCommand(id, request.Nome, request.Tipo, moeda, request.ValorAtual, request.ValorizacaoAnualPct),
            cancellationToken);

        return NoContent();
    }

    /// <summary>Remove um ativo patrimonial.</summary>
    [HttpDelete("ativos/{id:guid}")]
    public async Task<IActionResult> DeleteAtivo(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteAtivoPatrimonialCommand(id), cancellationToken);
        return NoContent();
    }
}
