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

        // Agrupa por GrupoParcelas (se existir) ou por (DescricaoBase + CartaoId)
        // DescricaoBase remove o sufixo "(N/M)" de parcelados antigos sem GrupoParcelas
        var grupos = lancamentos
            .GroupBy(l => l.GrupoParcelas.HasValue
                ? l.GrupoParcelas.Value.ToString()
                : $"{NormalizarDescricao(l.Descricao)}|{l.CartaoId}")
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

    // Remove sufixo "(N/M)" gerado em parcelados antigos sem GrupoParcelas
    // Ex: "Netflix (3/12)" → "Netflix"
    private static string NormalizarDescricao(string descricao)
    {
        var idx = descricao.LastIndexOf('(');
        if (idx <= 0) return descricao;

        var sufixo = descricao[idx..].Trim();
        // Valida formato "(N/M)" onde N e M são números
        if (System.Text.RegularExpressions.Regex.IsMatch(sufixo, @"^\(\d+/\d+\)$"))
            return descricao[..idx].Trim();

        return descricao;
    }
}
