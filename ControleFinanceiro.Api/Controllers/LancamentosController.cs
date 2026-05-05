using ControleFinanceiro.Application.Lancamentos.Commands.AtualizarSituacao;
using ControleFinanceiro.Application.Lancamentos.Commands.CreateTransferencia;
using ControleFinanceiro.Application.Lancamentos.Commands.DeleteGrupoParcelas;
using ControleFinanceiro.Application.Lancamentos.Commands.DeleteParcelasFuturas;
using ControleFinanceiro.Application.Lancamentos.Commands.CreateLancamento;
using ControleFinanceiro.Application.Lancamentos.Commands.DeleteLancamento;
using ControleFinanceiro.Application.Lancamentos.Commands.UpdateLancamento;
using ControleFinanceiro.Application.Lancamentos.Commands.UpdateLancamentoRecorrenteFuturas;
using ControleFinanceiro.Application.Lancamentos.Queries.GetDashboard;
using ControleFinanceiro.Application.Lancamentos.Queries.GetAnaliseDividas;
using ControleFinanceiro.Application.Lancamentos.Queries.GetDicas;
using ControleFinanceiro.Application.Lancamentos.Queries.GetLancamentosBusca;
using ControleFinanceiro.Application.Lancamentos.Queries.GetLancamentosByMes;
using ControleFinanceiro.Application.Lancamentos.Queries.GetParceladosVigentes;
using ControleFinanceiro.Application.Lancamentos.Queries.GetProjecao;
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
    public async Task<IActionResult> GetByMes(int mes, int ano, [FromQuery] int page = 1, [FromQuery] int pageSize = 200, CancellationToken ct = default)
        => Ok(await mediator.Send(new GetLancamentosByMesQuery(mes, ano, page, pageSize), ct));

    [HttpGet("dashboard/{mes}/{ano}")]
    public async Task<IActionResult> GetDashboard(int mes, int ano, CancellationToken ct)
        => Ok(await mediator.Send(new GetDashboardQuery(mes, ano), ct));

    [HttpGet("dicas/{mes}/{ano}")]
    public async Task<IActionResult> GetDicas(int mes, int ano, CancellationToken ct)
        => Ok(await mediator.Send(new GetDicasQuery(mes, ano), ct));

    [HttpGet("parcelados-vigentes")]
    public async Task<IActionResult> GetParceladosVigentes(CancellationToken ct)
        => Ok(await mediator.Send(new GetParceladosVigentesQuery(), ct));

    [HttpGet("parcelados-vigentes/analise")]
    public async Task<IActionResult> GetAnaliseDividas(CancellationToken ct)
        => Ok(await mediator.Send(new GetAnaliseDividasQuery(), ct));

    [HttpGet("resumo-anual/{ano}")]
    public async Task<IActionResult> GetResumoAnual(int ano, CancellationToken ct)
        => Ok(await mediator.Send(new GetResumoAnualQuery(ano), ct));

    [HttpGet("projecao/{mes}/{ano}")]
    public async Task<IActionResult> GetProjecao(int mes, int ano, CancellationToken ct)
        => Ok(await mediator.Send(new GetProjecaoQuery(mes, ano), ct));

    [HttpGet("busca")]
    public async Task<IActionResult> Busca([FromQuery] string q, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
            return BadRequest("Informe ao menos 2 caracteres para buscar.");

        return Ok(await mediator.Send(new GetLancamentosBuscaQuery(q, page, pageSize), ct));
    }

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

    [HttpPut("{id}/recorrente-futuras")]
    public async Task<IActionResult> UpdateRecorrenteFuturas(Guid id, [FromBody] UpdateLancamentoRecorrenteFuturasCommand command, CancellationToken ct)
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

    [HttpDelete("grupo/{grupoParcelas}")]
    public async Task<IActionResult> DeleteGrupoParcelas(Guid grupoParcelas, CancellationToken ct)
    {
        await mediator.Send(new DeleteGrupoParcelasCommand(grupoParcelas), ct);
        return NoContent();
    }

    [HttpPost("transferencia")]
    public async Task<IActionResult> CreateTransferencia(
        [FromBody] CreateTransferenciaCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return Ok(result);
    }
}
