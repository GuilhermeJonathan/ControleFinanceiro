using Login.Application.Common.Email;
using Login.Application.Common.Interfaces;
using Login.Domain.Common;
using Login.Domain.Entities;
using Login.Domain.Repositories;
using MediatR;

namespace Login.Application.Users.Commands.SetPlan;

public class SetPlanCommandHandler : IRequestHandler<SetPlanCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;

    public SetPlanCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork, IEmailService emailService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }

    public async Task Handle(SetPlanCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Usuário {request.Id} não encontrado.");

        var planType = (PlanType)request.PlanType;

        switch (planType)
        {
            case PlanType.None:
                user.AdminClearPlan();
                break;

            case PlanType.Trial:
                var days = request.TrialDays ?? 30;
                user.AdminSetTrial(days);
                break;

            case PlanType.Monthly:
                user.SetPlan(PlanType.Monthly, DateTime.UtcNow.AddDays(30));
                break;

            case PlanType.Annual:
                user.SetPlan(PlanType.Annual, DateTime.UtcNow.AddDays(365));
                break;

            case PlanType.Assessor:
                user.SetPlan(PlanType.Assessor, DateTime.UtcNow.AddDays(30));
                break;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (planType != PlanType.None)
            await SendActivationEmailAsync(user, planType, cancellationToken);
    }

    private async Task SendActivationEmailAsync(User user, PlanType planType, CancellationToken cancellationToken)
    {
        var label = planType switch
        {
            PlanType.Trial   => "Trial",
            PlanType.Monthly => "Mensal",
            PlanType.Annual  => "Anual",
            _                => planType.ToString()
        };

        var expiresStr = user.PlanExpiresAt.HasValue
            ? user.PlanExpiresAt.Value.ToString("dd/MM/yyyy")
            : "—";

        await _emailService.SendAsync(
            user.Email, user.Name,
            "✅ Seu plano foi ativado — Meu FinDog",
            BuildActivationEmail(user.Name, label, expiresStr),
            cancellationToken);
    }

    private static string BuildActivationEmail(string nome, string label, string validoAte) =>
        EmailTemplateBuilder.Wrap(
            EmailTemplateBuilder.Greeting($"Olá, {nome}! 🎉") +
            EmailTemplateBuilder.Paragraph("Seu plano foi ativado pela equipe Findog. Bem-vindo(a)!") +
            EmailTemplateBuilder.Card(
                """<p style="color:#94a3b8;font-size:13px;margin:0 0 12px;text-transform:uppercase;letter-spacing:.05em">Detalhes do plano</p>""" +
                EmailTemplateBuilder.CardRow("Plano", label) +
                EmailTemplateBuilder.CardRow("Válido até", validoAte, "#4ade80")) +
            EmailTemplateBuilder.Card("""
                <p style="color:#94a3b8;font-size:13px;margin:0 0 10px">O que você tem acesso:</p>
                <p style="margin:6px 0;color:#e2e8f0;font-size:14px">✅ Lançamentos ilimitados</p>
                <p style="margin:6px 0;color:#e2e8f0;font-size:14px">✅ Integração com WhatsApp</p>
                <p style="margin:6px 0;color:#e2e8f0;font-size:14px">✅ Relatórios e gráficos</p>
                <p style="margin:6px 0;color:#e2e8f0;font-size:14px">✅ Metas financeiras</p>
                """) +
            EmailTemplateBuilder.Button("Acessar o Meu FinDog 🐶", EmailTemplateBuilder.AppUrl));
}
