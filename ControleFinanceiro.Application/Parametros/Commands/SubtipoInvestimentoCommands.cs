using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Parametros.Commands;

// ── Save Subtipo de Investimento (admin) ─────────────────────────────────────

public record SaveSubtipoInvestimentoCommand(int? Id, int TipoInvestimentoId, string Nome, int Ordem, bool Ativo) : IRequest<int>;

public class SaveSubtipoInvestimentoCommandHandler(
    ISubtipoInvestimentoParamRepository repo,
    ICurrentUser currentUser,
    IUnitOfWork uow)
    : IRequestHandler<SaveSubtipoInvestimentoCommand, int>
{
    public async Task<int> Handle(SaveSubtipoInvestimentoCommand request, CancellationToken ct)
    {
        if (!currentUser.IsAdmin)
            throw new UnauthorizedAccessException("Apenas o admin pode gerenciar subtipos de investimento.");
        if (string.IsNullOrWhiteSpace(request.Nome))
            throw new InvalidOperationException("Informe o nome do subtipo.");

        if (request.Id.HasValue)
        {
            var existing = await repo.GetByIdAsync(request.Id.Value, ct)
                ?? throw new KeyNotFoundException($"Subtipo {request.Id} não encontrado.");
            existing.Atualizar(request.Nome.Trim(), request.Ordem, request.Ativo);
            await uow.SaveChangesAsync(ct);
            return existing.Id;
        }

        var entity = new SubtipoInvestimentoParam(request.TipoInvestimentoId, request.Nome.Trim(), request.Ordem);
        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return entity.Id;
    }
}

// ── Delete Subtipo de Investimento (admin) ────────────────────────────────────

public record DeleteSubtipoInvestimentoCommand(int Id) : IRequest;

public class DeleteSubtipoInvestimentoCommandHandler(
    ISubtipoInvestimentoParamRepository repo,
    ICurrentUser currentUser,
    IUnitOfWork uow)
    : IRequestHandler<DeleteSubtipoInvestimentoCommand>
{
    public async Task Handle(DeleteSubtipoInvestimentoCommand request, CancellationToken ct)
    {
        if (!currentUser.IsAdmin)
            throw new UnauthorizedAccessException("Apenas o admin pode gerenciar subtipos de investimento.");

        var entity = await repo.GetByIdAsync(request.Id, ct)
            ?? throw new KeyNotFoundException($"Subtipo {request.Id} não encontrado.");
        if (entity.IsSystem)
            throw new InvalidOperationException("Subtipos do sistema não podem ser excluídos (desative se não usar).");

        repo.Remove(entity);
        await uow.SaveChangesAsync(ct);
    }
}
