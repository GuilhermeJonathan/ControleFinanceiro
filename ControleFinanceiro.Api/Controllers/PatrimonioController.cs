using ControleFinanceiro.Application.Patrimonio.Commands.CreateAtivo;
using ControleFinanceiro.Application.Patrimonio.Commands.CreatePassivo;
using ControleFinanceiro.Application.Patrimonio.Commands.DeleteAtivo;
using ControleFinanceiro.Application.Patrimonio.Commands.DeletePassivo;
using ControleFinanceiro.Application.Patrimonio.Commands.UpdateAtivo;
using ControleFinanceiro.Application.Patrimonio.Commands.UpdatePassivo;
using ControleFinanceiro.Application.Patrimonio.Queries.GetProjecaoDividas;
using ControleFinanceiro.Application.Patrimonio.Queries.GetProjecaoPatrimonio;
using ControleFinanceiro.Application.Patrimonio.Queries.GetPlanoAcao;
using ControleFinanceiro.Application.Patrimonio.Commands.SavePlanoAcao;
using ControleFinanceiro.Application.Patrimonio.Commands.DeletePlanoAcao;
using ControleFinanceiro.Application.Patrimonio.Queries.GetDicasPatrimonio;
using ControleFinanceiro.Application.Patrimonio.Commands.ImportarInvestimentos;
using ControleFinanceiro.Application.Patrimonio.Commands.SaveAlocacaoAlvo;
using ControleFinanceiro.Application.Patrimonio.Queries.GetEvolucaoPatrimonial;
using ControleFinanceiro.Application.Patrimonio.Queries.GetInsightsPatrimonio;
using ControleFinanceiro.Application.Patrimonio.Queries.GetRebalanceamento;
using ControleFinanceiro.Application.Patrimonio.Queries.GetResumoPatrimonial;
using ControleFinanceiro.Application.Relatorios;
using ControleFinanceiro.Application.Relatorios.Queries.GerarRelatorio;
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
    decimal? ValorizacaoAnualPct,
    decimal ReceitaMensal = 0m,
    decimal DespesaMensal = 0m,
    Guid? EstruturaId = null);

/// <summary>Request body para criação/edição de dívida. Moeda como string; Prazo 1=Curto, 2=Longo.</summary>
public record PassivoPatrimonialRequest(
    string Nome,
    string Moeda,
    decimal Valor,
    PrazoDivida Prazo,
    decimal? TaxaJurosAnualPct,
    int? PrazoMeses);

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

    /// <summary>Evolução mensal do patrimônio do usuário efetivo (gráfico).</summary>
    [HttpGet("evolucao")]
    public async Task<IActionResult> GetEvolucao([FromQuery] int meses = 12, CancellationToken cancellationToken = default) =>
        Ok(await mediator.Send(new GetEvolucaoPatrimonialQuery(meses), cancellationToken));

    /// <summary>Insights determinísticos e acionáveis sobre o patrimônio do usuário efetivo.</summary>
    [HttpGet("insights")]
    public async Task<IActionResult> GetInsights(CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetInsightsPatrimonioQuery(), cancellationToken));

    /// <summary>Alocação atual dos investimentos vs. alocação-alvo (rebalanceamento).</summary>
    [HttpGet("rebalanceamento")]
    public async Task<IActionResult> GetRebalanceamento(CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetRebalanceamentoQuery(), cancellationToken));

    /// <summary>Importa investimentos a partir de um CSV (cabeçalho + linhas).</summary>
    [HttpPost("investimentos/importar")]
    public async Task<IActionResult> ImportarInvestimentos([FromBody] ImportarInvestimentosCommand command, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(command, cancellationToken));

    /// <summary>Define a alocação-alvo (% por classe) do usuário efetivo.</summary>
    [HttpPut("alocacao-alvo")]
    public async Task<IActionResult> SalvarAlocacaoAlvo([FromBody] IEnumerable<AlvoItem> alvos, CancellationToken cancellationToken)
    {
        await mediator.Send(new SaveAlocacaoAlvoCommand(alvos), cancellationToken);
        return NoContent();
    }

    /// <summary>Dicas e análise do patrimônio geradas por IA.</summary>
    [HttpGet("dicas")]
    public async Task<IActionResult> GetDicas(CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetDicasPatrimonioQuery(), cancellationToken));

    /// <summary>Cadastra um novo ativo patrimonial.</summary>
    [HttpPost("ativos")]
    public async Task<IActionResult> CreateAtivo([FromBody] AtivoPatrimonialRequest request, CancellationToken cancellationToken)
    {
        if (!MoedaMap.TryGetValue(request.Moeda, out var moeda))
            return BadRequest($"Moeda inválida: {request.Moeda}.");

        var id = await mediator.Send(
            new CreateAtivoPatrimonialCommand(request.Nome, request.Tipo, moeda, request.ValorAtual,
                request.ValorizacaoAnualPct, request.ReceitaMensal, request.DespesaMensal, request.EstruturaId),
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
            new UpdateAtivoPatrimonialCommand(id, request.Nome, request.Tipo, moeda, request.ValorAtual,
                request.ValorizacaoAnualPct, request.ReceitaMensal, request.DespesaMensal, request.EstruturaId),
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

    // ── Dívidas / Passivos ────────────────────────────────────────────────────

    /// <summary>Cadastra uma nova dívida/passivo.</summary>
    [HttpPost("passivos")]
    public async Task<IActionResult> CreatePassivo([FromBody] PassivoPatrimonialRequest request, CancellationToken cancellationToken)
    {
        if (!MoedaMap.TryGetValue(request.Moeda, out var moeda))
            return BadRequest($"Moeda inválida: {request.Moeda}.");

        var id = await mediator.Send(
            new CreatePassivoPatrimonialCommand(request.Nome, moeda, request.Valor, request.Prazo,
                request.TaxaJurosAnualPct, request.PrazoMeses),
            cancellationToken);

        return CreatedAtAction(nameof(GetResumo), new { }, new { id });
    }

    /// <summary>Atualiza uma dívida/passivo existente.</summary>
    [HttpPut("passivos/{id:guid}")]
    public async Task<IActionResult> UpdatePassivo(Guid id, [FromBody] PassivoPatrimonialRequest request, CancellationToken cancellationToken)
    {
        if (!MoedaMap.TryGetValue(request.Moeda, out var moeda))
            return BadRequest($"Moeda inválida: {request.Moeda}.");

        await mediator.Send(
            new UpdatePassivoPatrimonialCommand(id, request.Nome, moeda, request.Valor, request.Prazo,
                request.TaxaJurosAnualPct, request.PrazoMeses),
            cancellationToken);

        return NoContent();
    }

    /// <summary>Remove uma dívida/passivo.</summary>
    [HttpDelete("passivos/{id:guid}")]
    public async Task<IActionResult> DeletePassivo(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeletePassivoPatrimonialCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>Projeção de quitação das dívidas (amortização mês a mês).</summary>
    [HttpGet("projecao-dividas")]
    public async Task<IActionResult> GetProjecaoDividas([FromQuery] int? meses, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetProjecaoDividasQuery(meses), cancellationToken));

    /// <summary>Projeção do patrimônio líquido (bens valorizando × dívidas amortizando) no mesmo horizonte.</summary>
    [HttpGet("projecao-patrimonio")]
    public async Task<IActionResult> GetProjecaoPatrimonio([FromQuery] int? meses, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetProjecaoPatrimonioQuery(meses), cancellationToken));

    /// <summary>Planos de Ação (jornadas de etapas) do cliente efetivo — um cliente pode ter vários.</summary>
    [HttpGet("plano-acao")]
    public async Task<IActionResult> GetPlanosAcao(CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetPlanosAcaoQuery(), cancellationToken));

    /// <summary>Cria um novo plano de ação do cliente efetivo (assessor no view-as). Retorna o id.</summary>
    [HttpPost("plano-acao")]
    public async Task<IActionResult> CriarPlanoAcao([FromBody] SavePlanoAcaoCommand command, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(command with { Id = null }, cancellationToken);
        return Ok(new { id });
    }

    /// <summary>Atualiza um plano de ação existente do cliente efetivo.</summary>
    [HttpPut("plano-acao/{id:guid}")]
    public async Task<IActionResult> AtualizarPlanoAcao(Guid id, [FromBody] SavePlanoAcaoCommand command, CancellationToken cancellationToken)
    {
        var salvo = await mediator.Send(command with { Id = id }, cancellationToken);
        return Ok(new { id = salvo });
    }

    /// <summary>Exclui um plano de ação do cliente efetivo.</summary>
    [HttpDelete("plano-acao/{id:guid}")]
    public async Task<IActionResult> ExcluirPlanoAcao(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeletePlanoAcaoCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>Gera o relatório patrimonial em PDF (marca do assessor + dados do cliente efetivo).</summary>
    [HttpPost("relatorio")]
    public async Task<IActionResult> GerarRelatorio([FromBody] RelatorioRequest req, CancellationToken cancellationToken)
    {
        var pdf = await mediator.Send(
            new GerarRelatorioPatrimonialQuery(
                req.ClienteNome,
                new RelatorioBranding(req.NomeConsultoria, req.LogoBase64, req.CorMarca)),
            cancellationToken);

        return File(pdf, "application/pdf", "relatorio-patrimonial.pdf");
    }

    /// <summary>Gera o relatório COMPLETO (patrimonial + sucessão) num único PDF.</summary>
    [HttpPost("relatorio-completo")]
    public async Task<IActionResult> GerarRelatorioCompleto([FromBody] RelatorioRequest req, CancellationToken cancellationToken)
    {
        var pdf = await mediator.Send(
            new GerarRelatorioCompletoQuery(
                req.ClienteNome,
                new RelatorioBranding(req.NomeConsultoria, req.LogoBase64, req.CorMarca)),
            cancellationToken);

        return File(pdf, "application/pdf", "relatorio-completo.pdf");
    }
}

/// <summary>Request do relatório: dados do cliente vêm do servidor; a marca vem do app.</summary>
public record RelatorioRequest(string? ClienteNome, string? NomeConsultoria, string? LogoBase64, string? CorMarca);
