using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Lancamentos.Commands.AtualizarSituacao;

public class AtualizarSituacaoCommandHandler(
    ILancamentoRepository lancamentoRepository,
    ISaldoContaRepository contaRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AtualizarSituacaoCommand>
{
    private static bool IsConfirmado(SituacaoLancamento s)
        => s is SituacaoLancamento.Pago or SituacaoLancamento.Recebido;

    public async Task Handle(AtualizarSituacaoCommand request, CancellationToken cancellationToken)
    {
        var lancamento = await lancamentoRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Lançamento {request.Id} não encontrado.");

        var eraConfirmado  = IsConfirmado(lancamento.Situacao);
        var seraConfirmado = IsConfirmado(request.Situacao);

        // ── Rollback: estava confirmado e vai ser desconfirmado ──────────────
        if (eraConfirmado && !seraConfirmado && lancamento.ContaBancariaId.HasValue)
        {
            var contaAntiga = await contaRepository.GetByIdAsync(lancamento.ContaBancariaId.Value, cancellationToken);
            if (contaAntiga is not null)
            {
                // Reverte: receita → subtrai, despesa → soma
                var estorno = lancamento.Tipo == TipoLancamento.Credito
                    ? -lancamento.Valor
                    : lancamento.Valor;
                contaAntiga.Movimentar(estorno);
                contaRepository.Update(contaAntiga);
            }
            lancamento.SetContaBancaria(null);
        }

        // ── Confirmação: não estava confirmado e vai ser confirmado ──────────
        if (!eraConfirmado && seraConfirmado && request.ContaBancariaId.HasValue)
        {
            var conta = await contaRepository.GetByIdAsync(request.ContaBancariaId.Value, cancellationToken);
            if (conta is not null)
            {
                // Receita → soma, despesa → subtrai
                var movimento = lancamento.Tipo == TipoLancamento.Credito
                    ? lancamento.Valor
                    : -lancamento.Valor;
                conta.Movimentar(movimento);
                contaRepository.Update(conta);
            }
            lancamento.SetContaBancaria(request.ContaBancariaId);
        }

        lancamento.AtualizarSituacao(request.Situacao);
        lancamentoRepository.Update(lancamento);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
