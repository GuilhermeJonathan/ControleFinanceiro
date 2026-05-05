using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Lancamentos.Commands.CreateTransferencia;

public class CreateTransferenciaCommandHandler(
    ILancamentoRepository lancamentoRepository,
    ISaldoContaRepository saldoRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<CreateTransferenciaCommand, CreateTransferenciaResult>
{
    public async Task<CreateTransferenciaResult> Handle(
        CreateTransferenciaCommand request, CancellationToken cancellationToken)
    {
        var usuarioId = currentUser.UserId;

        // Valida contas (GetByIdAsync já filtra por usuário)
        var origem = await saldoRepository.GetByIdAsync(request.ContaOrigemId, usuarioId, cancellationToken)
            ?? throw new KeyNotFoundException("Conta de origem não encontrada.");
        var destino = await saldoRepository.GetByIdAsync(request.ContaDestinoId, usuarioId, cancellationToken)
            ?? throw new KeyNotFoundException("Conta de destino não encontrada.");

        var transferenciaId = Guid.NewGuid();
        var mes = request.Data.Month;
        var ano = request.Data.Year;

        // Lançamento de DÉBITO na conta de origem
        var debito = new Lancamento(
            descricao:       $"Transferência: {request.Descricao}",
            data:            request.Data,
            valor:           request.Valor,
            tipo:            TipoLancamento.Debito,
            situacao:        SituacaoLancamento.Pago,
            mes:             mes,
            ano:             ano,
            usuarioId:       usuarioId,
            transferenciaId: transferenciaId);
        debito.SetContaBancaria(request.ContaOrigemId);

        // Lançamento de CRÉDITO na conta de destino
        var credito = new Lancamento(
            descricao:       $"Transferência: {request.Descricao}",
            data:            request.Data,
            valor:           request.Valor,
            tipo:            TipoLancamento.Credito,
            situacao:        SituacaoLancamento.Pago,
            mes:             mes,
            ano:             ano,
            usuarioId:       usuarioId,
            transferenciaId: transferenciaId);
        credito.SetContaBancaria(request.ContaDestinoId);

        await lancamentoRepository.AddAsync(debito, cancellationToken);
        await lancamentoRepository.AddAsync(credito, cancellationToken);

        // Atualiza saldos: débito na origem, crédito no destino
        origem.Movimentar(-request.Valor);
        destino.Movimentar(request.Valor);
        saldoRepository.Update(origem);
        saldoRepository.Update(destino);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateTransferenciaResult(debito.Id, credito.Id);
    }
}
