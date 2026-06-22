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

    private static string BuildActivationEmail(string nome, string label, string validoAte) => $"""
        <div style="font-family:sans-serif;max-width:560px;margin:0 auto;background:#0f1117;color:#e2e8f0;border-radius:12px;overflow:hidden">
          <div style="background:#0f1117;padding:0;border-bottom:2px solid #16a34a">
            <a href="https://app.findog.com.br" style="display:block;line-height:0">
              <img src="https://app.findog.com.br/og-image.png" alt="Meu FinDog" width="560"
                   style="display:block;width:100%;max-width:560px;height:auto;border:0" />
            </a>
          </div>
          <div style="padding:32px 24px">
            <p style="font-size:18px;font-weight:700;color:#f1f5f9">Olá, {nome}! 🎉</p>
            <p style="color:#94a3b8;line-height:1.6">
              Seu plano foi ativado pela equipe Findog. Bem-vindo(a)!
            </p>
            <div style="background:#1e293b;border-radius:10px;padding:20px;margin:20px 0">
              <p style="color:#94a3b8;font-size:13px;margin:0 0 12px;text-transform:uppercase;letter-spacing:.05em">Detalhes do plano</p>
              <div style="display:flex;justify-content:space-between;margin-bottom:8px">
                <span style="color:#94a3b8;font-size:14px">Plano</span>
                <span style="color:#e2e8f0;font-weight:700;font-size:14px">{label}</span>
              </div>
              <div style="display:flex;justify-content:space-between">
                <span style="color:#94a3b8;font-size:14px">Válido até</span>
                <span style="color:#4ade80;font-weight:700;font-size:14px">{validoAte}</span>
              </div>
            </div>
            <div style="background:#1e293b;border-radius:10px;padding:16px;margin:20px 0">
              <p style="color:#94a3b8;font-size:13px;margin:0 0 10px">O que você tem acesso:</p>
              <p style="margin:6px 0;color:#e2e8f0;font-size:14px">✅ Lançamentos ilimitados</p>
              <p style="margin:6px 0;color:#e2e8f0;font-size:14px">✅ Integração com WhatsApp</p>
              <p style="margin:6px 0;color:#e2e8f0;font-size:14px">✅ Relatórios e gráficos</p>
              <p style="margin:6px 0;color:#e2e8f0;font-size:14px">✅ Metas financeiras</p>
            </div>
            <div style="text-align:center;margin:28px 0">
              <a href="https://app.findog.com.br"
                 style="background:#16a34a;color:#fff;text-decoration:none;padding:14px 32px;border-radius:10px;font-weight:700;font-size:15px;display:inline-block">
                Acessar o Meu FinDog 🐶
              </a>
            </div>
            <p style="color:#64748b;font-size:12px;text-align:center;margin-top:24px">
              Meu FinDog · <a href="https://app.findog.com.br" style="color:#64748b">app.findog.com.br</a>
            </p>
          </div>
        </div>
        """;
}
