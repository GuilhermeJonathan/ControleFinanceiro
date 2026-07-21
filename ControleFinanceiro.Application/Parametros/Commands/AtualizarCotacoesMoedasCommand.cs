using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Parametros.Commands;

public record AtualizarCotacoesResult(int Atualizadas, bool Pulado);

/// <summary>
/// Busca as cotações das moedas ativas (≠ BRL) na API e atualiza CotacaoBRL + grava histórico.
/// Forcar=false respeita um "frescor" (não reconsulta se já atualizou há pouco) — usado pelo job diário.
/// Forcar=true ignora o frescor — usado no botão "Atualizar agora".
/// </summary>
public record AtualizarCotacoesMoedasCommand(bool Forcar = false) : IRequest<AtualizarCotacoesResult>;

public class AtualizarCotacoesMoedasCommandHandler(
    IMoedaParamRepository moedaRepo,
    ICurrencyRateService rateService,
    ICotacaoHistoricoRepository historicoRepo,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AtualizarCotacoesMoedasCommand, AtualizarCotacoesResult>
{
    private static readonly TimeSpan Frescor = TimeSpan.FromHours(3);

    public async Task<AtualizarCotacoesResult> Handle(AtualizarCotacoesMoedasCommand request, CancellationToken ct)
    {
        var moedas = (await moedaRepo.GetAllAsync(ct))
            .Where(m => m.Ativo && !string.Equals(m.Codigo, "BRL", StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (moedas.Count == 0) return new AtualizarCotacoesResult(0, false);

        // Guarda de frescor: evita marteladas na API externa a cada restart/execução.
        if (!request.Forcar)
        {
            var maisRecente = moedas
                .Where(m => m.CotacaoAtualizadaEm.HasValue)
                .Select(m => m.CotacaoAtualizadaEm!.Value)
                .DefaultIfEmpty()
                .Max();
            if (maisRecente != default && DateTime.UtcNow - maisRecente < Frescor)
                return new AtualizarCotacoesResult(0, true);
        }

        var codigos = moedas.Select(m => m.Codigo.ToUpperInvariant()).Distinct().ToList();
        var rates = await rateService.GetRatesVsBrlAsync(codigos, ct);
        if (rates.Count == 0) return new AtualizarCotacoesResult(0, false);

        var atualizadas = 0;
        foreach (var codigo in codigos)
        {
            if (!rates.TryGetValue(codigo, out var cotacao)) continue;
            // Aplica em todas as linhas com esse código; grava UM histórico por código (dedup).
            foreach (var m in moedas.Where(m => string.Equals(m.Codigo, codigo, StringComparison.OrdinalIgnoreCase)))
                m.AtualizarCotacao(cotacao);
            await historicoRepo.AddAsync(new CotacaoHistorico(codigo, cotacao, "AwesomeAPI"), ct);
            atualizadas++;
        }

        await unitOfWork.SaveChangesAsync(ct);
        return new AtualizarCotacoesResult(atualizadas, false);
    }
}
