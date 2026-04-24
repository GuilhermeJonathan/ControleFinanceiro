using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Categorias.Commands.DeleteCategoria;

public class DeleteCategoriaCommandHandler(ICategoriaRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteCategoriaCommand>
{
    public async Task Handle(DeleteCategoriaCommand request, CancellationToken cancellationToken)
    {
        var categoria = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Categoria {request.Id} não encontrada.");

        repository.Delete(categoria);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
