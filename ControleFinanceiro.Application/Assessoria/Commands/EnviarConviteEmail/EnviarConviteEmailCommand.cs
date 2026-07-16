using ControleFinanceiro.Application.Assessoria.Commands.GerarConviteAssessoria;
using ControleFinanceiro.Application.Common.Email;
using ControleFinanceiro.Application.Common.Interfaces;
using ControleFinanceiro.Domain.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace ControleFinanceiro.Application.Assessoria.Commands.EnviarConviteEmail;

/// <summary>
/// Gera um código de convite (mesmas regras/limites do GerarConvite), grava o e-mail
/// do convidado e envia o convite com um link para /aceitar. Retorna o código gerado.
/// </summary>
public record EnviarConviteEmailCommand(string Email) : IRequest<string>;

public class EnviarConviteEmailCommandValidator : AbstractValidator<EnviarConviteEmailCommand>
{
    public EnviarConviteEmailCommandValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório.")
            .EmailAddress().WithMessage("E-mail inválido.");
    }
}

public class EnviarConviteEmailCommandHandler(
    ISender mediator,
    ICurrentUser currentUser,
    IEmailService emailService,
    IConsultoriaConfigRepository consultoriaRepo,
    IConfiguration configuration)
    : IRequestHandler<EnviarConviteEmailCommand, string>
{
    public async Task<string> Handle(EnviarConviteEmailCommand request, CancellationToken cancellationToken)
    {
        // Reusa toda a validação (perfil assessor, limite de carteira) do GerarConvite
        // e já grava o e-mail do convidado no vínculo.
        var codigo = await mediator.Send(new GerarConviteAssessoriaCommand(request.Email), cancellationToken);

        var consultoria = await consultoriaRepo.GetByUsuarioAsync(currentUser.RealUserId, cancellationToken);
        var marca = consultoria?.NomeConsultoria is { Length: > 0 } n ? n : (currentUser.RealUserName ?? "Seu assessor");
        var cor = consultoria?.CorMarca is { Length: > 0 } c ? c : "#16a34a";

        var logo = ConviteEmailBuilder.LogoUrl(configuration, currentUser.RealUserId, !string.IsNullOrWhiteSpace(consultoria?.LogoBase64));
        var link = ConviteEmailBuilder.MontarLink(configuration, codigo, "cliente");
        var body = ConviteEmailBuilder.CorpoCliente(marca, cor, logo, codigo, link);

        await emailService.SendAsync(
            request.Email, request.Email,
            $"{marca} convidou você para acompanhar seu patrimônio", body, cancellationToken);

        return codigo;
    }
}
