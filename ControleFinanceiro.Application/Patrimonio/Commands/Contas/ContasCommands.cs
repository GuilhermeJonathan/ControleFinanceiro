using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Patrimonio.Commands.Contas;

// ── Save Conta (create/update) ───────────────────────────────────────────────

public record SaveContaCommand(
    Guid? Id,
    string Nome,
    TipoContaFinanceira Tipo,
    MoedaPatrimonio Moeda,
    decimal Saldo,
    string? Instituicao,
    string? Pais,
    string? Identificador,
    Guid? EstruturaId) : IRequest<Guid>;

public class SaveContaCommandHandler(
    IContaFinanceiraRepository repo,
    IEstruturaRepository estruturaRepo,
    ICurrentUser currentUser,
    IUnitOfWork uow)
    : IRequestHandler<SaveContaCommand, Guid>
{
    public async Task<Guid> Handle(SaveContaCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
            throw new InvalidOperationException("Informe o nome da conta.");

        // Se ligada a uma estrutura, ela precisa ser do próprio usuário.
        if (request.EstruturaId.HasValue)
        {
            var est = await estruturaRepo.GetByIdAsync(request.EstruturaId.Value, ct)
                ?? throw new KeyNotFoundException("Estrutura não encontrada.");
            if (est.UsuarioId != currentUser.UserId)
                throw new UnauthorizedAccessException("Acesso negado à estrutura.");
        }

        if (request.Id.HasValue)
        {
            var existing = await repo.GetByIdAsync(request.Id.Value, ct)
                ?? throw new KeyNotFoundException($"Conta {request.Id} não encontrada.");
            if (existing.UsuarioId != currentUser.UserId)
                throw new UnauthorizedAccessException("Acesso negado à conta.");

            existing.Atualizar(request.Nome.Trim(), request.Tipo, request.Moeda, request.Saldo,
                request.Instituicao, request.Pais, request.Identificador, request.EstruturaId);
            await uow.SaveChangesAsync(ct);
            return existing.Id;
        }

        var entity = new ContaFinanceira(currentUser.UserId, request.Nome.Trim(), request.Tipo,
            request.Moeda, request.Saldo, request.Instituicao, request.Pais, request.Identificador, request.EstruturaId);
        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return entity.Id;
    }
}

// ── Delete Conta ──────────────────────────────────────────────────────────────
// Ao excluir, os investimentos vinculados voltam a ficar soltos (ContaId = null).

public record DeleteContaCommand(Guid Id) : IRequest;

public class DeleteContaCommandHandler(
    IContaFinanceiraRepository repo,
    IInvestimentoRepository investimentoRepo,
    ICurrentUser currentUser,
    IUnitOfWork uow)
    : IRequestHandler<DeleteContaCommand>
{
    public async Task Handle(DeleteContaCommand request, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(request.Id, ct)
            ?? throw new KeyNotFoundException($"Conta {request.Id} não encontrada.");
        if (entity.UsuarioId != currentUser.UserId)
            throw new UnauthorizedAccessException("Acesso negado à conta.");

        foreach (var i in (await investimentoRepo.GetByUsuarioAsync(currentUser.UserId, ct)).Where(i => i.ContaId == entity.Id))
        {
            i.DesvincularConta();
            investimentoRepo.Update(i);
        }

        repo.Remove(entity);
        await uow.SaveChangesAsync(ct);
    }
}
