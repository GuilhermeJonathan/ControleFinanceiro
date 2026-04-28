using ControleFinanceiro.Api.WhatsApp;
using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Application.Lancamentos.Commands.CreateLancamento;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.Api.Controllers;

[ApiController]
[Route("api/whatsapp")]
public class WhatsAppController(
    IMediator mediator,
    IWhatsAppVinculoRepository vinculoRepo,
    WhatsAppSenderService sender,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IConfiguration config) : ControllerBase
{
    // ── Verificação do webhook (Meta chama ao configurar) ─────────────────────

    [HttpGet("webhook")]
    [AllowAnonymous]
    public IActionResult Verify(
        [FromQuery(Name = "hub.mode")]         string? mode,
        [FromQuery(Name = "hub.challenge")]    string? challenge,
        [FromQuery(Name = "hub.verify_token")] string? verifyToken)
    {
        var expected = config["WhatsApp:VerifyToken"];

        if (mode == "subscribe" && verifyToken == expected && challenge is not null)
            return Ok(int.Parse(challenge));

        return Forbid();
    }

    // ── Recebimento de mensagens ──────────────────────────────────────────────

    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Receive(
        [FromBody] WhatsAppWebhookPayload payload,
        CancellationToken ct)
    {
        var messages = payload.Entry
            .SelectMany(e => e.Changes)
            .Where(c => c.Field == "messages")
            .SelectMany(c => c.Value.Messages ?? [])
            .Where(m => m.Type == "text" && m.Text is not null)
            .ToList();

        foreach (var msg in messages)
            await ProcessMessageAsync(msg.From, msg.Text!.Body, ct);

        // A Meta exige HTTP 200 independente de erros internos
        return Ok();
    }

    // ── Vincular número ao usuário autenticado ────────────────────────────────

    [HttpPost("vincular")]
    [Authorize]
    public async Task<IActionResult> Vincular(
        [FromBody] VincularRequest body,
        CancellationToken ct)
    {
        var userId = currentUser.UserId;

        // Remove vínculo anterior do usuário, se houver
        var existing = await vinculoRepo.GetByUserIdAsync(userId, ct);
        if (existing is not null) vinculoRepo.Remove(existing);

        // Verifica se o número já está em uso por outro usuário
        var byPhone = await vinculoRepo.GetByPhoneAsync(body.PhoneNumber, ct);
        if (byPhone is not null && byPhone.UserId != userId)
            return Conflict("Este número já está vinculado a outra conta.");

        if (byPhone is not null) vinculoRepo.Remove(byPhone);

        await vinculoRepo.AddAsync(new WhatsAppVinculo(userId, body.PhoneNumber), ct);
        await unitOfWork.SaveChangesAsync(ct);

        return NoContent();
    }

    // ── Consultar vínculo do usuário autenticado ──────────────────────────────

    [HttpGet("vinculo")]
    [Authorize]
    public async Task<IActionResult> GetVinculo(CancellationToken ct)
    {
        var vinculo = await vinculoRepo.GetByUserIdAsync(currentUser.UserId, ct);
        if (vinculo is null) return NotFound();
        return Ok(new { vinculo.PhoneNumber, vinculo.CreatedAt });
    }

    // ── Desvincular ───────────────────────────────────────────────────────────

    [HttpDelete("vinculo")]
    [Authorize]
    public async Task<IActionResult> Desvincular(CancellationToken ct)
    {
        var vinculo = await vinculoRepo.GetByUserIdAsync(currentUser.UserId, ct);
        if (vinculo is null) return NotFound();

        vinculoRepo.Remove(vinculo);
        await unitOfWork.SaveChangesAsync(ct);
        return NoContent();
    }

    // ── Lógica interna ────────────────────────────────────────────────────────

    private async Task ProcessMessageAsync(string from, string text, CancellationToken ct)
    {
        Console.WriteLine($"[WhatsApp] from={from} | text={text}");

        try
        {
            // Comandos especiais (ajuda, etc.)
            if (WhatsAppMessageParser.IsCommand(text, out var reply))
            {
                await sender.SendTextAsync(from, reply, ct);
                return;
            }

            // Verifica se o número está vinculado
            var vinculo = await vinculoRepo.GetByPhoneAsync(from, ct);
            if (vinculo is null)
            {
                await sender.SendTextAsync(from,
                    "⚠️ Seu número não está vinculado.\nAbra o app *Meu Financeiro*, vá em *Perfil → Vincular WhatsApp* e adicione este número.", ct);
                return;
            }

            // Usa o número cadastrado no vínculo (não o from do webhook, que pode ter formato diferente)
            var replyTo = vinculo.PhoneNumber;

            // Faz o parse da mensagem
            var parsed = WhatsAppMessageParser.Parse(text);
            if (!parsed.Success)
            {
                await sender.SendTextAsync(from, parsed.Erro ?? "Não entendi a mensagem.", ct);
                return;
            }

            // Injeta o userId via HttpContext.Items para o ICurrentUser funcionar sem JWT
            HttpContext.Items["EffectiveUserId"] = vinculo.UserId;
            HttpContext.Items["RealUserId"]      = vinculo.UserId;

            var situacao = parsed.Tipo == TipoLancamento.Credito
                ? SituacaoLancamento.Recebido
                : SituacaoLancamento.Pago;

            await mediator.Send(new CreateLancamentoCommand(
                Descricao:     parsed.Descricao,
                Data:          parsed.Data,
                Valor:         parsed.Valor,
                Tipo:          parsed.Tipo,
                Situacao:      situacao,
                Mes:           parsed.Data.Month,
                Ano:           parsed.Data.Year,
                CategoriaId:   null), ct);

            var tipoIcon  = parsed.Tipo == TipoLancamento.Credito ? "📈" : "💸";
            var dataLabel = parsed.Data.Date == DateTime.Today          ? "hoje"
                          : parsed.Data.Date == DateTime.Today.AddDays(-1) ? "ontem"
                          : parsed.Data.ToString("dd/MM");

            await sender.SendTextAsync(replyTo,
                $"{tipoIcon} *{parsed.Descricao}* registrado!\n" +
                $"Valor: R$ {parsed.Valor:N2}\n" +
                $"Data: {dataLabel}", ct);
        }
        catch (Exception ex)
        {
            // Nunca deixa o webhook lançar — a Meta iria reenviar indefinidamente
            await sender.SendTextAsync(from,
                $"❌ Erro ao registrar. Tente novamente.\n({ex.Message})", ct);
        }
    }
}

public record VincularRequest(string PhoneNumber);
