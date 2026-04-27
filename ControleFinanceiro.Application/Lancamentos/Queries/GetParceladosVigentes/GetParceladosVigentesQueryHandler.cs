using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Lancamentos.Queries.GetParceladosVigentes;

public class GetParceladosVigentesQueryHandler(
    ILancamentoRepository lancamentoRepository,
    ICurrentUser currentUser)
    : IRequestHandler<GetParceladosVigentesQuery, ParceladosVigentesResultDto>
{
    public async Task<ParceladosVigentesResultDto> Handle(
        GetParceladosVigentesQuery request, CancellationToken cancellationToken)
    {
        var usuarioId = currentUser.UserId;
        var lancamentos = (await lancamentoRepository
            .GetParceladosVigentesAsync(usuarioId, cancellationToken)).ToList();

        // Agrupa por GrupoParcelas (se existir) ou por (Descricao + CartaoId)
        var grupos = lancamentos
            .GroupBy(l => l.GrupoParcelas.HasValue
                ? l.GrupoParcelas.Value.ToString()
                : $"{l.Descricao}|{l.CartaoId}")
            .Select(g =>
            {
                var primeiro = g.OrderBy(l => l.ParcelaAtual).First();
                return new ParceladoVigenteItemDto(
                    Descricao:     primeiro.Descricao,
                    CategoriaNome: primeiro.Categoria?.Nome,
                    CartaoNome:    primeiro.Cartao?.Nome,
                    PrimeiraData:  g.Min(l => l.Data),
                    ParcelaMin:    g.Min(l => l.ParcelaAtual!.Value),
                    TotalParcelas: primeiro.TotalParcelas ?? g.Count(),
                    ValorParcela:  primeiro.Valor,
                    SaldoRestante: g.Sum(l => l.Valor)
                );
            })
            .OrderByDescending(i => i.SaldoRestante)
            .ToList();

        return new ParceladosVigentesResultDto(
            TotalDivida: grupos.Sum(i => i.SaldoRestante),
            Itens: grupos);
    }
}
