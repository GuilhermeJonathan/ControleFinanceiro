using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Patrimonio.Commands.Estruturas;

// ── Beneficiário (do cliente) ───────────────────────────────────────────────

public record SaveBeneficiarioCommand(
    Guid? Id,
    string Nome,
    PapelBeneficiario Papel,
    decimal PercentualDistribuicao,
    string? CondicaoLiberacao) : IRequest<Guid>;

public class SaveBeneficiarioCommandHandler(
    IEstruturaRepository repo,
    ICurrentUser currentUser,
    IUnitOfWork uow)
    : IRequestHandler<SaveBeneficiarioCommand, Guid>
{
    public async Task<Guid> Handle(SaveBeneficiarioCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
            throw new InvalidOperationException("Informe o nome do beneficiário.");
        if (request.PercentualDistribuicao is < 0 or > 100)
            throw new InvalidOperationException("Percentual deve estar entre 0 e 100.");

        if (request.Id.HasValue)
        {
            var existing = await repo.GetBeneficiarioByIdAsync(request.Id.Value, ct)
                ?? throw new KeyNotFoundException("Beneficiário não encontrado.");
            if (existing.UsuarioId != currentUser.UserId)
                throw new UnauthorizedAccessException("Acesso negado ao beneficiário.");
            existing.Atualizar(request.Nome.Trim(), request.Papel, request.PercentualDistribuicao, request.CondicaoLiberacao);
            await uow.SaveChangesAsync(ct);
            return existing.Id;
        }

        var entity = new Beneficiario(currentUser.UserId, request.Nome.Trim(),
            request.Papel, request.PercentualDistribuicao, request.CondicaoLiberacao);
        await repo.AddBeneficiarioAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return entity.Id;
    }
}

public record DeleteBeneficiarioCommand(Guid Id) : IRequest;

public class DeleteBeneficiarioCommandHandler(
    IEstruturaRepository repo,
    ICurrentUser currentUser,
    IUnitOfWork uow)
    : IRequestHandler<DeleteBeneficiarioCommand>
{
    public async Task Handle(DeleteBeneficiarioCommand request, CancellationToken ct)
    {
        var entity = await repo.GetBeneficiarioByIdAsync(request.Id, ct)
            ?? throw new KeyNotFoundException("Beneficiário não encontrado.");
        if (entity.UsuarioId != currentUser.UserId)
            throw new UnauthorizedAccessException("Acesso negado ao beneficiário.");

        repo.RemoveBeneficiario(entity);
        await uow.SaveChangesAsync(ct);
    }
}

// ── Distribuição (do cliente; estrutura de origem opcional) ─────────────────

public record SaveDistribuicaoCommand(
    Guid? Id,
    DateTime Data,
    decimal Valor,
    MoedaPatrimonio Moeda,
    Guid? EstruturaId,
    Guid? BeneficiarioId,
    string? Descricao) : IRequest<Guid>;

public class SaveDistribuicaoCommandHandler(
    IEstruturaRepository repo,
    ICurrentUser currentUser,
    IUnitOfWork uow)
    : IRequestHandler<SaveDistribuicaoCommand, Guid>
{
    public async Task<Guid> Handle(SaveDistribuicaoCommand request, CancellationToken ct)
    {
        if (request.Valor <= 0)
            throw new InvalidOperationException("Valor da distribuição deve ser positivo.");

        // Se informou estrutura de origem, ela precisa ser do próprio cliente.
        if (request.EstruturaId.HasValue)
        {
            var est = await repo.GetByIdAsync(request.EstruturaId.Value, ct)
                ?? throw new KeyNotFoundException("Estrutura não encontrada.");
            if (est.UsuarioId != currentUser.UserId)
                throw new UnauthorizedAccessException("Acesso negado à estrutura.");
        }

        if (request.Id.HasValue)
        {
            var existing = await repo.GetDistribuicaoByIdAsync(request.Id.Value, ct)
                ?? throw new KeyNotFoundException("Distribuição não encontrada.");
            if (existing.UsuarioId != currentUser.UserId)
                throw new UnauthorizedAccessException("Acesso negado à distribuição.");
            existing.Atualizar(request.Data, request.Valor, request.Moeda, request.EstruturaId, request.BeneficiarioId, request.Descricao);
            await uow.SaveChangesAsync(ct);
            return existing.Id;
        }

        var entity = new Distribuicao(currentUser.UserId, request.Data, request.Valor, request.Moeda,
            request.EstruturaId, request.BeneficiarioId, request.Descricao);
        await repo.AddDistribuicaoAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return entity.Id;
    }
}

public record DeleteDistribuicaoCommand(Guid Id) : IRequest;

public class DeleteDistribuicaoCommandHandler(
    IEstruturaRepository repo,
    ICurrentUser currentUser,
    IUnitOfWork uow)
    : IRequestHandler<DeleteDistribuicaoCommand>
{
    public async Task Handle(DeleteDistribuicaoCommand request, CancellationToken ct)
    {
        var entity = await repo.GetDistribuicaoByIdAsync(request.Id, ct)
            ?? throw new KeyNotFoundException("Distribuição não encontrada.");
        if (entity.UsuarioId != currentUser.UserId)
            throw new UnauthorizedAccessException("Acesso negado à distribuição.");

        repo.RemoveDistribuicao(entity);
        await uow.SaveChangesAsync(ct);
    }
}
