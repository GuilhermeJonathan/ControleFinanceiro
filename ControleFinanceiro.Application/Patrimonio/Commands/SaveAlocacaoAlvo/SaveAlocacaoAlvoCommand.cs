using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Patrimonio.Commands.SaveAlocacaoAlvo;

public record AlvoItem(int Tipo, decimal PercentualAlvo);

/// <summary>Substitui a alocação-alvo do usuário efetivo (upsert do conjunto completo).</summary>
public record SaveAlocacaoAlvoCommand(IEnumerable<AlvoItem> Alvos) : IRequest;

public class SaveAlocacaoAlvoCommandHandler(
    IAlocacaoAlvoRepository repository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : IRequestHandler<SaveAlocacaoAlvoCommand>
{
    public async Task Handle(SaveAlocacaoAlvoCommand request, CancellationToken cancellationToken)
    {
        await repository.RemoveByUsuarioAsync(currentUser.UserId, cancellationToken);

        var novos = (request.Alvos ?? [])
            .Where(a => a.PercentualAlvo > 0 && Enum.IsDefined(typeof(TipoInvestimento), a.Tipo))
            .GroupBy(a => a.Tipo)   // evita duplicar a mesma classe
            .Select(g => AlocacaoAlvo.Criar(currentUser.UserId, (TipoInvestimento)g.Key, g.Last().PercentualAlvo))
            .ToList();

        if (novos.Count > 0)
            await repository.AddRangeAsync(novos, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
