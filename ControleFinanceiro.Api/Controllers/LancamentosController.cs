using ControleFinanceiro.Application.Lancamentos.Commands.AtualizarSituacao;
using ControleFinanceiro.Application.Lancamentos.Commands.DeleteParcelasFuturas;
using ControleFinanceiro.Application.Lancamentos.Commands.CreateLancamento;
using ControleFinanceiro.Application.Lancamentos.Commands.DeleteLancamento;
using ControleFinanceiro.Application.Lancamentos.Commands.UpdateLancamento;
using ControleFinanceiro.Application.Lancamentos.Queries.GetDashboard;
using ControleFinanceiro.Application.Lancamentos.Queries.GetLancamentosByMes;
using ControleFinanceiro.Application.Lancamentos.Queries.GetParceladosVigentes;
using ControleFinanceiro.Application.Lancamentos.Queries.GetResumoAnual;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LancamentosController(IMediator mediator) : ControllerBase
{
    [HttpGet("{mes}/{ano}")]
    public async Task<IActionResult> GetByMes(int mes, int ano, CancellationToken ct)
        => Ok(await mediator.Send(new GetLancamentosByMesQuery(mes, ano), ct));

    [HttpGet("dashboard/{mes}/{ano}")]
    public async Task<IActionResult> GetDashboard(int mes, int ano, CancellationToken ct)
        => Ok(await mediator.Send(new GetDashboardQuery(mes, ano), ct));

    [HttpGet("parcelados-vigentes")]
    public async Task<IActionResult> GetParceladosVigentes(CancellationToken ct)
        => Ok(await mediator.Send(new GetParceladosVigentesQuery(), ct));

    [HttpGet("resumo-anual/{ano}")]
    public async Task<IActionResult> GetResumoAnual(int ano, CancellationToken ct)
        => Ok(await mediator.Send(new GetResumoAnualQuery(ano), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLancamentoCommand command, CancellationToken ct)
    {
        var id = await mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetByMes), new { mes = command.Mes, ano = command.Ano }, new { id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLancamentoCommand command, CancellationToken ct)
    {
        await mediator.Send(command with { Id = id }, ct);
        return NoContent();
    }

    [HttpPatch("{id}/situacao")]
    public async Task<IActionResult> AtualizarSituacao(Guid id, [FromBody] AtualizarSituacaoCommand command, CancellationToken ct)
    {
        await mediator.Send(command with { Id = id }, ct);
        return NoContent();
    }

    [HttpPatch("{id}/situacao-com-conta")]
    public async Task<IActionResult> AtualizarSituacaoComConta(Guid id, [FromBody] AtualizarSituacaoCommand command, CancellationToken ct)
    {
        await mediator.Send(command with { Id = id }, ct);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteLancamentoCommand(id), ct);
        return NoContent();
    }

    [HttpDelete("parcelas-futuras/{grupoParcelas}/{parcelaAtualFrom}")]
    public async Task<IActionResult> DeleteParcelasFuturas(Guid grupoParcelas, int parcelaAtualFrom, CancellationToken ct)
    {
        await mediator.Send(new DeleteParcelasFuturasCommand(grupoParcelas, parcelaAtualFrom), ct);
        return NoContent();
    }
}
