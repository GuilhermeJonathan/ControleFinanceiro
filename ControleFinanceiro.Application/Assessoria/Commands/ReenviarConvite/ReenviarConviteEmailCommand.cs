using ControleFinanceiro.Application.Common.Email;
using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Common;
using ControleFinanceiro.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace ControleFinanceiro.Application.Assessoria.Commands.ReenviarConvite;

/// <summary>Reenvia o e-mail de um convite de cliente pendente (renova a validade).</summary>
public record ReenviarConviteEmailCommand(Guid VinculoId) : IRequest;

public class ReenviarConviteEmailCommandHandler(
    IVinculoAssessoriaRepository repository,
    ICurrentUser currentUser,
    IEmailService emailService,
    IConsultoriaConfigRepository consultoriaRepo,
    IConfiguration configuration,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ReenviarConviteEmailCommand>
{
    public async Task Handle(ReenviarConviteEmailCommand request, CancellationToken cancellationToken)
    {
        var vinculo = await repository.GetByIdAsync(request.VinculoId, cancellationToken)
            ?? throw new KeyNotFoundException("Convite não encontrado.");
        if (vinculo.AssessorId != currentUser.RealUserId)
            throw new UnauthorizedAccessException("Acesso negado.");
        if (string.IsNullOrWhiteSpace(vinculo.EmailConvidado))
            throw new InvalidOperationException("Este convite não foi enviado por e-mail.");

        vinculo.RenovarValidade();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var consultoria = await consultoriaRepo.GetByUsuarioAsync(currentUser.RealUserId, cancellationToken);
        var marca = consultoria?.NomeConsultoria is { Length: > 0 } n ? n : (currentUser.RealUserName ?? "Seu assessor");
        var cor = consultoria?.CorMarca is { Length: > 0 } c ? c : "#16a34a";
        var logo = ConviteEmailBuilder.LogoUrl(configuration, currentUser.RealUserId, !string.IsNullOrWhiteSpace(consultoria?.LogoBase64));
        var link = ConviteEmailBuilder.MontarLink(configuration, vinculo.CodigoConvite, "cliente");
        var body = ConviteEmailBuilder.CorpoCliente(marca, cor, logo, vinculo.CodigoConvite, link);

        await emailService.SendAsync(
            vinculo.EmailConvidado!, vinculo.EmailConvidado!,
            $"{marca} convidou você para acompanhar seu patrimônio", body, cancellationToken, marca);
    }
}
