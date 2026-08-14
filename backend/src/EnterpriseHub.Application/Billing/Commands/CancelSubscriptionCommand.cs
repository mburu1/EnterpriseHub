namespace EnterpriseHub.Application.Billing.Commands;

using EnterpriseHub.Application.Billing.Dtos;
using EnterpriseHub.Application.Common.Interfaces;
using EnterpriseHub.Application.Common.Messaging;
using EnterpriseHub.Domain.Billing;
using EnterpriseHub.Domain.Common;

public sealed record CancelSubscriptionCommand : ICommand<SubscriptionDto>;

public sealed class CancelSubscriptionCommandHandler(
    ISubscriptionRepository subscriptionRepository,
    ICurrentUserService currentUser,
    IBillingUnitOfWork unitOfWork)
    : ICommandHandler<CancelSubscriptionCommand, SubscriptionDto>
{
    public async Task<SubscriptionDto> Handle(CancelSubscriptionCommand command, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId ?? throw new ForbiddenException("No tenant context on the current user.");

        var subscription = await subscriptionRepository.GetByTenantIdAsync(tenantId, ct)
            ?? throw new DomainException("No subscription found for this organization.");

        subscription.Cancel();
        subscriptionRepository.Update(subscription);
        await unitOfWork.SaveChangesAsync(ct);

        return subscription.ToDto();
    }
}
