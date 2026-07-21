using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Patrimonio.Queries.GetSucessao;

public record BeneficiarioSucessaoDto(
    Guid Id, string Nome, int Papel, decimal PercentualDistribuicao, string? CondicaoLiberacao);

public record DistribuicaoSucessaoDto(
    Guid Id, DateTime Data, decimal Valor, string Moeda,
    Guid? EstruturaId, string? EstruturaNome, Guid? BeneficiarioId, string? BeneficiarioNome, string? Descricao);

public record SucessaoDto(
    IReadOnlyList<BeneficiarioSucessaoDto> Beneficiarios,
    IReadOnlyList<DistribuicaoSucessaoDto> Distribuicoes)
{
    public SucessaoDto() : this([], []) { }
}

/// <summary>Beneficiários e distribuições da família (do cliente), independente de estrutura.</summary>
public record GetSucessaoQuery : IRequest<SucessaoDto>;

public class GetSucessaoQueryHandler(
    IEstruturaRepository repo,
    ICurrentUser currentUser)
    : IRequestHandler<GetSucessaoQuery, SucessaoDto>
{
    public async Task<SucessaoDto> Handle(GetSucessaoQuery request, CancellationToken ct)
    {
        var userId = currentUser.UserId;

        var beneficiarios = await repo.GetBeneficiariosByUsuarioAsync(userId, ct);
        var distribs      = await repo.GetDistribuicoesByUsuarioAsync(userId, ct);
        var estruturas    = await repo.GetByUsuarioAsync(userId, ct);
        var nomeEstrutura = estruturas.ToDictionary(e => e.Id, e => e.Nome);
        var nomeBenef     = beneficiarios.ToDictionary(b => b.Id, b => b.Nome);

        var benefDtos = beneficiarios
            .Select(b => new BeneficiarioSucessaoDto(b.Id, b.Nome, (int)b.Papel, b.PercentualDistribuicao, b.CondicaoLiberacao))
            .ToList();

        var distDtos = distribs
            .Select(d => new DistribuicaoSucessaoDto(
                d.Id, d.Data, Math.Round(d.Valor, 2), d.Moeda.ToString(),
                d.EstruturaId, d.EstruturaId.HasValue ? nomeEstrutura.GetValueOrDefault(d.EstruturaId.Value) : null,
                d.BeneficiarioId, d.BeneficiarioId.HasValue ? nomeBenef.GetValueOrDefault(d.BeneficiarioId.Value) : null,
                d.Descricao))
            .ToList();

        return new SucessaoDto(benefDtos, distDtos);
    }
}
