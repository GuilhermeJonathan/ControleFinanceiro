using ControleFinanceiro.Application.Patrimonio.Commands.Estruturas;
using ControleFinanceiro.Application.Patrimonio.Queries.GetEstruturas;
using ControleFinanceiro.Application.Patrimonio.Queries.GetSucessao;
using ControleFinanceiro.Application.Relatorios;
using ControleFinanceiro.Application.Relatorios.Queries.GerarRelatorio;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.Api.Controllers;

public record EstruturaRequest(
    string Nome,
    TipoEstrutura Tipo,
    string? Jurisdicao,
    DateTime? ConstituidaEm,
    string? Observacoes);

public record ParticipacaoRequest(
    Guid? EstruturaPaiId,
    Guid EstruturaFilhaId,
    decimal PercentualParticipacao,
    TipoRelacaoEstrutura TipoRelacao);

public record BeneficiarioRequest(
    string Nome,
    PapelBeneficiario Papel,
    decimal PercentualDistribuicao,
    string? CondicaoLiberacao);

public record DistribuicaoRequest(
    DateTime Data,
    decimal Valor,
    string Moeda,
    Guid? EstruturaId,
    Guid? BeneficiarioId,
    string? Descricao);

/// <summary>Estruturas patrimoniais/sucessórias do cliente (trust, holding, offshore) + grafo de participações.</summary>
[ApiController]
[Authorize]
[Route("api/estruturas")]
public class EstruturasController(IMediator mediator) : ControllerBase
{
    private static readonly Dictionary<string, MoedaPatrimonio> MoedaMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["BRL"] = MoedaPatrimonio.BRL, ["USD"] = MoedaPatrimonio.USD, ["EUR"] = MoedaPatrimonio.EUR,
        ["CHF"] = MoedaPatrimonio.CHF, ["GBP"] = MoedaPatrimonio.GBP,
    };

    /// <summary>Grafo completo: estruturas com valores derivados + participações.</summary>
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct) =>
        Ok(await mediator.Send(new GetEstruturasQuery(), ct));

    /// <summary>Beneficiários e distribuições da família (do cliente).</summary>
    [HttpGet("sucessao")]
    public async Task<IActionResult> GetSucessao(CancellationToken ct) =>
        Ok(await mediator.Send(new GetSucessaoQuery(), ct));

    public record RelatorioSucessaoRequest(string? ClienteNome, string? NomeConsultoria, string? LogoBase64, string? CorMarca);

    /// <summary>Gera o PDF do relatório de sucessão (estrutura, beneficiários, contas, planos).</summary>
    [HttpPost("relatorio")]
    public async Task<IActionResult> GerarRelatorio([FromBody] RelatorioSucessaoRequest req, CancellationToken ct)
    {
        var pdf = await mediator.Send(new GerarRelatorioSucessaoQuery(
            req.ClienteNome, new RelatorioBranding(req.NomeConsultoria, req.LogoBase64, req.CorMarca)), ct);
        return File(pdf, "application/pdf", "relatorio-sucessao.pdf");
    }

    /// <summary>Detalhe de uma estrutura: ativos/investimentos ligados + estruturas detidas.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDetalhe(Guid id, CancellationToken ct) =>
        Ok(await mediator.Send(new GetEstruturaDetalheQuery(id), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] EstruturaRequest req, CancellationToken ct)
    {
        var id = await mediator.Send(new SaveEstruturaCommand(null, req.Nome, req.Tipo,
            req.Jurisdicao, req.ConstituidaEm, req.Observacoes), ct);
        return Ok(new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] EstruturaRequest req, CancellationToken ct)
    {
        await mediator.Send(new SaveEstruturaCommand(id, req.Nome, req.Tipo,
            req.Jurisdicao, req.ConstituidaEm, req.Observacoes), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteEstruturaCommand(id), ct);
        return NoContent();
    }

    public record PosicaoRequest(double PosX, double PosY);

    /// <summary>Salva a posição manual da estrutura no mapa.</summary>
    [HttpPut("{id:guid}/posicao")]
    public async Task<IActionResult> SalvarPosicao(Guid id, [FromBody] PosicaoRequest req, CancellationToken ct)
    {
        await mediator.Send(new SalvarPosicaoEstruturaCommand(id, req.PosX, req.PosY), ct);
        return NoContent();
    }

    // ── Participações (arestas do grafo) ─────────────────────────────────

    [HttpPost("participacoes")]
    public async Task<IActionResult> SaveParticipacao([FromBody] ParticipacaoRequest req, CancellationToken ct)
    {
        var id = await mediator.Send(new SaveParticipacaoCommand(req.EstruturaPaiId,
            req.EstruturaFilhaId, req.PercentualParticipacao, req.TipoRelacao), ct);
        return Ok(new { id });
    }

    [HttpDelete("participacoes/{id:guid}")]
    public async Task<IActionResult> DeleteParticipacao(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteParticipacaoCommand(id), ct);
        return NoContent();
    }

    // ── Beneficiários ────────────────────────────────────────────────────

    [HttpPost("beneficiarios")]
    public async Task<IActionResult> SaveBeneficiario([FromBody] BeneficiarioRequest req, CancellationToken ct)
    {
        var id = await mediator.Send(new SaveBeneficiarioCommand(null, req.Nome,
            req.Papel, req.PercentualDistribuicao, req.CondicaoLiberacao), ct);
        return Ok(new { id });
    }

    [HttpPut("beneficiarios/{id:guid}")]
    public async Task<IActionResult> UpdateBeneficiario(Guid id, [FromBody] BeneficiarioRequest req, CancellationToken ct)
    {
        await mediator.Send(new SaveBeneficiarioCommand(id, req.Nome,
            req.Papel, req.PercentualDistribuicao, req.CondicaoLiberacao), ct);
        return NoContent();
    }

    [HttpDelete("beneficiarios/{id:guid}")]
    public async Task<IActionResult> DeleteBeneficiario(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteBeneficiarioCommand(id), ct);
        return NoContent();
    }

    // ── Distribuições ─────────────────────────────────────────────────────

    [HttpPost("distribuicoes")]
    public async Task<IActionResult> SaveDistribuicao([FromBody] DistribuicaoRequest req, CancellationToken ct)
    {
        if (!MoedaMap.TryGetValue(req.Moeda, out var moeda))
            return BadRequest($"Moeda inválida: {req.Moeda}.");
        var id = await mediator.Send(new SaveDistribuicaoCommand(null, req.Data, req.Valor, moeda,
            req.EstruturaId, req.BeneficiarioId, req.Descricao), ct);
        return Ok(new { id });
    }

    [HttpPut("distribuicoes/{id:guid}")]
    public async Task<IActionResult> UpdateDistribuicao(Guid id, [FromBody] DistribuicaoRequest req, CancellationToken ct)
    {
        if (!MoedaMap.TryGetValue(req.Moeda, out var moeda))
            return BadRequest($"Moeda inválida: {req.Moeda}.");
        await mediator.Send(new SaveDistribuicaoCommand(id, req.Data, req.Valor, moeda,
            req.EstruturaId, req.BeneficiarioId, req.Descricao), ct);
        return NoContent();
    }

    [HttpDelete("distribuicoes/{id:guid}")]
    public async Task<IActionResult> DeleteDistribuicao(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteDistribuicaoCommand(id), ct);
        return NoContent();
    }
}
