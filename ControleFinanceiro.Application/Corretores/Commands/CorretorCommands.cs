using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Corretores.Commands;

// ── Gerar convite para corretor ──────────────────────────────────────────────

public record GerarConviteCorretorCommand : IRequest<string>;

public class GerarConviteCorretorCommandHandler(
    IVinculoCorretorRepository repo,
    ICurrentUser currentUser,
    IUnitOfWork uow) : IRequestHandler<GerarConviteCorretorCommand, string>
{
    private const string Chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public async Task<string> Handle(GerarConviteCorretorCommand request, CancellationToken ct)
    {
        if (!currentUser.IsAssessor)
            throw new UnauthorizedAccessException("Apenas assessores podem convidar corretores.");

        string codigo;
        do
        {
            codigo = new string(Enumerable.Range(0, 6)
                .Select(_ => Chars[Random.Shared.Next(Chars.Length)])
                .ToArray());
        } while (await repo.GetByCodigoAsync(codigo, ct) != null);

        var vinculo = VinculoCorretor.Criar(currentUser.RealUserId, codigo, currentUser.RealUserName);
        await repo.AddAsync(vinculo, ct);
        await uow.SaveChangesAsync(ct);

        return codigo;
    }
}

// ── Corretor aceita convite ──────────────────────────────────────────────────

public record AceitarConviteCorretorCommand(string Codigo) : IRequest;

public class AceitarConviteCorretorCommandHandler(
    IVinculoCorretorRepository repo,
    ICurrentUser currentUser,
    IUnitOfWork uow) : IRequestHandler<AceitarConviteCorretorCommand>
{
    public async Task Handle(AceitarConviteCorretorCommand request, CancellationToken ct)
    {
        var vinculo = await repo.GetByCodigoAsync(request.Codigo, ct)
            ?? throw new KeyNotFoundException("Código de convite inválido ou já utilizado.");

        if (vinculo.AceitoEm != null)
            throw new InvalidOperationException("Este convite já foi utilizado.");

        // Verifica se já é corretor deste assessor
        var existentes = await repo.GetByCorretorAsync(currentUser.UserId, ct);
        if (existentes.Any(v => v.AssessorId == vinculo.AssessorId && v.RevogadoEm == null))
            throw new InvalidOperationException("Você já é corretor deste assessor.");

        vinculo.Aceitar(currentUser.UserId, currentUser.RealUserName ?? "Corretor");
        repo.Update(vinculo);
        await uow.SaveChangesAsync(ct);
    }
}

// ── Assessor revoga corretor ─────────────────────────────────────────────────

public record RevogarCorretorCommand(Guid VinculoId) : IRequest;

public class RevogarCorretorCommandHandler(
    IVinculoCorretorRepository repo,
    IDelegacaoCarteiraRepository delegacaoRepo,
    ICurrentUser currentUser,
    IUnitOfWork uow) : IRequestHandler<RevogarCorretorCommand>
{
    public async Task Handle(RevogarCorretorCommand request, CancellationToken ct)
    {
        var vinculo = await repo.GetByIdAsync(request.VinculoId, ct)
            ?? throw new KeyNotFoundException("Vínculo não encontrado.");

        if (vinculo.AssessorId != currentUser.RealUserId)
            throw new UnauthorizedAccessException("Acesso negado.");

        vinculo.Revogar();
        repo.Update(vinculo);

        // Revoga todas as delegações ativas deste corretor
        var delegacoes = await delegacaoRepo.GetByCorretorAsync(vinculo.CorretorId, ct);
        foreach (var d in delegacoes.Where(d => d.AssessorId == currentUser.RealUserId && d.Ativa))
        {
            d.Revogar();
            delegacaoRepo.Update(d);
        }

        await uow.SaveChangesAsync(ct);
    }
}
