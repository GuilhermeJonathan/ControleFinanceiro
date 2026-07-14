using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Parametros.Commands;

// ── Save TipoAtivo ────────────────────────────────────────────────────────

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
            throw new UnauthorizedAccessException("Apenas assessores podem gerenciar parâmetros.");

        if (request.Id.HasValue)
        {
            var existing = await repo.GetByIdAsync(request.Id.Value, ct)
                ?? throw new KeyNotFoundException($"TipoAtivo {request.Id} não encontrado.");
            existing.Atualizar(request.Nome, request.Ordem, request.Ativo, request.Icone);
            await uow.SaveChangesAsync(ct);
            return existing.Id;
        }

        var entity = new TipoAtivoParam(request.Nome, request.Ordem, request.Icone);
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
            throw new UnauthorizedAccessException("Apenas assessores podem gerenciar parâmetros.");

        var entity = await repo.GetByIdAsync(request.Id, ct)
            ?? throw new KeyNotFoundException($"TipoAtivo {request.Id} não encontrado.");

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
            throw new UnauthorizedAccessException("Apenas assessores podem gerenciar parâmetros.");

        if (request.Id.HasValue)
        {
            var existing = await repo.GetByIdAsync(request.Id.Value, ct)
                ?? throw new KeyNotFoundException($"TipoInvestimento {request.Id} não encontrado.");
            existing.Atualizar(request.Nome, request.Ordem, request.Ativo, request.Icone);
            await uow.SaveChangesAsync(ct);
            return existing.Id;
        }

        var entity = new TipoInvestimentoParam(request.Nome, request.Ordem, request.Icone);
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
            throw new UnauthorizedAccessException("Apenas assessores podem gerenciar parâmetros.");

        var entity = await repo.GetByIdAsync(request.Id, ct)
            ?? throw new KeyNotFoundException($"TipoInvestimento {request.Id} não encontrado.");

        if (entity.IsSystem)
            throw new InvalidOperationException("Itens do sistema não podem ser excluídos.");

        repo.Remove(entity);
        await uow.SaveChangesAsync(ct);
    }
}

// ── Save Moeda ────────────────────────────────────────────────────────────

public record SaveMoedaCommand(int? Id, string Codigo, string Nome, int Ordem, bool Ativo) : IRequest<int>;

public class SaveMoedaCommandHandler(
    IMoedaParamRepository repo,
    ICurrentUser currentUser,
    IUnitOfWork uow)
    : IRequestHandler<SaveMoedaCommand, int>
{
    public async Task<int> Handle(SaveMoedaCommand request, CancellationToken ct)
    {
        if (!currentUser.IsAssessor)
            throw new UnauthorizedAccessException("Apenas assessores podem gerenciar parâmetros.");

        if (request.Id.HasValue)
        {
            var existing = await repo.GetByIdAsync(request.Id.Value, ct)
                ?? throw new KeyNotFoundException($"Moeda {request.Id} não encontrada.");
            existing.Atualizar(request.Codigo, request.Nome, request.Ordem, request.Ativo);
            await uow.SaveChangesAsync(ct);
            return existing.Id;
        }

        var entity = new MoedaParam(request.Codigo, request.Nome, request.Ordem);
        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return entity.Id;
    }
}

// ── Delete Moeda ──────────────────────────────────────────────────────────

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
            throw new UnauthorizedAccessException("Apenas assessores podem gerenciar parâmetros.");

        var entity = await repo.GetByIdAsync(request.Id, ct)
            ?? throw new KeyNotFoundException($"Moeda {request.Id} não encontrada.");

        if (entity.IsSystem)
            throw new InvalidOperationException("Itens do sistema não podem ser excluídos.");

        repo.Remove(entity);
        await uow.SaveChangesAsync(ct);
    }
}
