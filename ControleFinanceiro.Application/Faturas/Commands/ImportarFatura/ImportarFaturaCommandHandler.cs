using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Faturas.Commands.ImportarFatura;

public class ImportarFaturaCommandHandler(
    ILancamentoRepository  lancamentoRepository,
    ICategoriaRepository   categoriaRepository,
    IUnitOfWork            unitOfWork,
    ICurrentUser           currentUser)
    : IRequestHandler<ImportarFaturaCommand, int>
{
    public async Task<int> Handle(ImportarFaturaCommand request, CancellationToken cancellationToken)
    {
        var usuarioId = currentUser.UserId;

        // Carrega todas as categorias do usuário (lookup nome → id, case-insensitive)
        var categoriasExistentes = (await categoriaRepository.GetAllAsync(usuarioId, cancellationToken))
            .ToList();

        var lookup = categoriasExistentes
            .ToDictionary(c => c.Nome.Trim().ToLowerInvariant(), c => c.Id);

        // Cache de novas categorias criadas nesta importação (evita duplicatas dentro do mesmo batch)
        var novas = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

        var lancamentos = new List<Lancamento>();

        foreach (var item in request.Items)
        {
            // Resolve categoria
            var nomeNorm = (item.CategoriaNome ?? "Outros").Trim();
            if (string.IsNullOrWhiteSpace(nomeNorm)) nomeNorm = "Outros";
            var chave    = nomeNorm.ToLowerInvariant();

            Guid categoriaId;

            if (lookup.TryGetValue(chave, out var idExistente))
            {
                categoriaId = idExistente;
            }
            else if (novas.TryGetValue(chave, out var idNova))
            {
                categoriaId = idNova;
            }
            else
            {
                // Cria categoria nova
                var nova = new Categoria(nomeNorm, TipoLancamento.Debito, usuarioId);
                await categoriaRepository.AddAsync(nova, cancellationToken);
                novas[chave]    = nova.Id;
                lookup[chave]   = nova.Id;   // evita recriar se aparecer novamente
                categoriaId     = nova.Id;
            }

            lancamentos.Add(new Lancamento(
                descricao:    item.Descricao,
                data:         item.Data,
                valor:        item.Valor,
                tipo:         TipoLancamento.Debito,
                situacao:     SituacaoLancamento.AVencer,
                mes:          item.Mes,
                ano:          item.Ano,
                categoriaId:  categoriaId,
                cartaoId:     item.CartaoId,
                parcelaAtual: item.ParcelaAtual,
                totalParcelas:item.TotalParcelas,
                usuarioId:    usuarioId));
        }

        await lancamentoRepository.AddRangeAsync(lancamentos, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return lancamentos.Count;
    }
}
