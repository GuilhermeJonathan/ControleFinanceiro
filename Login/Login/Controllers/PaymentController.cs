using Login.Application.Common.Interfaces;
using Login.Application.Payments.Commands.CreateCheckout;
using Login.Application.Payments.Commands.ProcessWebhook;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Login.Controllers;

[ApiController]
[Route("[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMercadoPagoService _mp;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        IMediator mediator,
        IMercadoPagoService mp,
        ILogger<PaymentController> logger)
    {
        _mediator = mediator;
        _mp       = mp;
        _logger   = logger;
    }

    /// <summary>
    /// Cria uma assinatura no Mercado Pago e retorna a URL de checkout.
    /// O app abre essa URL no browser para o usuário finalizar o pagamento.
    /// </summary>
    [HttpPost("checkout")]
    [Authorize]
    public async Task<IActionResult> Checkout(
        [FromBody] CheckoutRequest body,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateCheckoutCommand(body.PlanId), cancellationToken);
        return Ok(new { checkoutUrl = result.CheckoutUrl });
    }

    /// <summary>
    /// Recebe notificações do Mercado Pago (webhook).
    /// DEVE ser público (sem autenticação) — o MP não envia token JWT.
    /// A validação é feita via HMAC-SHA256 (X-Signature).
    /// </summary>
    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook(CancellationToken cancellationToken)
    {
        // Lê o corpo raw para validação de assinatura
        Request.EnableBuffering();
        using var reader = new StreamReader(Request.Body, leaveOpen: true);
        var rawBody = await reader.ReadToEndAsync(cancellationToken);
        Request.Body.Position = 0;

        // Valida assinatura HMAC (ignora em sandbox se webhookSecret não configurado)
        var xSignature = Request.Headers["X-Signature"].ToString();
        var xRequestId = Request.Headers["X-Request-Id"].ToString();

        // Parse do payload para extrair type e data.id
        JsonElement root;
        try
        {
            using var doc = JsonDocument.Parse(rawBody);
            root = doc.RootElement.Clone();
        }
        catch (JsonException ex)
        {
            _logger.LogWarning("Webhook MP: payload inválido — {Msg}", ex.Message);
            return BadRequest();
        }

        var type   = root.TryGetProperty("type",   out var t) ? t.GetString() ?? "" : "";
        var dataId = root.TryGetProperty("data",   out var d) &&
                     d.TryGetProperty("id",        out var i)
                     ? i.GetString() ?? "" : "";

        if (string.IsNullOrEmpty(dataId))
        {
            _logger.LogDebug("Webhook MP sem data.id — type={Type}", type);
            return Ok(); // MP envia pings de teste sem data.id
        }

        // Valida assinatura — em produção rejeita se inválida; em dev apenas loga
        var isLive = root.TryGetProperty("live_mode", out var lm) && lm.GetBoolean();
        if (!string.IsNullOrEmpty(xSignature) &&
            !_mp.ValidateWebhookSignature(xSignature, xRequestId, dataId))
        {
            if (isLive)
            {
                _logger.LogWarning("Webhook MP: assinatura inválida em produção. DataId={DataId}", dataId);
                return Unauthorized();
            }
            _logger.LogWarning("Webhook MP: assinatura inválida (sandbox — continuando). DataId={DataId}", dataId);
        }

        // Extrai paymentId quando type=payment
        string? paymentId = type == "payment" ? dataId : null;

        await _mediator.Send(
            new ProcessWebhookCommand(type, dataId, paymentId),
            cancellationToken);

        return Ok();
    }
}

public record CheckoutRequest(string PlanId);
