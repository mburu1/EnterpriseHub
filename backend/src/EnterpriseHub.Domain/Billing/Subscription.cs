using EnterpriseHub.Domain.Common;

namespace EnterpriseHub.Domain.Billing;

public sealed class Subscription : AggregateRoot<Guid>, ITenantScoped
{
    public Guid TenantId { get; private set; }
    public Guid PlanId { get; private set; }
    public SubscriptionStatus Status { get; private set; } = SubscriptionStatus.Trialing;
    public DateTimeOffset CurrentPeriodEnd { get; private set; }

    private Subscription() { }

    public static Subscription Create(Guid tenantId, Guid planId, DateTimeOffset currentPeriodEnd) => new()
    {
        Id = Guid.NewGuid(),
        TenantId = tenantId,
        PlanId = planId,
        CurrentPeriodEnd = currentPeriodEnd
    };

    public void Activate()
    {
        Status = SubscriptionStatus.Active;
        Touch();
    }

    public void Cancel()
    {
        Status = SubscriptionStatus.Canceled;
        Touch();
    }

    public void Renew(DateTimeOffset newPeriodEnd)
    {
        if (Status == SubscriptionStatus.Canceled)
            throw new DomainException("Cannot renew a canceled subscription.");
        CurrentPeriodEnd = newPeriodEnd;
        Touch();
    }
}
