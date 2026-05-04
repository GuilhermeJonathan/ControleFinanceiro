using Login.Application.Common.Interfaces;
using Login.Domain.Common;
using Login.Domain.Entities;
using Login.Domain.Repositories;
using MediatR;

namespace Login.Application.Payments.Commands.CreateCheckout;

public class CreateCheckoutCommandHandler : IRequestHandler<CreateCheckoutCommand, CreateCheckoutResult>
{
    private readonly IMercadoPagoService _mp;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IUserAccessor _userAccessor;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCheckoutCommandHandler(
        IMercadoPagoService mp,
        ISubscriptionRepository subscriptionRepository,
        IUserAccessor userAccessor,
        IUnitOfWork unitOfWork)
    {
        _mp = mp;
        _subscriptionRepository = subscriptionRepository;
        _userAccessor = userAccessor;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateCheckoutResult> Handle(
        CreateCheckoutCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _userAccessor.UserId;
        // Usa o email MP fornecido pelo usuário; se não informado, usa o email da conta
        var email  = !string.IsNullOrWhiteSpace(request.PayerEmail)
            ? request.PayerEmail.Trim()
            : _userAccessor.Email;

        // Cria assinatura no MP e obtém o link de checkout
        var mpResult = await _mp.CreateSubscriptionAsync(
            email, userId, request.PlanId.ToLower(), cancellationToken);

        // Persiste o registro local para rastrear via webhook
        var planType = request.PlanId.ToLower() == "anual"
            ? PlanType.Annual
            : PlanType.Monthly;

        var subscription = new MercadoPagoSubscription(
            Guid.NewGuid(),
            userId,
            mpResult.SubscriptionId,
            planType);

        await _subscriptionRepository.AddAsync(subscription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateCheckoutResult(mpResult.CheckoutUrl);
    }
}
