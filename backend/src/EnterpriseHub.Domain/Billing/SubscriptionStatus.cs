namespace EnterpriseHub.Domain.Billing;

public enum SubscriptionStatus
{
    Trialing = 0,
    Active = 1,
    PastDue = 2,
    Canceled = 3
}
