using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Enums;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Categorias.Commands.UpdateCategoria;

public record UpdateCategoriaCommand(
    Guid Id,
    string Nome,
    TipoLancamento Tipo,
    string? Icone,
    string? Cor) : IRequest;

public class UpdateCategoriaCommandHandler(
    ICategoriaRepository repository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateCategoriaCommand>
{
    public async Task Handle(UpdateCategoriaCommand request, CancellationToken cancellationToken)
    {
        var categoria = await repository.GetByIdAsync(request.Id, currentUser.UserId, cancellationToken)
            ?? throw new KeyNotFoundException($"Categoria {request.Id} não encontrada.");
        categoria.Update(request.Nome, request.Tipo, request.Icone, request.Cor);
        repository.Update(categoria);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
