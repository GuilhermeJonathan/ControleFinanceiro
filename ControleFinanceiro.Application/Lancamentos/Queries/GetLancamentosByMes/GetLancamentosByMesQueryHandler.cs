using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Lancamentos.Queries.GetLancamentosByMes;

public class GetLancamentosByMesQueryHandler(
    ILancamentoRepository repository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<GetLancamentosByMesQuery, IEnumerable<LancamentoDto>>
{
    public async Task<IEnumerable<LancamentoDto>> Handle(GetLancamentosByMesQuery request, CancellationToken cancellationToken)
    {
        var lancamentos = await repository.GetByMesAnoAsync(request.Mes, request.Ano, currentUser.UserId, cancellationToken);

        // Auto-expirar: AVencer com data anterior a hoje → Vencido
        var hoje = DateTime.UtcNow.Date;
        var expirados = lancamentos
            .Where(l => l.Situacao == SituacaoLancamento.AVencer && l.Data.Date < hoje)
            .ToList();

        if (expirados.Count > 0)
        {
            foreach (var l in expirados)
                l.AtualizarSituacao(SituacaoLancamento.Vencido);

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return lancamentos.Select(l => new LancamentoDto(
            l.Id, l.Descricao, l.Data, l.Valor, l.Tipo, l.Situacao,
            l.Mes, l.Ano, l.CategoriaId, l.Categoria?.Nome,
            l.CartaoId, l.Cartao?.Nome, l.ParcelaAtual, l.TotalParcelas, l.GrupoParcelas,
            l.Cartao?.DiaVencimento,
            l.ReceitaRecorrenteId,
            l.ReceitaRecorrente?.Tipo,
            l.ReceitaRecorrente?.ValorHora,
            l.ReceitaRecorrente?.QuantidadeHoras,
            l.IsRecorrente,
            l.ContaBancariaId,
            l.ContaBancaria?.Banco,
            l.DataPagamento));
    }
}
