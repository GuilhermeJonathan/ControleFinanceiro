using ControleFinanceiro.Application.Assessoria.Commands.GerarConviteAssessoria;
using ControleFinanceiro.Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace ControleFinanceiro.Application.Assessoria.Commands.EnviarConviteEmail;

/// <summary>
/// Gera um código de convite (mesmas regras/limites do GerarConvite) e envia
/// por e-mail ao endereço informado. Retorna o código gerado.
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
    IEmailService emailService)
    : IRequestHandler<EnviarConviteEmailCommand, string>
{
    public async Task<string> Handle(EnviarConviteEmailCommand request, CancellationToken cancellationToken)
    {
        // Reusa toda a validação (perfil assessor, limite de carteira) do GerarConvite
        var codigo = await mediator.Send(new GerarConviteAssessoriaCommand(), cancellationToken);

        var nomeAssessor = currentUser.RealUserName ?? "Seu assessor";
        var body = $"""
            <div style="font-family:sans-serif;max-width:560px;margin:0 auto;background:#0f1117;color:#e2e8f0;border-radius:12px;overflow:hidden">
              <div style="background:#0f1117;padding:0;border-bottom:2px solid #16a34a">
                <a href="https://app.findog.com.br" style="display:block;line-height:0">
                  <img src="https://app.findog.com.br/og-image.png" alt="Meu FinDog" width="560"
                       style="display:block;width:100%;max-width:560px;height:auto;border:0" />
                </a>
              </div>
              <div style="padding:32px 24px">
                <p style="font-size:18px;font-weight:700;color:#f1f5f9">Você recebeu um convite! 👔</p>
                <p style="color:#94a3b8;line-height:1.6">
                  <strong style="color:#e2e8f0">{nomeAssessor}</strong> quer ser seu assessor financeiro
                  no Meu FinDog. Ele terá acesso de leitura às suas finanças — nunca poderá alterar nada —
                  e você pode revogar o acesso a qualquer momento.
                </p>
                <div style="background:#1e293b;border-radius:10px;padding:20px;margin:20px 0;text-align:center">
                  <p style="color:#94a3b8;font-size:13px;margin:0 0 8px">Seu código de convite</p>
                  <p style="color:#4ade80;font-size:32px;font-weight:800;letter-spacing:8px;margin:0">{codigo}</p>
                </div>
                <p style="color:#94a3b8;line-height:1.6;font-size:14px">
                  Entre no app, abra a tela <strong style="color:#e2e8f0">"Meu Assessor"</strong> e informe o código.
                  Ainda não tem conta? Cadastre-se grátis e ganhe 30 dias de trial.
                </p>
                <div style="text-align:center;margin:28px 0">
                  <a href="https://app.findog.com.br"
                     style="background:#16a34a;color:#fff;text-decoration:none;padding:14px 32px;border-radius:10px;font-weight:700;font-size:15px;display:inline-block">
                    Abrir o Meu FinDog 🐶
                  </a>
                </div>
                <p style="color:#64748b;font-size:12px;text-align:center;margin-top:24px">
                  Meu FinDog · <a href="https://app.findog.com.br" style="color:#64748b">app.findog.com.br</a>
                </p>
              </div>
            </div>
            """;

        await emailService.SendAsync(
            request.Email, request.Email,
            $"👔 {nomeAssessor} convidou você para o Meu FinDog", body, cancellationToken);

        return codigo;
    }
}
