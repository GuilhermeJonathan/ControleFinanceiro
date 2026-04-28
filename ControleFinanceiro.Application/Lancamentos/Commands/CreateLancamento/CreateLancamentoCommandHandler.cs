using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Lancamentos.Commands.CreateLancamento;

public class CreateLancamentoCommandHandler(
    ILancamentoRepository repository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<CreateLancamentoCommand, Guid>
{
    public async Task<Guid> Handle(CreateLancamentoCommand request, CancellationToken cancellationToken)
    {
        var usuarioId    = currentUser.UserId;
        var criadoPorId  = currentUser.RealUserId;
        var criadoPorNome = currentUser.RealUserName;
        var totalParcelas = request.TotalParcelas < 1 ? 1 : request.TotalParcelas;

        if (totalParcelas == 1)
        {
            var lancamento = new Lancamento(
                request.Descricao, request.Data, request.Valor,
                request.Tipo, request.Situacao, request.Mes, request.Ano,
                request.CategoriaId, request.CartaoId,
                usuarioId: usuarioId,
                criadoPorId: criadoPorId,
                criadoPorNome: criadoPorNome);

            await repository.AddAsync(lancamento, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return lancamento.Id;
        }

        var grupo = Guid.NewGuid();
        Guid primeiroId = Guid.Empty;
        var lancamentos = new List<Lancamento>();

        // Recorrente: mesmo valor todo mês, descrição sem sufixo "(1/N)"
        // Parcela   : valor dividido,      descrição com sufixo "(1/N)"
        var valorParcela = request.IsRecorrente
            ? request.Valor
            : Math.Round(request.Valor / totalParcelas, 2);
        var valorUltima = request.IsRecorrente
            ? request.Valor
            : request.Valor - valorParcela * (totalParcelas - 1);

        for (int i = 0; i < totalParcelas; i++)
        {
            var mesBase = request.Mes - 1 + i;
            var mesAtual = mesBase % 12 + 1;
            var anoAtual = request.Ano + mesBase / 12;

            var diaMax = DateTime.DaysInMonth(anoAtual, mesAtual);
            var dia = Math.Min(request.Data.Day, diaMax);
            var dataAtual = new DateTime(anoAtual, mesAtual, dia);

            var descricao = request.IsRecorrente
                ? request.Descricao
                : $"{request.Descricao} ({i + 1}/{totalParcelas})";

            var situacao = i == 0
                ? request.Situacao
                : SituacaoLancamento.AVencer;

            var valor = i == totalParcelas - 1 ? valorUltima : valorParcela;

            var l = new Lancamento(
                descricao, dataAtual, valor,
                request.Tipo, situacao, mesAtual, anoAtual,
                request.CategoriaId, request.CartaoId,
                i + 1, totalParcelas, grupo,
                isRecorrente: request.IsRecorrente,
                usuarioId: usuarioId,
                criadoPorId: criadoPorId,
                criadoPorNome: criadoPorNome);

            lancamentos.Add(l);
            if (i == 0) primeiroId = l.Id;
        }

        await repository.AddRangeAsync(lancamentos, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return primeiroId;
    }
}
