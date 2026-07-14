using ControleFinanceiro.Application.Simulacoes;
using ControleFinanceiro.Application.Simulacoes.Commands.CreateSimulacao;
using ControleFinanceiro.Application.Simulacoes.Commands.DeleteSimulacao;
using ControleFinanceiro.Application.Simulacoes.Commands.UpdateSimulacao;
using ControleFinanceiro.Application.Simulacoes.Queries.GetSimulacoes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.Api.Controllers;

/// <summary>Request body para criar/editar uma simulação de projeção patrimonial.</summary>
public record SimulacaoRequest(
    string Nome,
    bool Favorita,
    int IdadeAtual,
    int IdadeAlvo,
    decimal PatrimonioInicial,
    bool ModoAutomatico,
    decimal AporteMensal,
    decimal TaxaRetornoRealAnualPct,
    decimal RetiradaMensal,
    IReadOnlyList<CenarioInput>? Cenarios);

/// <summary>
/// Proteção patrimonial: simulações de projeção de longo prazo (acúmulo + decumulação).
/// O cálculo roda no cliente; aqui só persistimos parâmetros e cenários do usuário efetivo.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class SimulacoesController(IMediator mediator) : ControllerBase
{
    /// <summary>Lista as simulações salvas do usuário efetivo (favoritas primeiro).</summary>
    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetSimulacoesQuery(), cancellationToken));

    /// <summary>Salva uma nova simulação.</summary>
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] SimulacaoRequest request, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(new CreateSimulacaoCommand(
            request.Nome, request.Favorita, request.IdadeAtual, request.IdadeAlvo,
            request.PatrimonioInicial, request.ModoAutomatico, request.AporteMensal,
            request.TaxaRetornoRealAnualPct, request.RetiradaMensal,
            request.Cenarios ?? []), cancellationToken);

        return CreatedAtAction(nameof(Listar), new { }, new { id });
    }

    /// <summary>Atualiza uma simulação existente.</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] SimulacaoRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new UpdateSimulacaoCommand(
            id, request.Nome, request.Favorita, request.IdadeAtual, request.IdadeAlvo,
            request.PatrimonioInicial, request.ModoAutomatico, request.AporteMensal,
            request.TaxaRetornoRealAnualPct, request.RetiradaMensal,
            request.Cenarios ?? []), cancellationToken);

        return NoContent();
    }

    /// <summary>Remove uma simulação.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remover(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteSimulacaoCommand(id), cancellationToken);
        return NoContent();
    }
}
