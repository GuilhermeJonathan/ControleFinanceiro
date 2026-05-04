using Login.Application.Common.Interfaces;
using Login.Domain.Common;
using Login.Domain.Entities;
using Login.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Login.Application.Payments.Commands.ProcessWebhook;

public class ProcessWebhookCommandHandler : IRequestHandler<ProcessWebhookCommand>
{
    private readonly IMercadoPagoService _mp;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPaymentTransactionRepository _paymentTransactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ProcessWebhookCommandHandler> _logger;

    private static readonly Dictionary<PlanType, decimal> PlanAmounts = new()
    {
        { PlanType.Monthly, 4.90m },
        { PlanType.Annual,  39.90m },
    };

    public ProcessWebhookCommandHandler(
        IMercadoPagoService mp,
        ISubscriptionRepository subscriptionRepository,
        IUserRepository userRepository,
        IPaymentTransactionRepository paymentTransactionRepository,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<ProcessWebhookCommandHandler> logger)
    {
        _mp = mp;
        _subscriptionRepository = subscriptionRepository;
        _userRepository = userRepository;
        _paymentTransactionRepository = paymentTransactionRepository;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task Handle(ProcessWebhookCommand request, CancellationToken cancellationToken)
    {
        // Apenas assinaturas e pagamentos são processados
        if (request.Type is not ("subscription_preapproval" or "payment"))
        {
            _logger.LogDebug("Webhook ignorado: type={Type}", request.Type);
            return;
        }

        // Para type=payment, o DataId é o ID do pagamento; para subscription, é o ID da assinatura
        var mpSubscriptionId = request.Type == "payment"
            ? await ResolveSubscriptionIdFromPayment(request.DataId, cancellationToken)
            : request.DataId;

        if (mpSubscriptionId is null)
        {
            _logger.LogWarning("Não foi possível resolver o ID de assinatura para DataId={DataId}", request.DataId);
            return;
        }

        // Busca detalhes atualizados da assinatura no MP
        var detail = await _mp.GetSubscriptionAsync(mpSubscriptionId, cancellationToken);
        if (detail is null)
        {
            _logger.LogWarning("Assinatura {Id} não encontrada no MP.", mpSubscriptionId);
            return;
        }

        _logger.LogInformation("Webhook MP: subscription={Id} status={Status}", detail.Id, detail.Status);

        // Busca o registro local
        var subscription = await _subscriptionRepository.GetByMpIdAsync(mpSubscriptionId, cancellationToken);
        if (subscription is null)
        {
            _logger.LogWarning("Assinatura {Id} não encontrada localmente.", mpSubscriptionId);
            return;
        }

        switch (detail.Status)
        {
            case "authorized":
                await ActivatePlanAsync(subscription, detail.LastPaymentId, cancellationToken);
                break;

            case "cancelled":
                subscription.Cancel();
                _subscriptionRepository.Update(subscription);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Assinatura {Id} cancelada.", mpSubscriptionId);
                break;

            case "paused":
                subscription.Pause();
                _subscriptionRepository.Update(subscription);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                break;

            default:
                _logger.LogDebug("Status {Status} não requer ação.", detail.Status);
                break;
        }
    }

    private async Task ActivatePlanAsync(
        MercadoPagoSubscription subscription,
        string? paymentId,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(subscription.UserId, cancellationToken);
        if (user is null)
        {
            _logger.LogError("Usuário {UserId} não encontrado ao ativar plano.", subscription.UserId);
            return;
        }

        var expiresAt = subscription.PlanType == PlanType.Annual
            ? DateTime.UtcNow.AddDays(365)
            : DateTime.UtcNow.AddDays(30);

        user.SetPlan(subscription.PlanType, expiresAt);
        _userRepository.Update(user);

        subscription.Authorize(paymentId);
        _subscriptionRepository.Update(subscription);

        // Salva a transação de pagamento
        var amount = PlanAmounts.TryGetValue(subscription.PlanType, out var a) ? a : 0m;
        var transaction = new PaymentTransaction(
            id: Guid.NewGuid(),
            userId: user.Id,
            userName: user.Name,
            userEmail: user.Email,
            planType: subscription.PlanType,
            amount: amount,
            status: "authorized",
            mpPaymentId: paymentId,
            mpSubscriptionId: subscription.MpSubscriptionId,
            paidAt: DateTime.UtcNow);

        await _paymentTransactionRepository.AddAsync(transaction, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Plano {PlanType} ativado para usuário {UserId}. Expira em {ExpiresAt:yyyy-MM-dd}.",
            subscription.PlanType, subscription.UserId, expiresAt);

        // Envia e-mail para o cliente (erros não fatais)
        try
        {
            var planLabel = subscription.PlanType == PlanType.Annual ? "Anual" : "Mensal";
            var customerSubject = "✅ Sua assinatura foi ativada!";
            var customerBody = $@"
<p>Olá, <strong>{user.Name}</strong>!</p>
<p>Sua assinatura <strong>{planLabel}</strong> foi ativada com sucesso.</p>
<ul>
  <li><strong>Plano:</strong> {planLabel}</li>
  <li><strong>Valor:</strong> R${amount:F2}</li>
  <li><strong>Válida até:</strong> {expiresAt:dd/MM/yyyy}</li>
</ul>
<p>Obrigado por assinar o Findog!</p>";

            await _emailService.SendAsync(user.Email, user.Name, customerSubject, customerBody, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao enviar e-mail de ativação para {Email}.", user.Email);
        }

        // Envia e-mail para o admin (erros não fatais)
        try
        {
            var adminEmail = _configuration["AdminEmail"] ?? "admin@findog.com.br";
            var planLabel = subscription.PlanType == PlanType.Annual ? "Anual" : "Mensal";
            var adminSubject = $"🎉 Nova assinatura — {user.Name}";
            var adminBody = $@"
<p>Nova assinatura registrada:</p>
<ul>
  <li><strong>Nome:</strong> {user.Name}</li>
  <li><strong>E-mail:</strong> {user.Email}</li>
  <li><strong>Plano:</strong> {planLabel}</li>
  <li><strong>Valor:</strong> R${amount:F2}</li>
  <li><strong>Data:</strong> {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC</li>
</ul>";

            await _emailService.SendAsync(adminEmail, "Admin", adminSubject, adminBody, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao enviar e-mail de notificação ao admin.");
        }
    }

    /// <summary>
    /// Quando o webhook é do tipo "payment", busca a assinatura associada pelo external_reference.
    /// O external_reference é o userId que usamos para localizar a assinatura ativa.
    /// </summary>
    private async Task<string?> ResolveSubscriptionIdFromPayment(
        string paymentId,
        CancellationToken cancellationToken)
    {
        // Busca o pagamento no MP para pegar o preapproval_id
        // O MercadoPagoService implementa este lookup internamente
        var detail = await _mp.GetSubscriptionAsync(paymentId, cancellationToken);
        return detail?.Id;
    }
}
