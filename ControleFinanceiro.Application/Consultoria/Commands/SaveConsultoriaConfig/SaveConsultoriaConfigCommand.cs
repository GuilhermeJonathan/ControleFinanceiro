using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Repositories;
using MediatR;

namespace ControleFinanceiro.Application.Consultoria.Commands.SaveConsultoriaConfig;

public record SaveConsultoriaConfigCommand(
    string NomeConsultoria,
    string? LogoBase64,
    string? CorMarca,
    string? WhatsApp,
    string? MensagemRodape) : IRequest;

public class SaveConsultoriaConfigCommandHandler(
    IConsultoriaConfigRepository repository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : IRequestHandler<SaveConsultoriaConfigCommand>
{
    public async Task Handle(SaveConsultoriaConfigCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAssessor)
            throw new UnauthorizedAccessException("Apenas assessores podem configurar a consultoria.");

        var existing = await repository.GetByUsuarioAsync(currentUser.RealUserId, cancellationToken);

        if (existing is null)
        {
            var config = new ConsultoriaConfig(
                currentUser.RealUserId, request.NomeConsultoria,
                request.LogoBase64, request.CorMarca, request.WhatsApp, request.MensagemRodape);
            await repository.AddAsync(config, cancellationToken);
        }
        else
        {
            existing.Atualizar(request.NomeConsultoria, request.LogoBase64, request.CorMarca, request.WhatsApp, request.MensagemRodape);
            repository.Update(existing);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
