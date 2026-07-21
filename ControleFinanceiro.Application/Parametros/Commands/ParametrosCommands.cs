using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Parametros.Commands;

// ── Save TipoAtivo ────────────────────────────────────────────────────────
// Admin edita o catálogo global (AssessorId=null); assessor edita apenas os seus custom.

public record SaveTipoAtivoCommand(int? Id, string Nome, string? Icone, int Ordem, bool Ativo) : IRequest<int>;

public class SaveTipoAtivoCommandHandler(
    ITipoAtivoParamRepository repo,
    ICurrentUser currentUser,
    IUnitOfWork uow)
    : IRequestHandler<SaveTipoAtivoCommand, int>
{
    public async Task<int> Handle(SaveTipoAtivoCommand request, CancellationToken ct)
    {
        if (!currentUser.IsAssessor)
            throw new UnauthorizedAccessException("Apenas admin ou assessores podem gerenciar parâmetros.");

        Guid? escopo = currentUser.IsAdmin ? null : currentUser.RealUserId; // null = global

        if (request.Id.HasValue)
        {
            var existing = await repo.GetByIdAsync(request.Id.Value, ct)
                ?? throw new KeyNotFoundException($"TipoAtivo {request.Id} não encontrado.");
            if (existing.AssessorId != escopo)
                throw new UnauthorizedAccessException("Você só pode editar os seus próprios tipos.");
            existing.Atualizar(request.Nome, request.Ordem, request.Ativo, request.Icone);
            await uow.SaveChangesAsync(ct);
            return existing.Id;
        }

        var entity = new TipoAtivoParam(request.Nome, request.Ordem, request.Icone, escopo);
        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return entity.Id;
    }
}

// ── Delete TipoAtivo ──────────────────────────────────────────────────────

public record DeleteTipoAtivoCommand(int Id) : IRequest;

public class DeleteTipoAtivoCommandHandler(
    ITipoAtivoParamRepository repo,
    ICurrentUser currentUser,
    IUnitOfWork uow)
    : IRequestHandler<DeleteTipoAtivoCommand>
{
    public async Task Handle(DeleteTipoAtivoCommand request, CancellationToken ct)
    {
        if (!currentUser.IsAssessor)
            throw new UnauthorizedAccessException("Apenas admin ou assessores podem gerenciar parâmetros.");

        var entity = await repo.GetByIdAsync(request.Id, ct)
            ?? throw new KeyNotFoundException($"TipoAtivo {request.Id} não encontrado.");

        Guid? escopo = currentUser.IsAdmin ? null : currentUser.RealUserId;
        if (entity.AssessorId != escopo)
            throw new UnauthorizedAccessException("Você só pode excluir os seus próprios tipos.");

        if (entity.IsSystem)
            throw new InvalidOperationException("Itens do sistema não podem ser excluídos.");

        repo.Remove(entity);
        await uow.SaveChangesAsync(ct);
    }
}

// ── Save TipoInvestimento ─────────────────────────────────────────────────

public record SaveTipoInvestimentoCommand(int? Id, string Nome, string? Icone, int Ordem, bool Ativo) : IRequest<int>;

public class SaveTipoInvestimentoCommandHandler(
    ITipoInvestimentoParamRepository repo,
    ICurrentUser currentUser,
    IUnitOfWork uow)
    : IRequestHandler<SaveTipoInvestimentoCommand, int>
{
    public async Task<int> Handle(SaveTipoInvestimentoCommand request, CancellationToken ct)
    {
        if (!currentUser.IsAssessor)
            throw new UnauthorizedAccessException("Apenas admin ou assessores podem gerenciar parâmetros.");

        Guid? escopo = currentUser.IsAdmin ? null : currentUser.RealUserId;

        if (request.Id.HasValue)
        {
            var existing = await repo.GetByIdAsync(request.Id.Value, ct)
                ?? throw new KeyNotFoundException($"TipoInvestimento {request.Id} não encontrado.");
            if (existing.AssessorId != escopo)
                throw new UnauthorizedAccessException("Você só pode editar os seus próprios tipos.");
            existing.Atualizar(request.Nome, request.Ordem, request.Ativo, request.Icone);
            await uow.SaveChangesAsync(ct);
            return existing.Id;
        }

        var entity = new TipoInvestimentoParam(request.Nome, request.Ordem, request.Icone, escopo);
        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return entity.Id;
    }
}

// ── Delete TipoInvestimento ───────────────────────────────────────────────

public record DeleteTipoInvestimentoCommand(int Id) : IRequest;

public class DeleteTipoInvestimentoCommandHandler(
    ITipoInvestimentoParamRepository repo,
    ICurrentUser currentUser,
    IUnitOfWork uow)
    : IRequestHandler<DeleteTipoInvestimentoCommand>
{
    public async Task Handle(DeleteTipoInvestimentoCommand request, CancellationToken ct)
    {
        if (!currentUser.IsAssessor)
            throw new UnauthorizedAccessException("Apenas admin ou assessores podem gerenciar parâmetros.");

        var entity = await repo.GetByIdAsync(request.Id, ct)
            ?? throw new KeyNotFoundException($"TipoInvestimento {request.Id} não encontrado.");

        Guid? escopo = currentUser.IsAdmin ? null : currentUser.RealUserId;
        if (entity.AssessorId != escopo)
            throw new UnauthorizedAccessException("Você só pode excluir os seus próprios tipos.");

        if (entity.IsSystem)
            throw new InvalidOperationException("Itens do sistema não podem ser excluídos.");

        repo.Remove(entity);
        await uow.SaveChangesAsync(ct);
    }
}

// ── Ocultar / Reexibir default global (por assessoria) ─────────────────────
// O assessor esconde do seu catálogo um tipo global que não usa (sem afetar os demais).

public record OcultarParametroCommand(TipoParametroCatalogo Tipo, int ParametroId) : IRequest;

public class OcultarParametroCommandHandler(
    IParametroOcultoRepository ocultoRepo,
    ITipoAtivoParamRepository tipoAtivoRepo,
    ITipoInvestimentoParamRepository tipoInvestimentoRepo,
    IMoedaParamRepository moedaRepo,
    ICurrentUser currentUser,
    IUnitOfWork uow)
    : IRequestHandler<OcultarParametroCommand>
{
    public async Task Handle(OcultarParametroCommand request, CancellationToken ct)
    {
        if (!currentUser.IsAssessor || currentUser.IsAdmin)
            throw new UnauthorizedAccessException("Apenas assessores podem ocultar itens do próprio catálogo.");

        var assessorId = currentUser.RealUserId;

        // Só é possível ocultar um default GLOBAL existente.
        var ehGlobal = request.Tipo switch
        {
            TipoParametroCatalogo.TipoAtivo        => (await tipoAtivoRepo.GetByIdAsync(request.ParametroId, ct))?.AssessorId is null,
            TipoParametroCatalogo.TipoInvestimento => (await tipoInvestimentoRepo.GetByIdAsync(request.ParametroId, ct))?.AssessorId is null,
            TipoParametroCatalogo.Moeda            => (await moedaRepo.GetByIdAsync(request.ParametroId, ct))?.AssessorId is null,
            _ => false,
        };
        if (!ehGlobal)
            throw new InvalidOperationException("Só é possível ocultar itens globais (padrão).");

        // BRL é a base de conversão de todo o patrimônio — nunca pode sumir do catálogo.
        if (request.Tipo == TipoParametroCatalogo.Moeda)
        {
            var moeda = await moedaRepo.GetByIdAsync(request.ParametroId, ct);
            if (string.Equals(moeda?.Codigo, "BRL", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("A moeda BRL não pode ser ocultada.");
        }

        var jaOculto = await ocultoRepo.GetAsync(assessorId, request.Tipo, request.ParametroId, ct);
        if (jaOculto is not null) return; // idempotente

        await ocultoRepo.AddAsync(new ParametroOculto(assessorId, request.Tipo, request.ParametroId), ct);
        await uow.SaveChangesAsync(ct);
    }
}

public record ReexibirParametroCommand(TipoParametroCatalogo Tipo, int ParametroId) : IRequest;

public class ReexibirParametroCommandHandler(
    IParametroOcultoRepository ocultoRepo,
    ICurrentUser currentUser,
    IUnitOfWork uow)
    : IRequestHandler<ReexibirParametroCommand>
{
    public async Task Handle(ReexibirParametroCommand request, CancellationToken ct)
    {
        if (!currentUser.IsAssessor || currentUser.IsAdmin)
            throw new UnauthorizedAccessException("Apenas assessores podem gerenciar o próprio catálogo.");

        var existente = await ocultoRepo.GetAsync(currentUser.RealUserId, request.Tipo, request.ParametroId, ct);
        if (existente is null) return; // idempotente

        ocultoRepo.Remove(existente);
        await uow.SaveChangesAsync(ct);
    }
}

// ── Save Moeda (admin → global; assessor → sua custom) ──────────────────────

public record SaveMoedaCommand(int? Id, string Codigo, string Nome, decimal CotacaoBRL, int Ordem, bool Ativo) : IRequest<int>;

public class SaveMoedaCommandHandler(
    IMoedaParamRepository repo,
    ICurrentUser currentUser,
    IUnitOfWork uow)
    : IRequestHandler<SaveMoedaCommand, int>
{
    public async Task<int> Handle(SaveMoedaCommand request, CancellationToken ct)
    {
        if (!currentUser.IsAssessor)
            throw new UnauthorizedAccessException("Apenas admin ou assessores podem gerenciar moedas.");

        Guid? escopo = currentUser.IsAdmin ? null : currentUser.RealUserId; // null = global

        if (request.Id.HasValue)
        {
            var existing = await repo.GetByIdAsync(request.Id.Value, ct)
                ?? throw new KeyNotFoundException($"Moeda {request.Id} não encontrada.");
            if (existing.AssessorId != escopo)
                throw new UnauthorizedAccessException("Você só pode editar as suas próprias moedas.");
            existing.Atualizar(request.Codigo, request.Nome, request.Ordem, request.Ativo, request.CotacaoBRL);
            await uow.SaveChangesAsync(ct);
            return existing.Id;
        }

        var entity = new MoedaParam(request.Codigo, request.Nome, request.Ordem, request.CotacaoBRL, escopo);
        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return entity.Id;
    }
}

// ── Delete Moeda (admin → global; assessor → sua custom) ────────────────────

public record DeleteMoedaCommand(int Id) : IRequest;

public class DeleteMoedaCommandHandler(
    IMoedaParamRepository repo,
    ICurrentUser currentUser,
    IUnitOfWork uow)
    : IRequestHandler<DeleteMoedaCommand>
{
    public async Task Handle(DeleteMoedaCommand request, CancellationToken ct)
    {
        if (!currentUser.IsAssessor)
            throw new UnauthorizedAccessException("Apenas admin ou assessores podem gerenciar moedas.");

        var entity = await repo.GetByIdAsync(request.Id, ct)
            ?? throw new KeyNotFoundException($"Moeda {request.Id} não encontrada.");

        Guid? escopo = currentUser.IsAdmin ? null : currentUser.RealUserId;
        if (entity.AssessorId != escopo)
            throw new UnauthorizedAccessException("Você só pode excluir as suas próprias moedas.");

        if (entity.IsSystem)
            throw new InvalidOperationException("Itens do sistema não podem ser excluídos.");

        repo.Remove(entity);
        await uow.SaveChangesAsync(ct);
    }
}
