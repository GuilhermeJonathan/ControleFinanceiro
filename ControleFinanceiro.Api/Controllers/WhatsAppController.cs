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
    ICategoriaRepository categoriaRepo,
    WhatsAppSenderService sender,
    WhatsAppMediaService mediaService,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IConfiguration config) : ControllerBase
{
    // ── Endpoints de teste (remover em produção) ──────────────────────────────

    /// <summary>Testa transcrição de áudio via Whisper. Envie um arquivo de áudio.</summary>
    [HttpPost("test/transcribe")]
    [Authorize]
    public async Task<IActionResult> TestTranscribe(
        IFormFile file, CancellationToken ct)
    {
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms, ct);
        var bytes    = ms.ToArray();
        var mimeType = file.ContentType;

        // Chama diretamente sem passar pela Meta
        var text = await mediaService.TranscribeRawAsync(bytes, mimeType, ct);
        return Ok(new { transcricao = text });
    }

    /// <summary>Testa extração de imagem via GPT Vision. Envie uma imagem.</summary>
    [HttpPost("test/vision")]
    [Authorize]
    public async Task<IActionResult> TestVision(
        IFormFile file, string? caption, CancellationToken ct)
    {
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms, ct);
        var bytes    = ms.ToArray();
        var mimeType = file.ContentType;

        var text   = await mediaService.ExtractRawAsync(bytes, mimeType, caption, ct);
        var parsed = WhatsAppMessageParser.Parse(text);
        return Ok(new { extraido = text, parsed });
    }

    /// <summary>Testa sugestão de categoria por IA (sem restrição às categorias existentes).</summary>
    [HttpPost("test/categoria")]
    [Authorize]
    public async Task<IActionResult> TestCategoria(
        [FromBody] TestCategoriaRequest body, CancellationToken ct)
    {
        var local     = CategoryMatcher.Infer(body.Descricao);
        var ia        = local is null ? await mediaService.SuggestCategoryAsync(body.Descricao, ct) : null;
        var resultado = local ?? ia ?? "Outros";
        return Ok(new { descricao = body.Descricao, fonte = local is not null ? "keyword" : ia is not null ? "ia" : "fallback", categoriaInferida = resultado });
    }

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
            .Where(m => m.Type is "text" or "audio" or "image")
            .ToList();

        foreach (var msg in messages)
            await ProcessMessageAsync(msg, ct);

        // Loga status de entrega para diagnóstico
        var statuses = payload.Entry
            .SelectMany(e => e.Changes)
            .Where(c => c.Field == "messages")
            .SelectMany(c => c.Value.Statuses ?? []);

        foreach (var st in statuses)
        {
            if (st.Errors?.Count > 0)
                Console.WriteLine($"[WhatsApp][ERRO ENTREGA] id={st.Id} status={st.Status} recipient={st.RecipientId} erro={st.Errors[0].Title}");
            else
                Console.WriteLine($"[WhatsApp][STATUS] id={st.Id} status={st.Status} recipient={st.RecipientId}");
        }

        // A Meta exige HTTP 200 independente de erros internos
        return Ok();
    }

    // ── Lista todos os vínculos (admin) ──────────────────────────────────────

    [HttpGet("vinculos/admin")]
    [Authorize]
    public async Task<IActionResult> ListAllVinculos(CancellationToken ct)
    {
        var vinculos = await vinculoRepo.GetAllAsync(ct);
        return Ok(vinculos.Select(v => new
        {
            v.UserId,
            v.PhoneNumber,
            v.CreatedAt,
        }));
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

    private async Task ProcessMessageAsync(WhatsAppMessage msg, CancellationToken ct)
    {
        var from = msg.From;
        Console.WriteLine($"[WhatsApp] from={from} | type={msg.Type}");

        try
        {
            // ── Resolve o texto a processar conforme o tipo ───────────────────
            string text;
            switch (msg.Type)
            {
                case "text":
                    text = msg.Text!.Body;
                    break;

                case "audio":
                    await sender.SendTextAsync(from, "🎙️ Transcrevendo seu áudio...", ct);
                    text = await mediaService.TranscribeAudioAsync(msg.Audio!.Id, ct);
                    Console.WriteLine($"[WhatsApp] áudio transcrito: {text}");
                    break;

                case "image":
                    await sender.SendTextAsync(from, "🖼️ Analisando a imagem...", ct);
                    text = await mediaService.ExtractFromImageAsync(
                        msg.Image!.Id, msg.Image.Caption, ct);
                    Console.WriteLine($"[WhatsApp] imagem extraída: {text}");
                    break;

                default:
                    return; // tipo não suportado — ignora silenciosamente
            }

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

            // ── Inferência e criação automática de categoria ──────────────────
            var categorias = (await categoriaRepo.GetAllAsync(vinculo.UserId, ct)).ToList();
            Console.WriteLine($"[WhatsApp] userId={vinculo.UserId} | categorias carregadas: {categorias.Count} | descricao=\"{parsed.Descricao}\"");

            // 1. Palavras-chave locais (rápido, sem custo de IA)
            var nomeCategoria = CategoryMatcher.Infer(parsed.Descricao);
            Console.WriteLine($"[WhatsApp] keyword match: {nomeCategoria ?? "(nenhum)"}");

            // 2. IA sugere livremente se não encontrou por palavras-chave
            if (nomeCategoria is null)
            {
                nomeCategoria = await mediaService.SuggestCategoryAsync(parsed.Descricao, ct);
                Console.WriteLine($"[WhatsApp] IA sugeriu: {nomeCategoria ?? "(nenhuma)"}");
            }

            // 3. Localiza a categoria sugerida entre as existentes
            Categoria? categoriaMatch = nomeCategoria is not null
                ? categorias.FirstOrDefault(c =>
                    string.Equals(c.Nome, nomeCategoria, StringComparison.OrdinalIgnoreCase))
                : null;

            Console.WriteLine($"[WhatsApp] match nas existentes: {categoriaMatch?.Nome ?? "(não encontrado)"}");

            // 4. Cria automaticamente se a categoria sugerida não existir ainda
            if (categoriaMatch is null && nomeCategoria is not null)
            {
                categoriaMatch = new Categoria(nomeCategoria, parsed.Tipo, vinculo.UserId);
                await categoriaRepo.AddAsync(categoriaMatch, ct);
                await unitOfWork.SaveChangesAsync(ct);
                Console.WriteLine($"[WhatsApp] ✅ Nova categoria criada: \"{nomeCategoria}\" id={categoriaMatch.Id}");
            }

            // 5. Fallback para "Outros" — cria se também não existir
            if (categoriaMatch is null)
            {
                categoriaMatch = categorias.FirstOrDefault(c =>
                    string.Equals(c.Nome, "Outros", StringComparison.OrdinalIgnoreCase));

                if (categoriaMatch is null)
                {
                    categoriaMatch = new Categoria("Outros", parsed.Tipo, vinculo.UserId);
                    await categoriaRepo.AddAsync(categoriaMatch, ct);
                    await unitOfWork.SaveChangesAsync(ct);
                    Console.WriteLine($"[WhatsApp] ✅ Categoria 'Outros' criada id={categoriaMatch.Id}");
                }
                else
                {
                    Console.WriteLine($"[WhatsApp] usando fallback 'Outros' existente id={categoriaMatch.Id}");
                }
            }

            Guid? categoriaId = categoriaMatch?.Id;
            Console.WriteLine($"[WhatsApp] categoriaId final: {categoriaId?.ToString() ?? "NULL ⚠️"}");

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
                CategoriaId:   categoriaId), ct);

            var tipoIcon  = parsed.Tipo == TipoLancamento.Credito ? "📈" : "💸";
            var todayBr   = WhatsAppMessageParser.TodayBrazil();
            var dataLabel = parsed.Data.Date == todayBr.Date               ? "hoje"
                          : parsed.Data.Date == todayBr.AddDays(-1).Date   ? "ontem"
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
public record TestCategoriaRequest(string Descricao);
