using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.ReceitasRecorrentes.Commands.CreateReceitaRecorrente;

public class CreateReceitaRecorrenteCommandHandler(
    IReceitaRecorrenteRepository receitaRepository,
    ILancamentoRepository lancamentoRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateReceitaRecorrenteCommand, Guid>
{
    public async Task<Guid> Handle(CreateReceitaRecorrenteCommand request, CancellationToken cancellationToken)
    {
        var hoje = DateTime.Today;
        var dataInicio = new DateTime(hoje.Year, hoje.Month, 1);

        var receita = new ReceitaRecorrente(
            request.Nome, request.Tipo, request.Dia, dataInicio,
            request.Valor, request.ValorHora, request.QuantidadeHoras);

        await receitaRepository.AddAsync(receita, cancellationToken);

        var meses = Math.Max(1, request.Meses);
        var lancamentos = new List<Lancamento>();

        for (int i = 0; i < meses; i++)
        {
            var data = new DateTime(hoje.Year, hoje.Month, 1).AddMonths(i);
            var dia = Math.Min(receita.Dia, DateTime.DaysInMonth(data.Year, data.Month));
            var dataLancamento = new DateTime(data.Year, data.Month, dia);

            var lancamento = new Lancamento(
                descricao: receita.Nome,
                data: dataLancamento,
                valor: receita.Valor,
                tipo: TipoLancamento.Credito,
                situacao: dataLancamento.Date <= hoje ? SituacaoLancamento.Recebido : SituacaoLancamento.AReceber,
                mes: data.Month,
                ano: data.Year,
                receitaRecorrenteId: receita.Id
            );
            lancamentos.Add(lancamento);
        }

        await lancamentoRepository.AddRangeAsync(lancamentos, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return receita.Id;
    }
}
