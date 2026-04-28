using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Categorias.Commands.AtualizarLimiteCategoria;

public class AtualizarLimiteCategoriaCommandHandler(
    ICategoriaRepository repository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<AtualizarLimiteCategoriaCommand>
{
    public async Task Handle(AtualizarLimiteCategoriaCommand request, CancellationToken cancellationToken)
    {
        var categoria = await repository.GetByIdAsync(request.Id, currentUser.UserId, cancellationToken)
            ?? throw new KeyNotFoundException($"Categoria {request.Id} não encontrada.");

        categoria.AtualizarLimite(request.LimiteMensal);
        repository.Update(categoria);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
