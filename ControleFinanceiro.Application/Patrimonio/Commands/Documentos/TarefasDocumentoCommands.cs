using ControleFinanceiro.Application.Common.Email;
using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ControleFinanceiro.Application.Patrimonio.Commands.Documentos;

// ── Criar tarefa (assessor pede um documento ao cliente) ─────────────────────

public record CriarTarefaDocumentoCommand(
    Guid ClienteId, string Titulo, string? Descricao, string? AtalhoRota) : IRequest<Guid>;

public class CriarTarefaDocumentoCommandHandler(
    ITarefaDocumentoRepository repo,
    IVinculoAssessoriaRepository vinculoRepo,
    ICurrentUser currentUser,
    IUnitOfWork uow,
    IUserNameLookup userLookup,
    IEmailService emailService,
    IConsultoriaConfigRepository consultoriaRepo,
    IConfiguration configuration,
    ILogger<CriarTarefaDocumentoCommandHandler> logger)
    : IRequestHandler<CriarTarefaDocumentoCommand, Guid>
{
    public async Task<Guid> Handle(CriarTarefaDocumentoCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Titulo))
            throw new InvalidOperationException("Informe a ação que o cliente deve realizar.");

        var assessorId = currentUser.RealUserId;
        // O assessor só cria tarefa para um cliente com vínculo ativo.
        var vinculo = await vinculoRepo.GetVinculoAtivoAsync(assessorId, request.ClienteId, ct)
            ?? throw new UnauthorizedAccessException("Vínculo de assessoria não encontrado ou revogado.");

        var tarefa = new TarefaDocumento(assessorId, request.ClienteId, request.Titulo.Trim(),
            request.Descricao?.Trim(), request.AtalhoRota?.Trim());
        await repo.AddAsync(tarefa, ct);
        await uow.SaveChangesAsync(ct);

        // Notifica o cliente por e-mail — falha aqui nunca desfaz a tarefa.
        try { await NotificarClienteAsync(vinculo, tarefa, ct); }
        catch (Exception ex) { logger.LogWarning(ex, "Falha ao notificar cliente {ClienteId} sobre nova tarefa.", request.ClienteId); }

        return tarefa.Id;
    }

    private async Task NotificarClienteAsync(VinculoAssessoria vinculo, TarefaDocumento tarefa, CancellationToken ct)
    {
        var contato = await userLookup.GetContatoAsync(vinculo.ClienteId, ct);
        if (contato?.Email is null) return;

        var nomeCliente = contato.Nome ?? vinculo.NomeCliente ?? "Cliente";
        var consultoria = await consultoriaRepo.GetByUsuarioAsync(vinculo.AssessorId, ct);
        var marca = consultoria?.NomeConsultoria is { Length: > 0 } n ? n : (vinculo.NomeAssessor ?? "Seu assessor");
        var cor = consultoria?.CorMarca is { Length: > 0 } c ? c : "#16a34a";
        var link = $"{ConviteEmailBuilder.BaseUrl(configuration)}/{(string.IsNullOrWhiteSpace(tarefa.AtalhoRota) ? "home" : tarefa.AtalhoRota)}";
        var logo = ConviteEmailBuilder.LogoUrl(configuration, vinculo.AssessorId, !string.IsNullOrWhiteSpace(consultoria?.LogoBase64));

        var body = ConviteEmailBuilder.CorpoTarefa(marca, cor, logo, nomeCliente, tarefa.Titulo, tarefa.Descricao, link);
        await emailService.SendAsync(contato.Email, nomeCliente, $"📋 Nova tarefa de {marca}", body, ct, marca);
    }
}

// ── Concluir (o cliente marca que anexou) ────────────────────────────────────

public record ConcluirTarefaDocumentoCommand(Guid Id) : IRequest;

public class ConcluirTarefaDocumentoCommandHandler(
    ITarefaDocumentoRepository repo, ICurrentUser currentUser, IUnitOfWork uow)
    : IRequestHandler<ConcluirTarefaDocumentoCommand>
{
    public async Task Handle(ConcluirTarefaDocumentoCommand request, CancellationToken ct)
    {
        var tarefa = await repo.GetByIdAsync(request.Id, ct)
            ?? throw new KeyNotFoundException("Tarefa não encontrada.");
        if (tarefa.ClienteId != currentUser.RealUserId)
            throw new UnauthorizedAccessException("Acesso negado à tarefa.");

        tarefa.Concluir();
        await uow.SaveChangesAsync(ct);
    }
}

// ── Excluir (assessor remove a tarefa) ───────────────────────────────────────

public record DeleteTarefaDocumentoCommand(Guid Id) : IRequest;

public class DeleteTarefaDocumentoCommandHandler(
    ITarefaDocumentoRepository repo, ICurrentUser currentUser, IUnitOfWork uow)
    : IRequestHandler<DeleteTarefaDocumentoCommand>
{
    public async Task Handle(DeleteTarefaDocumentoCommand request, CancellationToken ct)
    {
        var tarefa = await repo.GetByIdAsync(request.Id, ct)
            ?? throw new KeyNotFoundException("Tarefa não encontrada.");
        if (tarefa.AssessorId != currentUser.RealUserId)
            throw new UnauthorizedAccessException("Acesso negado à tarefa.");

        repo.Remove(tarefa);
        await uow.SaveChangesAsync(ct);
    }
}
