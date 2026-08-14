namespace EnterpriseHub.Application.Billing.Commands;

using EnterpriseHub.Application.Billing.Dtos;
using EnterpriseHub.Application.Common.Interfaces;
using EnterpriseHub.Application.Common.Messaging;
using EnterpriseHub.Domain.Billing;
using EnterpriseHub.Domain.Common;

public sealed record SubscribeCommand(Guid PlanId) : ICommand<SubscriptionDto>;

public sealed class SubscribeCommandHandler(
    IPlanRepository planRepository,
    ISubscriptionRepository subscriptionRepository,
    ICurrentUserService currentUser,
    IBillingUnitOfWork unitOfWork)
    : ICommandHandler<SubscribeCommand, SubscriptionDto>
{
    public async Task<SubscriptionDto> Handle(SubscribeCommand command, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId ?? throw new ForbiddenException("No tenant context on the current user.");

        var plan = await planRepository.GetByIdAsync(command.PlanId, ct)
            ?? throw new DomainException("Plan not found.");

        var existing = await subscriptionRepository.GetByTenantIdAsync(tenantId, ct);
        if (existing is { Status: not SubscriptionStatus.Canceled })
            throw new DomainException("This organization already has an active subscription. Cancel it before subscribing to a different plan.");

        var periodEnd = plan.Interval == BillingInterval.Yearly
            ? DateTimeOffset.UtcNow.AddYears(1)
            : DateTimeOffset.UtcNow.AddMonths(1);

        var subscription = Subscription.Create(tenantId, plan.Id, periodEnd);
        subscription.Activate();

        await subscriptionRepository.AddAsync(subscription, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return subscription.ToDto();
    }
}
