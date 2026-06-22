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

        var body = $@"
<p>Olá, <strong>{user.Name}</strong>!</p>
<p>Seu plano foi ativado pela equipe Findog.</p>
<ul>
  <li><strong>Plano:</strong> {label}</li>
  <li><strong>Válido até:</strong> {expiresStr}</li>
</ul>
<p>Bom uso do Findog! 🐶</p>";

        await _emailService.SendAsync(user.Email, user.Name, "✅ Seu plano foi ativado!", body, cancellationToken);
    }
}
