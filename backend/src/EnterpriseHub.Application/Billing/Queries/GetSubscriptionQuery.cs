namespace EnterpriseHub.Application.Billing.Queries;

using EnterpriseHub.Application.Billing.Dtos;
using EnterpriseHub.Application.Common.Interfaces;
using EnterpriseHub.Application.Common.Messaging;
using EnterpriseHub.Domain.Billing;
using EnterpriseHub.Domain.Common;

public sealed record GetSubscriptionQuery : IQuery<SubscriptionDto?>;

public sealed class GetSubscriptionQueryHandler(ISubscriptionRepository subscriptionRepository, ICurrentUserService currentUser)
    : IQueryHandler<GetSubscriptionQuery, SubscriptionDto?>
{
    public async Task<SubscriptionDto?> Handle(GetSubscriptionQuery query, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId ?? throw new ForbiddenException("No tenant context on the current user.");
        var subscription = await subscriptionRepository.GetByTenantIdAsync(tenantId, ct);
        return subscription?.ToDto();
    }
}
