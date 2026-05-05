using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Lancamentos.Commands.ImportarExtrato;

public class ImportarExtratoCommandHandler(
    ILancamentoRepository  lancamentoRepository,
    ICategoriaRepository   categoriaRepository,
    IUnitOfWork            unitOfWork,
    ICurrentUser           currentUser)
    : IRequestHandler<ImportarExtratoCommand, int>
{
    public async Task<int> Handle(ImportarExtratoCommand request, CancellationToken cancellationToken)
    {
        var usuarioId = currentUser.UserId;

        var categoriasExistentes = (await categoriaRepository.GetAllAsync(usuarioId, cancellationToken)).ToList();
        var lookup = categoriasExistentes.ToDictionary(c => c.Nome.Trim().ToLowerInvariant(), c => c.Id);
        var novas  = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

        var lancamentos = new List<Lancamento>();

        foreach (var item in request.Items)
        {
            var nomeNorm = (item.CategoriaNome ?? "Outros").Trim();
            if (string.IsNullOrWhiteSpace(nomeNorm)) nomeNorm = "Outros";
            var chave = nomeNorm.ToLowerInvariant();

            Guid categoriaId;
            if (lookup.TryGetValue(chave, out var idEx))          categoriaId = idEx;
            else if (novas.TryGetValue(chave, out var idNova))    categoriaId = idNova;
            else
            {
                var nova = new Categoria(nomeNorm, TipoLancamento.Debito, usuarioId);
                await categoriaRepository.AddAsync(nova, cancellationToken);
                novas[chave] = lookup[chave] = nova.Id;
                categoriaId = nova.Id;
            }

            var tipo  = item.Valor >= 0 ? TipoLancamento.Credito : TipoLancamento.Debito;
            var valor = Math.Abs(item.Valor);

            var l = new Lancamento(
                descricao:   item.Descricao,
                data:        item.Data,
                valor:       valor,
                tipo:        tipo,
                situacao:    SituacaoLancamento.Pago,
                mes:         item.Mes,
                ano:         item.Ano,
                categoriaId: categoriaId,
                usuarioId:   usuarioId);

            if (item.ContaBancariaId.HasValue)
                l.SetContaBancaria(item.ContaBancariaId);

            lancamentos.Add(l);
        }

        await lancamentoRepository.AddRangeAsync(lancamentos, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return lancamentos.Count;
    }
}
