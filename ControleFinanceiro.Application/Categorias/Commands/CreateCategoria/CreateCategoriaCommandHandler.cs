using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Categorias.Commands.CreateCategoria;

public class CreateCategoriaCommandHandler(
    ICategoriaRepository repository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<CreateCategoriaCommand, Guid>
{
    public async Task<Guid> Handle(CreateCategoriaCommand request, CancellationToken cancellationToken)
    {
        var categoria = new Categoria(request.Nome, request.Tipo, currentUser.UserId, request.Icone, request.Cor);
        await repository.AddAsync(categoria, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return categoria.Id;
    }
}
