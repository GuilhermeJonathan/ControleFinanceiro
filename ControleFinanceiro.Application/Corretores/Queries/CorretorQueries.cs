using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Corretores.Queries;

// ── DTOs ─────────────────────────────────────────────────────────────────────

public record CorretorDto(
    Guid VinculoId,
    Guid CorretorId,
    string? NomeCorretor,
    string CodigoConvite,
    DateTime CriadoEm,
    DateTime? AceitoEm,
    DateTime? RevogadoEm,
    bool Ativo,
    int QtdClientesDelegados);

public record DelegacaoDto(
    Guid Id,
    Guid CorretorId,
    string? NomeCorretor,
    Guid ClienteId,
    string? NomeCliente,
    DateTime DelegadoEm,
    DateTime? RevogadoEm,
    bool Ativa);

public record ClienteDelegadoDto(
    Guid ClienteId,
    string? NomeCliente,
    Guid DelegacaoId,
    DateTime DelegadoEm);

// ── Assessor lista seus corretores ───────────────────────────────────────────

public record GetCorretoresQuery : IRequest<IEnumerable<CorretorDto>>;

public class GetCorretoresQueryHandler(
    IVinculoCorretorRepository corretorRepo,
    IDelegacaoCarteiraRepository delegacaoRepo,
    ICurrentUser currentUser) : IRequestHandler<GetCorretoresQuery, IEnumerable<CorretorDto>>
{
    public async Task<IEnumerable<CorretorDto>> Handle(GetCorretoresQuery request, CancellationToken ct)
    {
        var vinculos = await corretorRepo.GetByAssessorAsync(currentUser.RealUserId, ct);
        var delegacoes = await delegacaoRepo.GetByAssessorAsync(currentUser.RealUserId, ct);

        return vinculos.Select(v => new CorretorDto(
            v.Id, v.CorretorId, v.NomeCorretor, v.CodigoConvite,
            v.CriadoEm, v.AceitoEm, v.RevogadoEm, v.Ativo,
            delegacoes.Count(d => d.CorretorId == v.CorretorId && d.Ativa)));
    }
}

// ── Assessor lista delegações (histórico completo) ───────────────────────────

public record GetDelegacoesQuery : IRequest<IEnumerable<DelegacaoDto>>;

public class GetDelegacoesQueryHandler(
    IDelegacaoCarteiraRepository delegacaoRepo,
    ICurrentUser currentUser) : IRequestHandler<GetDelegacoesQuery, IEnumerable<DelegacaoDto>>
{
    public async Task<IEnumerable<DelegacaoDto>> Handle(GetDelegacoesQuery request, CancellationToken ct)
    {
        var delegacoes = await delegacaoRepo.GetByAssessorAsync(currentUser.RealUserId, ct);
        return delegacoes.Select(d => new DelegacaoDto(
            d.Id, d.CorretorId, d.NomeCorretor, d.ClienteId,
            d.NomeCliente, d.DelegadoEm, d.RevogadoEm, d.Ativa));
    }
}

// ── Corretor lista seus clientes delegados ───────────────────────────────────

public record GetClientesDelegadosQuery : IRequest<IEnumerable<ClienteDelegadoDto>>;

public class GetClientesDelegadosQueryHandler(
    IDelegacaoCarteiraRepository delegacaoRepo,
    ICurrentUser currentUser) : IRequestHandler<GetClientesDelegadosQuery, IEnumerable<ClienteDelegadoDto>>
{
    public async Task<IEnumerable<ClienteDelegadoDto>> Handle(GetClientesDelegadosQuery request, CancellationToken ct)
    {
        var delegacoes = await delegacaoRepo.GetByCorretorAsync(currentUser.UserId, ct);
        return delegacoes.Select(d => new ClienteDelegadoDto(
            d.ClienteId, d.NomeCliente, d.Id, d.DelegadoEm));
    }
}
