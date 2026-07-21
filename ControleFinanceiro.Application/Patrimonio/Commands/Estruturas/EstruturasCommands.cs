using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Patrimonio.Commands.Estruturas;

// ── Save Estrutura (create/update) ──────────────────────────────────────────

public record SaveEstruturaCommand(
    Guid? Id,
    string Nome,
    TipoEstrutura Tipo,
    string? Jurisdicao,
    DateTime? ConstituidaEm,
    string? Observacoes) : IRequest<Guid>;

public class SaveEstruturaCommandHandler(
    IEstruturaRepository repo,
    ICurrentUser currentUser,
    IUnitOfWork uow)
    : IRequestHandler<SaveEstruturaCommand, Guid>
{
    public async Task<Guid> Handle(SaveEstruturaCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
            throw new InvalidOperationException("Informe o nome da estrutura.");

        if (request.Id.HasValue)
        {
            var existing = await repo.GetByIdAsync(request.Id.Value, ct)
                ?? throw new KeyNotFoundException($"Estrutura {request.Id} não encontrada.");
            if (existing.UsuarioId != currentUser.UserId)
                throw new UnauthorizedAccessException("Acesso negado à estrutura.");

            existing.Atualizar(request.Nome.Trim(), request.Tipo, request.Jurisdicao,
                request.ConstituidaEm, request.Observacoes);
            await uow.SaveChangesAsync(ct);
            return existing.Id;
        }

        var entity = new Estrutura(currentUser.UserId, request.Nome.Trim(), request.Tipo,
            request.Jurisdicao, request.ConstituidaEm, request.Observacoes);
        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return entity.Id;
    }
}

// ── Delete Estrutura ─────────────────────────────────────────────────────────
// Remove a estrutura + suas participações; ativos/investimentos voltam para pessoa física.

public record DeleteEstruturaCommand(Guid Id) : IRequest;

public class DeleteEstruturaCommandHandler(
    IEstruturaRepository repo,
    IAtivoPatrimonialRepository ativoRepo,
    IInvestimentoRepository investimentoRepo,
    ICurrentUser currentUser,
    IUnitOfWork uow)
    : IRequestHandler<DeleteEstruturaCommand>
{
    public async Task Handle(DeleteEstruturaCommand request, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(request.Id, ct)
            ?? throw new KeyNotFoundException($"Estrutura {request.Id} não encontrada.");
        if (entity.UsuarioId != currentUser.UserId)
            throw new UnauthorizedAccessException("Acesso negado à estrutura.");

        // Solta os bens ligados (voltam para pessoa física).
        foreach (var a in (await ativoRepo.GetByUsuarioAsync(currentUser.UserId, ct)).Where(a => a.EstruturaId == entity.Id))
            a.DesvincularEstrutura();
        foreach (var i in (await investimentoRepo.GetByUsuarioAsync(currentUser.UserId, ct)).Where(i => i.EstruturaId == entity.Id))
            i.DesvincularEstrutura();

        // Remove as arestas onde ela participa (como pai ou filha).
        foreach (var p in (await repo.GetParticipacoesByUsuarioAsync(currentUser.UserId, ct))
                 .Where(p => p.EstruturaPaiId == entity.Id || p.EstruturaFilhaId == entity.Id))
            repo.RemoveParticipacao(p);

        repo.Remove(entity);
        await uow.SaveChangesAsync(ct);
    }
}

// ── Save Participação (aresta do grafo) ─────────────────────────────────────

public record SaveParticipacaoCommand(
    Guid? EstruturaPaiId,   // null = família/cliente (raiz)
    Guid EstruturaFilhaId,
    decimal PercentualParticipacao,
    TipoRelacaoEstrutura TipoRelacao) : IRequest<Guid>;

public class SaveParticipacaoCommandHandler(
    IEstruturaRepository repo,
    ICurrentUser currentUser,
    IUnitOfWork uow)
    : IRequestHandler<SaveParticipacaoCommand, Guid>
{
    public async Task<Guid> Handle(SaveParticipacaoCommand request, CancellationToken ct)
    {
        if (request.PercentualParticipacao is <= 0 or > 100)
            throw new InvalidOperationException("Percentual de participação deve estar entre 0 e 100.");
        if (request.EstruturaPaiId == request.EstruturaFilhaId)
            throw new InvalidOperationException("Uma estrutura não pode deter a si mesma.");

        var userId = currentUser.UserId;

        var filha = await repo.GetByIdAsync(request.EstruturaFilhaId, ct)
            ?? throw new KeyNotFoundException("Estrutura detida não encontrada.");
        if (filha.UsuarioId != userId)
            throw new UnauthorizedAccessException("Acesso negado à estrutura.");

        if (request.EstruturaPaiId.HasValue)
        {
            var pai = await repo.GetByIdAsync(request.EstruturaPaiId.Value, ct)
                ?? throw new KeyNotFoundException("Estrutura detentora não encontrada.");
            if (pai.UsuarioId != userId)
                throw new UnauthorizedAccessException("Acesso negado à estrutura.");
        }

        var participacoes = await repo.GetParticipacoesByUsuarioAsync(userId, ct);

        // Anticiclo: a partir da FILHA, descendo pelas participações, não se pode alcançar o PAI.
        if (request.EstruturaPaiId.HasValue &&
            AlcancaDescendo(request.EstruturaFilhaId, request.EstruturaPaiId.Value, participacoes))
            throw new InvalidOperationException("Participação criaria um ciclo no grafo (A detém B que detém A).");

        // Aresta (pai, filha) já existe → atualiza percentual/relação.
        var existente = participacoes.FirstOrDefault(p =>
            p.EstruturaPaiId == request.EstruturaPaiId && p.EstruturaFilhaId == request.EstruturaFilhaId);
        if (existente is not null)
        {
            existente.Atualizar(request.PercentualParticipacao, request.TipoRelacao);
            await uow.SaveChangesAsync(ct);
            return existente.Id;
        }

        var nova = new ParticipacaoEstrutura(userId, request.EstruturaPaiId, request.EstruturaFilhaId,
            request.PercentualParticipacao, request.TipoRelacao);
        await repo.AddParticipacaoAsync(nova, ct);
        await uow.SaveChangesAsync(ct);
        return nova.Id;
    }

    /// <summary>BFS descendo o grafo a partir de "origem": true se alcançar "alvo".</summary>
    private static bool AlcancaDescendo(Guid origem, Guid alvo, List<ParticipacaoEstrutura> arestas)
    {
        var fila = new Queue<Guid>();
        var visitados = new HashSet<Guid> { origem };
        fila.Enqueue(origem);
        while (fila.Count > 0)
        {
            var atual = fila.Dequeue();
            foreach (var p in arestas.Where(a => a.EstruturaPaiId == atual))
            {
                if (p.EstruturaFilhaId == alvo) return true;
                if (visitados.Add(p.EstruturaFilhaId)) fila.Enqueue(p.EstruturaFilhaId);
            }
        }
        return false;
    }
}

// ── Delete Participação ──────────────────────────────────────────────────────

public record DeleteParticipacaoCommand(Guid Id) : IRequest;

public class DeleteParticipacaoCommandHandler(
    IEstruturaRepository repo,
    ICurrentUser currentUser,
    IUnitOfWork uow)
    : IRequestHandler<DeleteParticipacaoCommand>
{
    public async Task Handle(DeleteParticipacaoCommand request, CancellationToken ct)
    {
        var entity = await repo.GetParticipacaoByIdAsync(request.Id, ct)
            ?? throw new KeyNotFoundException($"Participação {request.Id} não encontrada.");
        if (entity.UsuarioId != currentUser.UserId)
            throw new UnauthorizedAccessException("Acesso negado à participação.");

        repo.RemoveParticipacao(entity);
        await uow.SaveChangesAsync(ct);
    }
}
