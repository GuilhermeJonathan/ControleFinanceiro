using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Corretores.Commands;

// ── Delegar carteira a corretor ──────────────────────────────────────────────

public record DelegarCarteiraCommand(Guid CorretorId, Guid ClienteId) : IRequest<Guid>;

public class DelegarCarteiraCommandHandler(
    IVinculoCorretorRepository corretorRepo,
    IVinculoAssessoriaRepository assessoriaRepo,
    IDelegacaoCarteiraRepository delegacaoRepo,
    ICurrentUser currentUser,
    IUnitOfWork uow) : IRequestHandler<DelegarCarteiraCommand, Guid>
{
    public async Task<Guid> Handle(DelegarCarteiraCommand request, CancellationToken ct)
    {
        if (!currentUser.IsAssessor)
            throw new UnauthorizedAccessException("Apenas assessores podem delegar carteiras.");

        // Corretor deve ser subordinado deste assessor
        var corretos = await corretorRepo.GetByAssessorAsync(currentUser.RealUserId, ct);
        if (!corretos.Any(v => v.CorretorId == request.CorretorId && v.Ativo))
            throw new InvalidOperationException("Corretor não encontrado ou não ativo.");

        // Cliente deve pertencer à carteira deste assessor
        var vinculos = await assessoriaRepo.GetByAssessorAsync(currentUser.RealUserId, ct);
        var vinculoCliente = vinculos.FirstOrDefault(v => v.ClienteId == request.ClienteId && v.Ativo)
            ?? throw new InvalidOperationException("Cliente não encontrado na sua carteira.");

        // Não duplicar delegação ativa
        if (await delegacaoRepo.ExisteAtivaAsync(request.CorretorId, request.ClienteId, ct))
            throw new InvalidOperationException("Este cliente já está delegado a este corretor.");

        var nomeCorretor = corretos.First(v => v.CorretorId == request.CorretorId).NomeCorretor;
        var delegacao = DelegacaoCarteira.Criar(
            currentUser.RealUserId,
            request.CorretorId,
            vinculoCliente.Id,
            request.ClienteId,
            vinculoCliente.NomeCliente,
            nomeCorretor);

        await delegacaoRepo.AddAsync(delegacao, ct);
        await uow.SaveChangesAsync(ct);

        return delegacao.Id;
    }
}

// ── Revogar delegação ────────────────────────────────────────────────────────

public record RevogarDelegacaoCommand(Guid DelegacaoId) : IRequest;

public class RevogarDelegacaoCommandHandler(
    IDelegacaoCarteiraRepository delegacaoRepo,
    ICurrentUser currentUser,
    IUnitOfWork uow) : IRequestHandler<RevogarDelegacaoCommand>
{
    public async Task Handle(RevogarDelegacaoCommand request, CancellationToken ct)
    {
        var delegacao = await delegacaoRepo.GetByIdAsync(request.DelegacaoId, ct)
            ?? throw new KeyNotFoundException("Delegação não encontrada.");

        if (delegacao.AssessorId != currentUser.RealUserId)
            throw new UnauthorizedAccessException("Acesso negado.");

        delegacao.Revogar();
        delegacaoRepo.Update(delegacao);
        await uow.SaveChangesAsync(ct);
    }
}
